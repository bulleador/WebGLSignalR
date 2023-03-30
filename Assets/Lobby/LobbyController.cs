using System;
using System.Collections.Generic;
using Lobby.SignalRWrapper;
using PlayFab;
using PlayFab.MultiplayerModels;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lobby
{
    public class LobbyController : MonoBehaviour
    {
        public static EntityKey LocalEntityKey => new()
        {
            Id = PlayFabSettings.staticPlayer.EntityId,
            Type = PlayFabSettings.staticPlayer.EntityType
        };

        public static Member LocalMember => new()
        {
            MemberEntity = LocalEntityKey
        };

        public event Action<ObservableLobby, bool> OnLobbyJoined;
        public event Action<ObservableLobby, LobbyLeaveReason> OnLobbyLeft;

        public ObservableLobby Lobby { get; private set; }
        public bool IsOwner => InLobby && Lobby.LobbyOwner.Id == LocalEntityKey.Id;
        public bool InLobby => Lobby != null;

        private SignalRController _signalRController;
        private SignalRLobbyMessageHandler _signalRLobbyMessageHandler;

        private void Awake()
        {
            AccountManager.OnAuthenticated += () =>
            {
                _signalRLobbyMessageHandler = new SignalRLobbyMessageHandler(this, true);
                _signalRController = new SignalRController();

                _signalRController.AddMessageHandler("LobbyChange", _signalRLobbyMessageHandler.OnLobbyChangeMessage);
                _signalRController.AddSubscriptionChangeMessageHandler("LobbyChange",
                    _signalRLobbyMessageHandler.OnLobbySubscriptionChangeMessage);
                _signalRController.Initialise(null);
            };
        }

        public void CreateLobby()
        {
            if (InLobby)
                throw new Exception("Already in a lobby");

            Debug.Log("Creating lobby...");
            PlayFabMultiplayerAPI.CreateLobby(new CreateLobbyRequest
            {
                Owner = LocalEntityKey,
                Members = new List<Member> { LocalMember },
                AccessPolicy = AccessPolicy.Public,
                MaxPlayers = 4,
                OwnerMigrationPolicy = OwnerMigrationPolicy.Automatic,
                UseConnections = true
            }, OnLobbyCreated, OnLobbyCreationFailed);

            void OnLobbyCreated(CreateLobbyResult createLobbyResult)
            {
                Debug.Log(
                    $"Lobby created. Lobby ID: {createLobbyResult.LobbyId}. Lobby connection string: {createLobbyResult.ConnectionString}");
                Lobby = new ObservableLobby(createLobbyResult.LobbyId, createLobbyResult.ConnectionString,
                    _signalRController);

#if UNITY_EDITOR
                EditorGUIUtility.systemCopyBuffer = createLobbyResult.ConnectionString;
#endif

                Lobby.Initialise(() => { OnLobbyJoined?.Invoke(Lobby, true); },
                    () => throw new NotImplementedException());
            }

            void OnLobbyCreationFailed(PlayFabError error)
            {
                Debug.LogError($"Lobby creation failed - {error.GenerateErrorReport()}");
                throw new NotImplementedException("TODO - handle lobby creation failure");
            }
        }

        public void JoinLobby(string connectionString)
        {
            if (InLobby)
                throw new Exception("Already in a lobby");

            PlayFabMultiplayerAPI.JoinLobby(new JoinLobbyRequest
            {
                MemberEntity = LocalEntityKey,
                ConnectionString = connectionString
            }, OnLobbyJoined, OnLobbyJoinFailed);

            void OnLobbyJoined(JoinLobbyResult joinLobbyResult)
            {
                Debug.Log($"Lobby joined. Lobby ID: {joinLobbyResult.LobbyId}");
                Lobby = new ObservableLobby(joinLobbyResult.LobbyId, connectionString, _signalRController);
                Lobby.Initialise(() => { this.OnLobbyJoined?.Invoke(Lobby, false); },
                    () => throw new NotImplementedException());
            }

            void OnLobbyJoinFailed(PlayFabError error)
            {
                Debug.LogError($"Lobby join failed - {error.GenerateErrorReport()}");
                throw new NotImplementedException("TODO - handle lobby join failure");
            }
        }

        public void LeaveLobby()
        {
            if (!InLobby)
                throw new Exception("Not in a lobby");

            PlayFabMultiplayerAPI.LeaveLobby(new LeaveLobbyRequest
            {
                LobbyId = Lobby.LobbyId,
                MemberEntity = LocalEntityKey
            }, OnLobbyLeft, OnLobbyLeaveFailed);

            void OnLobbyLeft(LobbyEmptyResult lobbyEmptyResult)
            {
                Dispose();
                Debug.Log("Left lobby");
            }

            void OnLobbyLeaveFailed(PlayFabError error)
            {
                Dispose();
                Debug.LogError($"Lobby leave failed - {error.GenerateErrorReport()}");
            }

            this.OnLobbyLeft?.Invoke(Lobby, LobbyLeaveReason.MemberLeft);
        }

        private void Dispose()
        {
            Lobby = null;
            // TODO _signalRController.Dispose();
        }

        public void SetReady(bool isReady)
        {
            if (!InLobby)
                throw new Exception("Not in a lobby");

            Lobby.SetReady(isReady);
        }

        public void StartGame()
        {
            if (!InLobby)
                throw new Exception("Not in a lobby");

            Lobby.StartGame();
        }

        public void KickMember(Member member, bool preventRejoin = false)
        {
            if (!InLobby)
                throw new Exception("Not in a lobby");

            Lobby.KickMember(member, preventRejoin);
        }

        public void OnSubscriptionMessage(SubscriptionMessageType messageType)
        {
            Debug.Log($"Received subscription message - {messageType}");

            switch (messageType)
            {
                case SubscriptionMessageType.Subscribed:
                    break;
                case SubscriptionMessageType.UnsubscribedMemberLeft:
                    break;
                case SubscriptionMessageType.UnsubscribedMemberRemoved:
                {
                    OnLobbyLeft?.Invoke(Lobby, LobbyLeaveReason.MemberKicked);
                    Dispose();
                }
                    break;
                case SubscriptionMessageType.UnsubscribedLobbyDeleted:
                    if (InLobby && !Lobby.IsOwner)
                    {
                        OnLobbyLeft?.Invoke(Lobby, LobbyLeaveReason.LobbyClosed);
                        Dispose();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }
        }
    }

    public enum SubscriptionMessageType
    {
        Subscribed,
        UnsubscribedMemberLeft,
        UnsubscribedMemberRemoved,
        UnsubscribedLobbyDeleted,
    }

    public enum LobbyLeaveReason
    {
        MemberLeft,
        MemberKicked,
        LobbyClosed,
    }
}