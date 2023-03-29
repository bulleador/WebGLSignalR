using Newtonsoft.Json;

namespace Lobby
{
    public class LobbyChange
    {
        [JsonProperty("changeNumber")] public int ChangeNumber { get; set; }

        [JsonProperty("memberToMerge")] public LobbyMember AddedMember { get; set; }
    }
}