using System.Collections.Generic;
using Newtonsoft.Json;
using PlayFab.MultiplayerModels;

namespace Lobby
{
    public class LobbyMember
    {
        public LobbyMember(Member member)
        {
            MemberEntity = member.MemberEntity;
            MemberData = member.MemberData;
            PubSubConnectionHandle = member.PubSubConnectionHandle;
        }

        [JsonProperty("memberData")] 
        public Dictionary<string, string> MemberData { get; set; }

        [JsonProperty("memberEntity")] 
        public EntityKey MemberEntity { get; set; }

        [JsonProperty("pubSubConnectionHandle")]
        public string PubSubConnectionHandle { get; set; }
    }
}