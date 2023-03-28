using Newtonsoft.Json;

namespace Lobby.Signal
{
    public class StartOrRecoverSessionResponse
    {
        [JsonProperty("newConnectionHandle")] public string NewConnectionHandle { get; set; }

        [JsonProperty("recoveredTopics")] public string[] RecoveredTopics { get; set; }

        [JsonProperty("status")] public string Status { get; set; }

        [JsonProperty("traceId")] public string TraceId { get; set; }

        public override string ToString()
        {
            return $"StartOrRecoverSessionResponse:\n" +
                   $"newConnectionHandle = {NewConnectionHandle},\n" +
                   $"recoveredTopics = {RecoveredTopics},\n" +
                   $"status = {Status},\n" +
                   $"traceId = {TraceId}";
        }
    }
}