using System;
using System.Collections.Generic;
using System.Linq;
using Lobby;
using Lobby.LobbyInstance;
using PlayFab.MultiplayerModels;
using UnityEngine;

public class LobbyMemberListUI : MonoBehaviour
{
    [SerializeField] private LobbyMemberEntryUI memberListEntryPrefab;
    [SerializeField] private Transform memberListEntryParent;
    
    [SerializeField] private LobbyController lobbyController;
    
    private readonly List<LobbyMemberEntryUI> _memberListEntries = new();
    
    private ObservableLobby _currentLobby;

    private void OnEnable()
    {
        Clear();
        
        lobbyController.OnLobbyJoined += OnLobbyJoined;
        lobbyController.OnLobbyLeft += OnLobbyLeft;
    }

    private void OnDisable()
    {
        lobbyController.OnLobbyJoined -= OnLobbyJoined;
        lobbyController.OnLobbyLeft -= OnLobbyLeft;
    }

    private void OnLobbyJoined(ObservableLobby lobby, bool asOwner)
    {
        _currentLobby = lobby;
        
        foreach (var member in lobby.Members)
        {
            OnMemberAdded(member);
        }
        
        lobby.OnLobbyMemberDataChanged += OnMemberDataChanged;
        lobby.OnLobbyMemberAdded += OnMemberAdded;
        lobby.OnLobbyMemberRemoved += OnMemberRemoved;
        lobby.OnLobbyOwnerChanged += OnOwnerChanged;
        lobby.OnLobbyDataChanged += OnLobbyDataChanged;
    }

    private void OnLobbyLeft(ObservableLobby observableLobby, LobbyLeaveReason lobbyLeaveReason)
    {
        observableLobby.OnLobbyMemberDataChanged -= OnMemberDataChanged;
        observableLobby.OnLobbyMemberAdded -= OnMemberAdded;
        observableLobby.OnLobbyMemberRemoved -= OnMemberRemoved;
        observableLobby.OnLobbyOwnerChanged -= OnOwnerChanged;
        observableLobby.OnLobbyDataChanged -= OnLobbyDataChanged;
        
        _currentLobby = null;
        
        Clear();
    }
    
    private void Clear()
    {
        foreach(var child in memberListEntryParent.transform)
        {
            if (child is Transform childTransform)
                Destroy(childTransform.gameObject);
        }
        
        _memberListEntries.Clear();
    }
    
    private void OnMemberAdded(Member newMember)
    {
        if (_memberListEntries.Any(entry => entry.Member.MemberEntity.Id == newMember.MemberEntity.Id))
        {
            Debug.Log($"Member {newMember.MemberEntity.Id} already in list.");
            return;
        }
        
        var newEntry = Instantiate(memberListEntryPrefab, memberListEntryParent);
        newEntry.Initialise(_currentLobby, newMember);
        _memberListEntries.Add(newEntry);
    }
    
    private void OnMemberRemoved(Member obj)
    {
        var entry = _memberListEntries.FirstOrDefault(e => e.Member.MemberEntity.Id == obj.MemberEntity.Id);
        if (entry == null)
            throw new Exception("Member not in list"); // TODO: Handle this better
        
        _memberListEntries.Remove(entry);
        entry.HandleMemberRemoved();
    }
    
    private void OnMemberDataChanged(Member obj)
    {
        var entry = _memberListEntries.FirstOrDefault(e => e.Member.MemberEntity.Id == obj.MemberEntity.Id);
        if (entry == null)
            throw new Exception("Member not in list"); // TODO: Handle this better
        
        entry.UpdateMember(obj);
    }
    
    private void OnLobbyDataChanged(Dictionary<string, string> obj)
    {
        
    }

    private void OnOwnerChanged(EntityKey obj)
    {
        
    }
}
