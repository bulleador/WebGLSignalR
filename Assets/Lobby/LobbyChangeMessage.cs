using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lobby
{
    public class LobbyChangeMessage
    {
        [JsonProperty("lobbyId")]
        public string LobbyId { get; set; }
        
        [JsonProperty("lobbyChanges")]
        public List<LobbyChange> Changes { get; set; }
    }
}