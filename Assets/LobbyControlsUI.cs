using Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyControlsUI : MonoBehaviour
{
    [SerializeField] private Button createLobbyButton;
    
    [SerializeField] private Button joinLobbyButton;
    [SerializeField] private TMP_InputField lobbyConnectionStringInputField;
    [SerializeField] private Button leaveLobbyButton;
    
    [SerializeField] private Toggle isReadyToggle;
    
    [SerializeField] private Button startGameButton;

    [SerializeField] private LobbyController lobbyController;

    private void Awake()
    {
        createLobbyButton.onClick.AddListener(OnCreateLobbyButtonClicked);
        joinLobbyButton.onClick.AddListener(OnJoinLobbyButtonClicked);
        leaveLobbyButton.onClick.AddListener(OnLeaveLobbyButtonClicked);
        startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        isReadyToggle.onValueChanged.AddListener(lobbyController.SetReady);
        
        isReadyToggle.SetIsOnWithoutNotify(false);

        SetInteractable(false);
        
        AccountManager.OnAuthenticated += OnAuthenticated;
        
        lobbyController.OnLobbyCreated += OnLobbyCreated;
        lobbyController.OnLobbyJoined += OnLobbyJoined;
        lobbyController.OnLobbyLeft += OnLobbyLeft;
    }

    private void OnLobbyLeft()
    {
        SetInteractable(false);
        
        createLobbyButton.interactable = true;
        joinLobbyButton.interactable = true;
        lobbyConnectionStringInputField.interactable = true;
    }

    private void OnLobbyJoined()
    {
        SetInteractable(false);
        
        leaveLobbyButton.interactable = true;
        isReadyToggle.interactable = true;
    }

    private void OnLobbyCreated()
    {
        SetInteractable(false);
        
        leaveLobbyButton.interactable = true;
        startGameButton.interactable = true;
    }

    private void OnAuthenticated()
    {
        SetInteractable(false);
        
        createLobbyButton.interactable = true;
        joinLobbyButton.interactable = true;
        lobbyConnectionStringInputField.interactable = true;
    }

    private void SetInteractable(bool interactable)
    {
        createLobbyButton.interactable = interactable;
        joinLobbyButton.interactable = interactable;
        leaveLobbyButton.interactable = interactable;
        isReadyToggle.interactable = interactable;
        startGameButton.interactable = interactable;
        lobbyConnectionStringInputField.interactable = interactable;
    }

    private void OnStartGameButtonClicked()
    {
        lobbyController.StartGame();
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
