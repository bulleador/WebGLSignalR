using System.Collections.Generic;
using Newtonsoft.Json;
using PlayFab.MultiplayerModels;

namespace Lobby
{
    public class LobbyMember
    {
        [JsonProperty("memberData")]
        public Dictionary<string,string> MemberData;
        
        /// <summary>
        /// The member entity key.
        /// </summary>
        [JsonProperty("memberEntity")]
        public EntityKey MemberEntity;
        
        /// <summary>
        /// Opaque string, stored on a Subscribe call, which indicates the connection an owner or member has with PubSub.
        /// </summary>
        [JsonProperty("pubSubConnectionHandle")]
        public string PubSubConnectionHandle;
    }
}