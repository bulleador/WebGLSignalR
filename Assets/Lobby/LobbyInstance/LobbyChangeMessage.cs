using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lobby.LobbyInstance
{
    public class LobbyChangeMessage
    {
        [JsonProperty("lobbyId")]
        public string LobbyId { get; set; }
        
        [JsonProperty("lobbyChanges")]
        public List<LobbyChange> Changes { get; set; }
    }
}