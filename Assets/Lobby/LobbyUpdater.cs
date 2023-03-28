using System;
using PlayFab;
using PlayFab.MultiplayerModels;
using UnityEngine;

namespace Lobby
{
    [RequireComponent(typeof(LobbyController))]
    public class LobbyUpdater : MonoBehaviour
    {
        [SerializeField] private float updateInterval = 1f;
        
        private LobbyController _lobbyController;
        
        private float _timeSinceLastUpdate;
        private bool _updateInProgress;

        private void Awake()
        {
            _lobbyController = GetComponent<LobbyController>();
        }

        private void Update()
        {
            // don't update if we're not in a lobby 
            if (string.IsNullOrEmpty(_lobbyController.CurrentLobbyId))
                return;
            
            // don't update if we're already updating
            if (_updateInProgress)
                return;
            
            // don't update if we're not ready to update
            _timeSinceLastUpdate += Time.deltaTime;
            if (!(_timeSinceLastUpdate >= updateInterval)) 
                return;
            
            // update
            _timeSinceLastUpdate = 0;
            UpdateLobby();
        }

        private void UpdateLobby()
        {
            _updateInProgress = true;
            
            PlayFabMultiplayerAPI.GetLobby(new GetLobbyRequest
            {
                LobbyId = _lobbyController.CurrentLobbyId
            }, OnLobbyUpdated, OnLobbyUpdateFailed);
            
            void OnLobbyUpdateFailed(PlayFabError error)
            {
                Debug.LogError($"Lobby update failed - {error.GenerateErrorReport()}");
                _updateInProgress = false;
            }
            
            void OnLobbyUpdated(GetLobbyResult lobbyResult)
            {
                _lobbyController.UpdateLobby(lobbyResult.Lobby);
                _updateInProgress = false;
            }
        }
    }
}