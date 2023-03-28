using System.Collections.Generic;
using Lobby;
using PlayFab.MultiplayerModels;
using TMPro;
using UnityEngine;

public class LobbyViewUI : MonoBehaviour
{
    [SerializeField] private LobbyController lobbyController;
    
    // lobby info
    [SerializeField] private TextMeshProUGUI lobbyIdText;
    [SerializeField] private TextMeshProUGUI lobbyStatusText;
    [SerializeField] private TextMeshProUGUI membershipLockText;
    
    // member list
    [SerializeField] private RectTransform memberListContainer;
    [SerializeField] private LobbyMemberEntryUI memberEntryPrefab;
    private readonly List<LobbyMemberEntryUI> _memberEntries = new();
    
    private PlayFab.MultiplayerModels.Lobby _currentLobby;

    private void OnEnable()
    {
        lobbyController.OnLobbyUpdated += OnLobbyUpdated;
        
        // member update events
        lobbyController.OnMemberJoined += OnMemberJoined;
        lobbyController.OnMemberLeft += OnMemberLeft;
        lobbyController.OnMemberDataUpdated += OnMemberUpdated;
        
        // lobby update events
        lobbyController.OnLobbyMembershipLockChanged += OnLobbyMembershipLockChanged;
        lobbyController.OnLobbyOwnerChanged += OnLobbyOwnerChanged;
        lobbyController.OnLobbyDataUpdated += OnLobbyDataUpdated;
        
        // local player events
        lobbyController.OnLobbyJoinedByLocalPlayer += OnLobbyJoined;
        lobbyController.OnLobbyLeftByLocalPlayer += OnLobbyLeft;
    }
    private void OnDisable()
    {
        lobbyController.OnLobbyUpdated -= OnLobbyUpdated;
        
        // member update events
        lobbyController.OnMemberJoined -= OnMemberJoined;
        lobbyController.OnMemberLeft -= OnMemberLeft;
        lobbyController.OnMemberDataUpdated -= OnMemberUpdated;
        
        // lobby update events
        lobbyController.OnLobbyMembershipLockChanged -= OnLobbyMembershipLockChanged;
        lobbyController.OnLobbyOwnerChanged -= OnLobbyOwnerChanged;
        lobbyController.OnLobbyDataUpdated -= OnLobbyDataUpdated;
        
        // local player events
        lobbyController.OnLobbyJoinedByLocalPlayer -= OnLobbyJoined;
        lobbyController.OnLobbyLeftByLocalPlayer -= OnLobbyLeft;
    }

    private void OnLobbyUpdated(PlayFab.MultiplayerModels.Lobby obj)
    {
        _currentLobby = obj;
    }

    #region Member Updates

    private void OnMemberUpdated(Member obj)
    {
        var memberEntry = _memberEntries.Find(x => x.Member.HasSameIdAs(obj));
        memberEntry.UpdateMember(_currentLobby, obj);
    }

    private void OnMemberLeft(Member obj)
    {
        var memberEntry = _memberEntries.Find(x => x.Member.HasSameIdAs(obj));
        memberEntry.HandleMemberLeft();
        
        _memberEntries.Remove(memberEntry);
    }

    private void OnMemberJoined(Member obj)
    {
        var memberEntry = Instantiate(memberEntryPrefab, memberListContainer);
        memberEntry.UpdateMember(_currentLobby, obj);
        
        _memberEntries.Add(memberEntry);
    }

    #endregion

    #region Local Player Events 

    private void OnLobbyJoined(PlayFab.MultiplayerModels.Lobby obj)
    {
        lobbyIdText.text = obj.LobbyId;
        
        foreach (var member in obj.Members)
        {
            OnMemberJoined(member);
        }
        
        OnLobbyDataUpdated(obj);
    }

    private void OnLobbyLeft()
    {
        lobbyIdText.text = "No Lobby";
        lobbyStatusText.text = "No Lobby";
        membershipLockText.text = "No Lobby";
        
        foreach (var memberEntry in _memberEntries)
        {
            Destroy(memberEntry.gameObject);
        }
        _memberEntries.Clear();
    }

    #endregion

    #region Lobby Updates

    private void OnLobbyDataUpdated(PlayFab.MultiplayerModels.Lobby obj)
    {
        lobbyStatusText.text = obj.IsReady() ? "Game Started" : "Waiting for Players";
    }

    private void OnLobbyOwnerChanged(EntityKey obj)
    {
        
    }

    private void OnLobbyMembershipLockChanged(MembershipLock obj)
    {
        membershipLockText.text = obj.ToString();
    }

    #endregion
}