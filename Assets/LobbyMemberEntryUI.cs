using PlayFab.MultiplayerModels;
using TMPro;
using UnityEngine;

public class LobbyMemberEntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerIdText;
    [SerializeField] private TextMeshProUGUI playerStatusText;
    [SerializeField] private TextMeshProUGUI isOwnerText;
    
    public Member Member { get; private set; }

    public void UpdateMember(PlayFab.MultiplayerModels.Lobby lobby, Member member)
    {
        Member = member;
        
        playerIdText.text = member.MemberEntity.Id;
        playerStatusText.text = member.IsReady() ? "Ready" : "Not Ready";
        isOwnerText.enabled = member.IsOwnerOf(lobby);
    }

    public void HandleMemberLeft()
    {
        Destroy(gameObject);
    }
}