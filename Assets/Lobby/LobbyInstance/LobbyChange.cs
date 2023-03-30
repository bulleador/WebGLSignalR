using System.Collections.Generic;
using Newtonsoft.Json;
using PlayFab.MultiplayerModels;

namespace Lobby.LobbyInstance
{
    public class LobbyChange
    {
        [JsonProperty("changeNumber")] public int ChangeNumber { get; set; }

        [JsonProperty("pubSubConnectionHandle")]
        public string PubSubConnectionHandle { get; set; }

        /// <summary>
        /// The member that was added or his data was changed.
        /// </summary>
        [JsonProperty("memberToMerge")]
        public Member MemberToMerge { get; set; }

        [JsonProperty("memberToDelete")] public Member RemovedMember { get; set; }

        [JsonProperty("lobbyData")] public Dictionary<string, string> LobbyData { get; set; }

        [JsonProperty("owner")] public EntityKey Owner { get; set; }

        public ChangeType ChangeType
        {
            get
            {
                if (MemberToMerge != null)
                    return ChangeType.MemberAddedOrChanged;

                if (RemovedMember != null)
                    return ChangeType.MemberRemoved;

                if (Owner != null)
                    return ChangeType.OwnerChanged;

                if (LobbyData != null)
                    return ChangeType.LobbyDataUpdate;

                return ChangeType.Ignore;
            }
        }
    }

    public enum ChangeType
    {
        LobbyDataUpdate,
        MemberAddedOrChanged,
        MemberRemoved,
        OwnerChanged,
        Ignore,
    }
}