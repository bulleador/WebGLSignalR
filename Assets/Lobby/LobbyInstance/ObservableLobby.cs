using System;
using System.Collections.Generic;
using System.Linq;
using Lobby.SignalRWrapper;
using PlayFab;
using PlayFab.MultiplayerModels;
using UnityEngine;

namespace Lobby.LobbyInstance
{
    public class ObservableLobby
    {
        private readonly SignalRController _signalRController;

        private readonly Queue<LobbyChange> _queuedChanges = new();
        private readonly PlayFab.MultiplayerModels.Lobby _lobby = new()
        {
            Members = new List<Member>(),
            LobbyData = new Dictionary<string, string>()
        };

        private bool _initialised;
        

        public string LobbyId => _lobby.LobbyId;
        public string ConnectionString => _lobby.ConnectionString;
        public Dictionary<string, string> LobbyData => _lobby.LobbyData;
        public List<Member> Members => _lobby.Members;
        public EntityKey LobbyOwner => _lobby.Owner;
        public MembershipLock MembershipLock => _lobby.MembershipLock;
        public AccessPolicy AccessPolicy => _lobby.AccessPolicy;
        public uint MaxPlayers => _lobby.MaxPlayers;

        public bool IsOwner => LobbyOwner.Id == PlayFabSettings.staticPlayer.EntityId;
        
        public event Action<Dictionary<string, string>> OnLobbyDataChanged;
        public event Action<Member> OnLobbyMemberAdded;
        public event Action<Member> OnLobbyMemberRemoved;
        public event Action<Member> OnLobbyMemberDataChanged;
        public event Action<EntityKey> OnLobbyOwnerChanged;
        public event Action<LobbyLeaveReason> OnLobbyLeft;

        public ObservableLobby(string lobbyId, string connectionString, SignalRController signalRController)
        {
            _lobby.LobbyId = lobbyId;
            _lobby.ConnectionString = connectionString;

            _signalRController = signalRController;
            var signalRLobbyMessageHandler = new SignalRLobbyMessageHandler(this, true);
            _signalRController.AddMessageHandler("LobbyChange", signalRLobbyMessageHandler.OnLobbyChangeMessage);
            _signalRController.AddSubscriptionChangeMessageHandler("LobbyChange", signalRLobbyMessageHandler.OnLobbySubscriptionChangeMessage);
        }

        public void Initialise(Action onInitialised, Action onInitialisationFailed)
        {
            if (_initialised)
                throw new InvalidOperationException("Lobby has already been initialised");

            SubscribeToLobbyEvents();
            
            PlayFabMultiplayerAPI.GetLobby(new GetLobbyRequest
            {
                LobbyId = LobbyId
            }, result =>
            {
                ApplyInitialState(result.Lobby);
                onInitialised?.Invoke();
            }, 
                error =>
            {
                Debug.LogError($"Failed to get lobby data - {error.GenerateErrorReport()}");
                onInitialisationFailed?.Invoke();
            });
        }

        private void ApplyInitialState(PlayFab.MultiplayerModels.Lobby lobby)
        {
            if (_initialised)
                throw new InvalidOperationException("Lobby has already been initialised");

            if (_lobby.LobbyId != lobby.LobbyId)
                throw new ArgumentException("Lobby ID does not match");

            _lobby.LobbyData = lobby.LobbyData ?? new Dictionary<string, string>();
            _lobby.Members = lobby.Members ?? new List<Member>();
            _lobby.Owner = lobby.Owner;
            _lobby.MembershipLock = lobby.MembershipLock;
            _lobby.ConnectionString = lobby.ConnectionString;
            _lobby.AccessPolicy = lobby.AccessPolicy;
            _lobby.MaxPlayers = lobby.MaxPlayers;

            _initialised = true;
            ApplyQueuedChanges();
        }

        public void ApplyOrQueueChanges(List<LobbyChange> changes)
        {
            var sorted = changes.OrderBy(x => x.ChangeNumber);

            if (_initialised)
            {
                foreach (var change in sorted)
                    ApplyChange(change);
            }
            else
            {
                foreach (var change in sorted)
                    _queuedChanges.Enqueue(change);
            }
        }

        private void ApplyChange(LobbyChange change)
        {
            if (!_initialised)
                throw new InvalidOperationException("Lobby has not been initialised");

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

            if (_lobby.Owner == null || _lobby.Owner.Id != newOwner.Id)
            {
                _lobby.Owner = newOwner;
                OnLobbyOwnerChanged?.Invoke(newOwner);
            }
        }

        private void RemoveMember(Member removedMember)
        {
            if (removedMember == null)
                throw new ArgumentNullException(nameof(removedMember));

            var member = _lobby.Members.FirstOrDefault(x => x.MemberEntity.Id == removedMember.MemberEntity.Id);
            if (member == null)
                return;

            _lobby.Members.Remove(member);
            OnLobbyMemberRemoved?.Invoke(removedMember);
        }

        private void UpdateMember(Member addedOrUpdatedMember)
        {
            if (addedOrUpdatedMember == null)
                throw new ArgumentNullException(nameof(addedOrUpdatedMember));

            var member = _lobby.Members.FirstOrDefault(x => x.MemberEntity.Id == addedOrUpdatedMember.MemberEntity.Id);
            if (member == null)
            {
                _lobby.Members.Add(addedOrUpdatedMember);
                OnLobbyMemberAdded?.Invoke(addedOrUpdatedMember);
            }
            else
            {
                if (member.MemberData == null || !member.MemberData.SequenceEqual(addedOrUpdatedMember.MemberData))
                {
                    member.MemberData = addedOrUpdatedMember.MemberData;
                    OnLobbyMemberDataChanged?.Invoke(addedOrUpdatedMember);
                }
                else
                {
                    Debug.LogWarning($"UpdateMember was called for entity " +
                                     $"{addedOrUpdatedMember.MemberEntity.Id} " +
                                     $"but member data is the same");
                }
            }
        }

        private void UpdateLobbyData(Dictionary<string, string> newLobbyData)
        {
            if (newLobbyData == null)
                throw new ArgumentNullException(nameof(newLobbyData));

            if (_lobby.LobbyData == null || !_lobby.LobbyData.SequenceEqual(newLobbyData))
            {
                _lobby.LobbyData = newLobbyData;
                OnLobbyDataChanged?.Invoke(newLobbyData);
            }
        }

        private void ApplyQueuedChanges()
        {
            if (!_initialised)
                throw new InvalidOperationException("Lobby has not been initialised");

            while (_queuedChanges.Count > 0)
            {
                var change = _queuedChanges.Dequeue();
                ApplyChange(change);
            }
        }

        public void KickMember(Member member, bool preventRejoin)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            if (!IsOwner)
                throw new InvalidOperationException("Only the lobby owner can kick members");

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

        public void SetReady(bool isReady)
        {
            if (IsOwner)
                throw new Exception("Lobby owner cannot set ready status");

            PlayFabMultiplayerAPI.UpdateLobby(new UpdateLobbyRequest
            {
                LobbyId = LobbyId,
                MemberEntity = LobbyController.LocalEntityKey,
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
        
        private void SubscribeToLobbyEvents()
        {
            PlayFabMultiplayerAPI.SubscribeToLobbyResource(new SubscribeToLobbyResourceRequest
            {
                Type = SubscriptionType.LobbyChange,
                EntityKey = LobbyController.LocalEntityKey,
                ResourceId = LobbyId,
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

        public void OnSubscriptionMessage(SubscriptionMessageType messageType)
        {
            switch (messageType)
            {
                case SubscriptionMessageType.Subscribed:
                    break;
                case SubscriptionMessageType.UnsubscribedMemberLeft:
                    OnLobbyLeft?.Invoke(LobbyLeaveReason.MemberLeft);
                    break;
                case SubscriptionMessageType.UnsubscribedMemberRemoved:
                    OnLobbyLeft?.Invoke(LobbyLeaveReason.MemberKicked);
                    break;
                case SubscriptionMessageType.UnsubscribedLobbyDeleted:
                    OnLobbyLeft?.Invoke(LobbyLeaveReason.LobbyClosed);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }
        }
    }
}