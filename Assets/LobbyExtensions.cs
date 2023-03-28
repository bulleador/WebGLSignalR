using Lobby;
using PlayFab.MultiplayerModels;

public static class LobbyExtensions
{
    public static bool IsReady(this PlayFab.MultiplayerModels.Lobby lobby)
    {
        if (lobby == null)
            throw new System.ArgumentNullException(nameof(lobby));

        if (lobby.LobbyData == null)
            return false;
        
        return lobby.LobbyData.TryGetValue(LobbyConstants.IsReady, out var isReady) && bool.Parse(isReady);
    }
}