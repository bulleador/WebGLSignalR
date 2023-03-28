using System.Collections.Generic;
using EasyButtons;
using Lobby.Signal;
using PlayFab;
using PlayFab.MultiplayerModels;
using UnityEngine;

namespace Lobby
{
    public class LobbyController : MonoBehaviour
    {
        private static EntityKey LocalEntityKey => new()
        {
            Id = PlayFabSettings.staticPlayer.EntityId,
            Type = PlayFabSettings.staticPlayer.EntityType
        };
        
        private static Member LocalMember => new()
        {
            MemberEntity = LocalEntityKey
        };

        private SignalRController _signalRController;
        private SignalRLobbyMessageHandler _signalRLobbyMessageHandler;
        private string _lobbyId;

        private void Awake()
        {
            AccountManager.OnAuthenticated += () =>
            {
                _signalRLobbyMessageHandler = new SignalRLobbyMessageHandler(this);

                _signalRController = GetComponent<SignalRController>();
                _signalRController.Initialise(s =>
                {
                    _signalRController.AddMessageHandler("LobbyChange",
                        _signalRLobbyMessageHandler.OnLobbyChangeMessage);
                    _signalRController.AddSubscriptionChangeMessageHandler("LobbyChange",
                        _signalRLobbyMessageHandler.OnLobbySubscriptionChangeMessage);
                });
            };
        }

        [Button]
        public void CreateLobby()
        {
            Debug.Log("Creating lobby...");
            
            PlayFabMultiplayerAPI.CreateLobby(new CreateLobbyRequest
            {
                Owner = LocalEntityKey,
                Members = new List<Member> {LocalMember},
                AccessPolicy = AccessPolicy.Public,
                MaxPlayers = 4,
                OwnerMigrationPolicy = OwnerMigrationPolicy.Automatic,
                UseConnections = true
            }, OnLobbyCreated, OnLobbyCreationFailed);
            
            void OnLobbyCreated(CreateLobbyResult createLobbyResult)
            {
                Debug.Log($"Lobby created. Lobby ID: {createLobbyResult.LobbyId}. Lobby connection string: {createLobbyResult.ConnectionString}");
                _lobbyId = createLobbyResult.LobbyId;
                SubscribeToLobbyEvents(_lobbyId);
            }
            
            void OnLobbyCreationFailed(PlayFabError error)
            {
                Debug.LogError($"Lobby creation failed - {error.GenerateErrorReport()}");
            }
        }

        [Button]
        public void JoinLobby(string connectionString)
        {
            PlayFabMultiplayerAPI.JoinLobby(new JoinLobbyRequest
            {
                MemberEntity = LocalEntityKey,
                ConnectionString = connectionString
            }, OnLobbyJoined, OnLobbyJoinFailed);
        
            void OnLobbyJoined(JoinLobbyResult joinLobbyResult)
            {
                Debug.Log($"Lobby joined. Lobby ID: {joinLobbyResult.LobbyId}");
            
                _lobbyId = joinLobbyResult.LobbyId;
                SubscribeToLobbyEvents(joinLobbyResult.LobbyId);
            }
        
            void OnLobbyJoinFailed(PlayFabError error)
            {
                Debug.LogError($"Lobby join failed - {error.GenerateErrorReport()}");
            }
        }

        [Button]
        public void LeaveLobby()
        {
            PlayFabMultiplayerAPI.LeaveLobby(new LeaveLobbyRequest()
            {
                LobbyId = _lobbyId,
                MemberEntity = LocalEntityKey
            }, OnLobbyLeft, OnLobbyLeaveFailed);
        
            void OnLobbyLeft(LobbyEmptyResult lobbyEmptyResult)
            {
                Debug.Log("Left lobby");
            }
        
            void OnLobbyLeaveFailed(PlayFabError error)
            {
                Debug.LogError($"Lobby leave failed - {error.GenerateErrorReport()}");
            }
        }

        [Button]
        public void SetReady(bool isReady)
        {
            PlayFabMultiplayerAPI.UpdateLobby(new UpdateLobbyRequest()
            {
                LobbyId = _lobbyId,
                MemberEntity = LocalEntityKey,
                MemberData = new Dictionary<string, string>
                {
                    {"Ready", isReady.ToString()}
                },
            }, OnLobbyUpdated, OnLobbyUpdateFailed);
            
            void OnLobbyUpdateFailed(PlayFabError obj)
            {
                Debug.LogError($"Set ready status update failed - {obj.GenerateErrorReport()}");
            }

            void OnLobbyUpdated(LobbyEmptyResult obj)
            {
                Debug.Log("Set ready updated");
            }
        }

        [Button]
        public void StartGame()
        {
            PlayFabMultiplayerAPI.UpdateLobby(new UpdateLobbyRequest()
            {
                LobbyId = _lobbyId,
                LobbyData = new Dictionary<string, string>
                {
                    {"GameStarted", "true"}
                }
            }, OnLobbyUpdated, OnLobbyUpdateFailed);
            
            void OnLobbyUpdateFailed(PlayFabError obj)
            {
                Debug.LogError($"Start game update failed - {obj.GenerateErrorReport()}");
            }
            
            void OnLobbyUpdated(LobbyEmptyResult obj)
            {
                Debug.Log("Start game updated");
            }
        }

        private void SubscribeToLobbyEvents(string lobbyId)
        {
            PlayFabMultiplayerAPI.SubscribeToLobbyResource(new SubscribeToLobbyResourceRequest
            {
                Type = SubscriptionType.LobbyChange,
                EntityKey = LocalEntityKey,
                ResourceId = lobbyId,
                SubscriptionVersion = 1,
                PubSubConnectionHandle = _signalRController.ConnectionHandle
            }, OnSubscribedToLobbyEvents, OnSubscriptionToLobbyEventsFailed);
            
            void OnSubscribedToLobbyEvents(SubscribeToLobbyResourceResult subscribeToLobbyResourceResult)
            {
                Debug.Log("Subscribed to lobby events");
            }
            
            void OnSubscriptionToLobbyEventsFailed(PlayFabError error)
            {
                Debug.LogError($"Subscription to lobby events failed - {error.GenerateErrorReport()}");
            }
        }
    }
}