using System;
using System.Collections.Generic;
using System.Linq;
using EasyButtons;
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

        public string CurrentLobbyId { get; private set; }
        public PlayFab.MultiplayerModels.Lobby CurrentLobby { get; private set; }

        [Button]
        public void CreateLobby()
        {
            Debug.Log("Creating lobby...");

            PlayFabMultiplayerAPI.CreateLobby(new CreateLobbyRequest
            {
                Owner = LocalEntityKey,
                Members = new List<Member> { LocalMember },
                AccessPolicy = AccessPolicy.Public,
                MaxPlayers = 4,
                OwnerMigrationPolicy = OwnerMigrationPolicy.None,
                UseConnections = false
            }, OnLobbyCreated, OnLobbyCreationFailed);

            void OnLobbyCreated(CreateLobbyResult createLobbyResult)
            {
                Debug.Log(
                    $"Lobby created. Lobby ID: {createLobbyResult.LobbyId}. Lobby connection string: {createLobbyResult.ConnectionString}");
                CurrentLobbyId = createLobbyResult.LobbyId;
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

                CurrentLobbyId = joinLobbyResult.LobbyId;
            }

            void OnLobbyJoinFailed(PlayFabError error)
            {
                Debug.LogError($"Lobby join failed - {error.GenerateErrorReport()}");
            }
        }

        [Button]
        public void LeaveLobby()
        {
            PlayFabMultiplayerAPI.LeaveLobby(new LeaveLobbyRequest
            {
                LobbyId = CurrentLobbyId,
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
            PlayFabMultiplayerAPI.UpdateLobby(new UpdateLobbyRequest
            {
                LobbyId = CurrentLobbyId,
                MemberEntity = LocalEntityKey,
                MemberData = new Dictionary<string, string>
                {
                    { LobbyConstants.IsReady, isReady.ToString() }
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
            PlayFabMultiplayerAPI.UpdateLobby(new UpdateLobbyRequest
            {
                LobbyId = CurrentLobbyId,
                LobbyData = new Dictionary<string, string>
                {
                    { LobbyConstants.LobbyStatus, "true" }
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

        #region Updates

        public event Action<PlayFab.MultiplayerModels.Lobby> OnLobbyUpdated;
        
        // LOCAL
        public event Action<PlayFab.MultiplayerModels.Lobby> OnLobbyJoinedByLocalPlayer;
        public event Action OnLobbyLeftByLocalPlayer;
        
        // REMOTE MEMBER UPDATES
        public event Action<Member> OnMemberJoined;
        public event Action<Member> OnMemberLeft;
        public event Action<Member> OnMemberDataUpdated;
        
        // REMOTE LOBBY UPDATES
        public event Action<PlayFab.MultiplayerModels.Lobby> OnLobbyDataUpdated;
        public event Action<EntityKey> OnLobbyOwnerChanged;
        public event Action<MembershipLock> OnLobbyMembershipLockChanged;
        

        public void UpdateLobby(PlayFab.MultiplayerModels.Lobby newLobby)
        {
            OnLobbyUpdated?.Invoke(newLobby);
            
            if (CurrentLobby == null)
            {
                Debug.Log("Lobby created");
                OnLobbyJoinedByLocalPlayer?.Invoke(newLobby);
            }
            else if (CurrentLobby != null && newLobby == null)
            {
                Debug.Log("Lobby left");
                OnLobbyLeftByLocalPlayer?.Invoke();
            }
            else if (CurrentLobby != null && newLobby != null)
            {
                UpdateMembers(newLobby);
                UpdateLobbyData(newLobby);
                UpdateOwner(newLobby);
                UpdateMembershipLock(newLobby);
                UpdateLobbyData(newLobby);
            } 
            else
            {
                throw new Exception("Unhandled lobby update");
            }

            CurrentLobby = newLobby;
        }

        private void UpdateMembershipLock(PlayFab.MultiplayerModels.Lobby newLobby)
        {
            if (CurrentLobby.MembershipLock != newLobby.MembershipLock)
            {
                OnLobbyMembershipLockChanged?.Invoke(newLobby.MembershipLock);
                Debug.Log($"Lobby membership lock status changed to {newLobby.MembershipLock}");
            }
        }

        private void UpdateOwner(PlayFab.MultiplayerModels.Lobby newLobby)
        {
            if (CurrentLobby.Owner.Id != newLobby.Owner.Id)
            {
                OnLobbyOwnerChanged?.Invoke(newLobby.Owner);
                Debug.Log($"Lobby owner changed from {CurrentLobby.Owner.Id} to {newLobby.Owner.Id}");
            }
        }

        private void UpdateLobbyData(PlayFab.MultiplayerModels.Lobby newLobby)
        {
            if (CurrentLobby.LobbyData == null && newLobby.LobbyData == null)
                return;
            
            if (CurrentLobby.LobbyData == null && newLobby.LobbyData != null)
            {
                OnLobbyDataUpdated?.Invoke(newLobby);
                Debug.Log("Lobby data updated");
            }
            else if (CurrentLobby.LobbyData != null && newLobby.LobbyData == null)
            {
                OnLobbyDataUpdated?.Invoke(newLobby);
                Debug.Log("Lobby data updated");
            }
            else if (!CurrentLobby.LobbyData.SequenceEqual(newLobby.LobbyData))
            {
                OnLobbyDataUpdated?.Invoke(newLobby);
                Debug.Log("Lobby data updated");
            }
        }

        private void UpdateMembers(PlayFab.MultiplayerModels.Lobby newLobby)
        {
            var membersThatJoined = newLobby.Members.ToList().Except(CurrentLobby.Members.ToList(), new IMemberEntityKeyComparer()).ToList();
            var membersThatLeft = CurrentLobby.Members.ToList().Except(newLobby.Members.ToList(), new IMemberEntityKeyComparer()).ToList();
            
            foreach(var member in membersThatJoined)
            {
                OnMemberJoined?.Invoke(member);
                Debug.Log($"Member {member.MemberEntity.Id} joined the lobby");
            }
            
            foreach(var member in membersThatLeft)
            {
                OnMemberLeft?.Invoke(member);
                Debug.Log($"Member {member.MemberEntity.Id} left the lobby");
            }
            
            UpdateMemberData(newLobby);

            void UpdateMemberData(PlayFab.MultiplayerModels.Lobby lobby)
            {
                var sameMembers = lobby.Members.ToList().Intersect(CurrentLobby.Members.ToList(), new IMemberEntityKeyComparer()).ToList();
                
                foreach (var member in sameMembers)
                {
                    var oldMember = CurrentLobby.Members.ToList().First(m => m.MemberEntity.Id == member.MemberEntity.Id);
                    if (!oldMember.MemberData.SequenceEqual(member.MemberData))
                    {
                        OnMemberDataUpdated?.Invoke(member);
                        Debug.Log($"Member {member.MemberEntity.Id} updated their data");
                    }
                }
            }
        }

        private class IMemberEntityKeyComparer : IEqualityComparer<Member>
        {
            public bool Equals(Member x, Member y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return Equals(x.MemberEntity.Id, y.MemberEntity.Id);
            }

            public int GetHashCode(Member obj)
            {
                return obj.MemberEntity.Id.GetHashCode();
            }
        }

        #endregion
    }
}