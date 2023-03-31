using System;
using System.Collections.Generic;
using Lobby.LobbyInstance;
using Lobby.SignalRWrapper;
using PlayFab;
using PlayFab.MultiplayerModels;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lobby
{
    /// <summary>
    /// This class is responsible for managing the user's presence in a lobby.
        /// <para>
            /// Allows the user to create a lobby, join a lobby and leave a lobby,
            /// but changing the lobby's data is handled by the ObservableLobby class.
        /// </para>
        /// <para>
            /// IMPORTANT: LobbyController has to be initialised before it can be used with <c>Initialise()</c>
        /// </para>
        /// <para>
            /// IMPORTANT 2: <c>PlayFabClient.IsEntityLoggedIn()</c> must be true before <c>Initialise()</c> is called.
        /// </para>
    /// </summary>
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

        private bool _initialised;

        public event Action OnInitialised;
        public event Action<ObservableLobby, bool> OnLobbyJoined;
        public event Action<ObservableLobby, LobbyLeaveReason> OnLobbyLeft;

        public ObservableLobby Lobby { get; private set; }
        public bool InLobby => Lobby != null;

        private SignalRController _signalRController;

        private void Awake()
        {
            _signalRController = new SignalRController();
        }

        public void Initialise()
        {
            if (_initialised)
                throw new Exception("Already initialised");

            _signalRController.Initialise(s =>
            {
                _initialised = true;
                OnInitialised?.Invoke();
            });
        }

        public void CreateLobby()
        {
            if (!_initialised)
                throw new Exception("Not initialised");

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
                Debug.Log($"Lobby created. Lobby ID: {createLobbyResult.LobbyId}. " +
                          $"Lobby connection string: {createLobbyResult.ConnectionString}");

#if UNITY_EDITOR
                EditorGUIUtility.systemCopyBuffer = createLobbyResult.ConnectionString;
#endif

                CreateAndInitialiseLobbyInstance(createLobbyResult.LobbyId, createLobbyResult.ConnectionString, true);
            }

            void OnLobbyCreationFailed(PlayFabError error)
            {
                Debug.LogError($"Lobby creation failed - {error.GenerateErrorReport()}");
                throw new NotImplementedException("TODO - handle lobby creation failure");
            }
        }

        public void JoinLobby(string connectionString)
        {
            if (!_initialised)
                throw new Exception("Not initialised");

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
                CreateAndInitialiseLobbyInstance(joinLobbyResult.LobbyId, connectionString, false);
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

        private void CreateAndInitialiseLobbyInstance(string lobbyId, string connectionString, bool asOwner)
        {
            Lobby = new ObservableLobby(lobbyId, connectionString, _signalRController);
            Lobby.Initialise(() => { OnLobbyJoined?.Invoke(Lobby, asOwner); },
                () => throw new NotImplementedException());

            Lobby.OnLobbyLeft += reason =>
            {
                OnLobbyLeft?.Invoke(Lobby, reason);
                Dispose();
            };
        }

        private void Dispose()
        {
            Lobby = null;
        }

        private void OnDestroy()
        {
            _signalRController.Dispose();
        }
    }
}