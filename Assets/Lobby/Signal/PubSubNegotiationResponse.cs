using Newtonsoft.Json;

namespace Lobby.Signal
{
    public class PubSubNegotiationResponse
    {
        [JsonProperty("url")] public string Url { get; set; }

        [JsonProperty("accessToken")] public string AccessToken { get; set; }
    }
}