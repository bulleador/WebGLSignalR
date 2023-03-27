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

    [SerializeField] private LobbyController lobbyController;
    //[SerializeField] private LobbyView lobbyView;

    private void Awake()
    {
        createLobbyButton.onClick.AddListener(OnCreateLobbyButtonClicked);
        joinLobbyButton.onClick.AddListener(OnJoinLobbyButtonClicked);
        leaveLobbyButton.onClick.AddListener(OnLeaveLobbyButtonClicked);
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
