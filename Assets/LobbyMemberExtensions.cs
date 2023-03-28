using Lobby;
using PlayFab.MultiplayerModels;

public static class LobbyMemberExtensions
{
    public static bool HasSameIdAs(this Member member, Member otherMember)
    {
        return member.MemberEntity.Id == otherMember.MemberEntity.Id;
    }

    public static bool IsReady(this Member member)
    {
        if (member == null)
            throw new System.ArgumentNullException(nameof(member));

        if (member.MemberData == null)
            return false;

        return member.MemberData.TryGetValue(LobbyConstants.IsReady, out var isReady) && bool.Parse(isReady);
    }
    
    public static bool IsOwnerOf(this Member member, PlayFab.MultiplayerModels.Lobby lobby)
    {
        if (member == null)
            throw new System.ArgumentNullException(nameof(member));
        if (lobby == null)
            throw new System.ArgumentNullException(nameof(lobby));

        return member.MemberEntity.Id == lobby.Owner.Id;
    }
}