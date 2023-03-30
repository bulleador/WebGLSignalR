using System.Collections.Generic;
using Newtonsoft.Json;
using PlayFab.MultiplayerModels;

namespace Lobby
{
    public class LobbyMember
    {
        // For serialisation
        private LobbyMember()
        {
        }

        [JsonProperty("memberData")] 
        public Dictionary<string, string> MemberData { get; set; }

        [JsonProperty("memberEntity")] 
        public EntityKey MemberEntity { get; set; }

        [JsonProperty("pubSubConnectionHandle")]
        public string PubSubConnectionHandle { get; set; }
        
        public static LobbyMember FromMember(Member member)
        {
            var lobbyMember = new LobbyMember
            {
                MemberData = member.MemberData,
                MemberEntity = member.MemberEntity,
                PubSubConnectionHandle = member.PubSubConnectionHandle
            };
            return lobbyMember;
        }
    }
}