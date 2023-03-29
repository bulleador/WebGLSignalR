using Newtonsoft.Json;

namespace Lobby.SignalR.PlayFab
{
    public class PubSubNegotiationResponse
    {
        [JsonProperty("url")] public string Url { get; set; }

        [JsonProperty("accessToken")] public string AccessToken { get; set; }
    }
}