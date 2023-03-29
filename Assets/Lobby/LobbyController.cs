using System;
using System.Collections.Generic;
using System.Linq;
using Lobby.SignalRWrapper;
using PlayFab;
using PlayFab.MultiplayerModels;
using UnityEngine;

namespace Lobby
{
    public class LobbyController : MonoBehaviour
    {
        [SerializeField] private bool debug;

        private static EntityKey LocalEntityKey => new()
        {
            Id = PlayFabSettings.staticPlayer.EntityId,
            Type = PlayFabSettings.staticPlayer.EntityType
        };

        private static Member LocalMember => new()
        {
            MemberEntity = LocalEntityKey
        };

        public List<LobbyMember> LobbyMembers { get; private set; } = new();
        public Dictionary<string, string> LobbyData { get; private set; } = new();
        public EntityKey LobbyOwner { get; private set; }
        public string LobbyId { get; private set; }
        public string ConnectionString { get; private set; }

        public event Action OnLobbyCreated;
        public event Action OnLobbyJoined;
        public event Action OnLobbyLeft;

        public event Action<LobbyMember> OnLobbyMemberAdded;
        public event Action<LobbyMember> OnLobbyMemberRemoved;
        public event Action<EntityKey> OnLobbyOwnerChanged;
        public event Action<Dictionary<string, string>> OnLobbyDataChanged;
        public event Action<LobbyMember> OnLobbyMemberDataChanged;

        public bool IsOwner => InLobby && LobbyOwner.Id == LocalEntityKey.Id;
        public bool InLobby => !string.IsNullOrEmpty(LobbyId);


        private SignalRController _signalRController;
        private SignalRLobbyMessageHandler _signalRLobbyMessageHandler;

        private void Awake()
        {
            AccountManager.OnAuthenticated += () =>
            {
                _signalRLobbyMessageHandler = new SignalRLobbyMessageHandler(this, true);

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
                LobbyId = createLobbyResult.LobbyId;
                ConnectionString = createLobbyResult.ConnectionString;
                SubscribeToLobbyEvents(LobbyId);
                this.OnLobbyCreated?.Invoke();

                InitialiseLobby();
            }

            void OnLobbyCreationFailed(PlayFabError error)
            {
                Debug.LogError($"Lobby creation failed - {error.GenerateErrorReport()}");
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

                LobbyId = joinLobbyResult.LobbyId;
                SubscribeToLobbyEvents(joinLobbyResult.LobbyId);
                this.OnLobbyJoined?.Invoke();

                InitialiseLobby();
            }

            void OnLobbyJoinFailed(PlayFabError error)
            {
                Debug.LogError($"Lobby join failed - {error.GenerateErrorReport()}");
            }
        }

        public void LeaveLobby()
        {
            if (!InLobby)
                throw new Exception("Not in a lobby");
            
            PlayFabMultiplayerAPI.LeaveLobby(new LeaveLobbyRequest
            {
                LobbyId = LobbyId,
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

            Dispose();
            this.OnLobbyLeft?.Invoke();
        }

        private void Dispose()
        {
            _initialised = false;
            LobbyId = null;

            // TODO _signalRController.Dispose();
        }

        public void SetReady(bool isReady)
        {
            if (!InLobby)
                throw new Exception("Not in a lobby");

            if (IsOwner)
                throw new Exception("Lobby owner cannot set ready status");

            PlayFabMultiplayerAPI.UpdateLobby(new UpdateLobbyRequest
            {
                LobbyId = LobbyId,
                MemberEntity = LocalEntityKey,
                MemberData = new Dictionary<string, string>
                {
                    { "Ready", isReady.ToString() }
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

        public void StartGame()
        {
            if (!IsOwner)
                throw new Exception("Not a lobby owner");

            PlayFabMultiplayerAPI.UpdateLobby(new UpdateLobbyRequest
            {
                LobbyId = LobbyId,
                LobbyData = new Dictionary<string, string>
                {
                    { "GameStarted", "true" }
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

        public void KickMember(LobbyMember member, bool preventRejoin = false)
        {
            if (!IsOwner)
                throw new Exception("Not a lobby owner");

            PlayFabMultiplayerAPI.RemoveMember(new RemoveMemberFromLobbyRequest
            {
                LobbyId = LobbyId,
                MemberEntity = member.MemberEntity,
                PreventRejoin = preventRejoin
            }, OnLobbyUpdated, OnLobbyUpdateFailed);

            void OnLobbyUpdateFailed(PlayFabError obj)
            {
                Debug.LogError($"Kick member update failed - {obj.GenerateErrorReport()}");
            }

            void OnLobbyUpdated(LobbyEmptyResult obj)
            {
                Debug.Log("Kick member updated");
            }
        }

        private readonly Queue<LobbyChange> _queuedChanges = new();
        private bool _initialised;

        private void InitialiseLobby()
        {
            PlayFabMultiplayerAPI.GetLobby(new GetLobbyRequest
            {
                LobbyId = LobbyId,
            }, result =>
            {
                LobbyData = result.Lobby.LobbyData ?? new Dictionary<string, string>();
                LobbyMembers = result.Lobby.Members.Select(m => new LobbyMember(m)).ToList() ?? new List<LobbyMember>();
                LobbyOwner = result.Lobby.Owner ?? new EntityKey();

                OnLobbyDataChanged?.Invoke(LobbyData);
                foreach (var member in LobbyMembers)
                {
                    OnLobbyMemberAdded?.Invoke(member);
                    OnLobbyMemberDataChanged?.Invoke(member);
                }

                OnLobbyOwnerChanged?.Invoke(LobbyOwner);

                _initialised = true;
                ApplyQueuedChanges();
            }, error => { Debug.LogError($"Failed to get lobby data - {error.GenerateErrorReport()}"); });
        }

        private void ApplyQueuedChanges()
        {
            while (_queuedChanges.Count > 0)
            {
                var change = _queuedChanges.Dequeue();
                ApplyChange(change);
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

        public void ApplyChanges(List<LobbyChange> changes)
        {
            var sortedByChangeNumber = changes.OrderBy(x => x.ChangeNumber);

            foreach (var change in sortedByChangeNumber)
                ApplyChange(change);
        }

        private void ApplyChange(LobbyChange change)
        {
            if (!_initialised)
            {
                if (debug)
                    Debug.Log($"Enqueuing change {change.ChangeNumber} - {change.ChangeType}");

                _queuedChanges.Enqueue(change);
                return;
            }

            if (debug)
                Debug.Log($"Applying change {change.ChangeNumber} - {change.ChangeType}");

            switch (change.ChangeType)
            {
                case ChangeType.LobbyDataUpdate:
                    UpdateLobbyData(change.LobbyData);
                    break;
                case ChangeType.MemberAddedOrChanged:
                    UpdateMember(change.MemberToMerge);
                    break;
                case ChangeType.MemberRemoved:
                    RemoveMember(change.RemovedMember);
                    break;
                case ChangeType.OwnerChanged:
                    UpdateOwner(change.Owner);
                    break;
                case ChangeType.Ignore:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateOwner(EntityKey newOwner)
        {
            if (newOwner == null)
                throw new ArgumentNullException(nameof(newOwner));

            if (LobbyOwner == null || LobbyOwner.Id != newOwner.Id)
            {
                LobbyOwner = newOwner;
                OnLobbyOwnerChanged?.Invoke(newOwner);
            }
        }

        private void RemoveMember(LobbyMember removedMember)
        {
            if (removedMember == null)
                throw new ArgumentNullException(nameof(removedMember));

            if (LobbyMembers.Contains(removedMember))
            {
                LobbyMembers.Remove(removedMember);
                OnLobbyMemberRemoved?.Invoke(removedMember);
            }
        }

        private void UpdateMember(LobbyMember addedOrUpdatedMember)
        {
            if (addedOrUpdatedMember == null)
                throw new ArgumentNullException(nameof(addedOrUpdatedMember));

            if (LobbyMembers.Contains(addedOrUpdatedMember))
            {
                var member = LobbyMembers.First(x => x.MemberEntity.Id == addedOrUpdatedMember.MemberEntity.Id);
                if (member.MemberData == null || !member.MemberData.SequenceEqual(addedOrUpdatedMember.MemberData))
                {
                    member.MemberData = addedOrUpdatedMember.MemberData;
                    OnLobbyMemberDataChanged?.Invoke(addedOrUpdatedMember);
                }
                else
                {
                    Debug.LogWarning(
                        $"UpdateMember was called for entity {addedOrUpdatedMember.MemberEntity.Id} but member data is the same");
                }
            }
            else
            {
                LobbyMembers.Add(addedOrUpdatedMember);
                OnLobbyMemberAdded?.Invoke(addedOrUpdatedMember);
            }
        }

        private void UpdateLobbyData(Dictionary<string, string> newLobbyData)
        {
            if (newLobbyData == null)
                throw new ArgumentNullException(nameof(newLobbyData));

            if (LobbyData == null || !LobbyData.SequenceEqual(newLobbyData))
            {
                LobbyData = newLobbyData;
                OnLobbyDataChanged?.Invoke(newLobbyData);
            }
        }
    }
}