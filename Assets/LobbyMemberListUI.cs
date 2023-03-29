using System;
using System.Collections.Generic;
using System.Linq;
using Lobby;
using UnityEngine;

public class LobbyMemberListUI : MonoBehaviour
{
    [SerializeField] private LobbyMemberEntryUI memberListEntryPrefab;
    [SerializeField] private Transform memberListEntryParent;
    
    [SerializeField] private LobbyController lobbyController;
    
    private readonly List<LobbyMemberEntryUI> _memberListEntries = new();

    private void OnEnable()
    {
        Clear();
        
        lobbyController.OnLobbyMemberAdded += OnMemberAdded;
        lobbyController.OnLobbyMemberRemoved += OnMemberRemoved;
        lobbyController.OnLobbyMemberDataChanged += OnMemberDataChanged;
    }
    private void OnDisable()
    {
        lobbyController.OnLobbyMemberAdded -= OnMemberAdded;
        lobbyController.OnLobbyMemberRemoved -= OnMemberRemoved;
        lobbyController.OnLobbyMemberDataChanged -= OnMemberDataChanged;
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
    
    private void OnMemberAdded(LobbyMember newMember)
    {
        if (_memberListEntries.Any(entry => entry.Member.MemberEntity.Id == newMember.MemberEntity.Id))
            throw new Exception("Member already in list"); // TODO: Handle this better
        
        var newEntry = Instantiate(memberListEntryPrefab, memberListEntryParent);
        newEntry.Initialise(lobbyController, newMember);
        _memberListEntries.Add(newEntry);
    }
    
    private void OnMemberRemoved(LobbyMember obj)
    {
        var entry = _memberListEntries.FirstOrDefault(e => e.Member.MemberEntity.Id == obj.MemberEntity.Id);
        if (entry == null)
            throw new Exception("Member not in list"); // TODO: Handle this better
        
        _memberListEntries.Remove(entry);
        entry.HandleMemberRemoved();
    }
    
    private void OnMemberDataChanged(LobbyMember obj)
    {
        var entry = _memberListEntries.FirstOrDefault(e => e.Member.MemberEntity.Id == obj.MemberEntity.Id);
        if (entry == null)
            throw new Exception("Member not in list"); // TODO: Handle this better
        
        entry.UpdateMember(obj);
    }
}
