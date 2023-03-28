using Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button createLobbyButton;
    
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private TMP_InputField lobbyConnectionStringInputField;
    [SerializeField] private Button leaveLobbyButton;
    
    [SerializeField] private Toggle isReadyToggle;
    [SerializeField] private Button setReadyButton;
    
    [SerializeField] private Button startGameButton;

    [SerializeField] private LobbyController lobbyController;

    private void Awake()
    {
        createLobbyButton.onClick.AddListener(OnCreateLobbyButtonClicked);
        joinLobbyButton.onClick.AddListener(OnJoinLobbyButtonClicked);
        leaveLobbyButton.onClick.AddListener(OnLeaveLobbyButtonClicked);
        
        setReadyButton.onClick.AddListener(OnSetReadyButtonClicked);
        startGameButton.onClick.AddListener(OnStartGameButtonClicked);
    }

    private void OnStartGameButtonClicked()
    {
        lobbyController.StartGame();
    }

    private void OnSetReadyButtonClicked()
    {
        lobbyController.SetReady(isReadyToggle.isOn);
    }

    private void OnCreateLobbyButtonClicked()
    {
        lobbyController.CreateLobby();
    }
    
    private void OnJoinLobbyButtonClicked()
    {
        lobbyController.JoinLobby(lobbyConnectionStringInputField.text);
    }
    
    private void OnLeaveLobbyButtonClicked()
    {
        lobbyController.LeaveLobby();
    }
}
