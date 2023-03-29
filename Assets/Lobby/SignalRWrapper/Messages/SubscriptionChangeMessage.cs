using Newtonsoft.Json;

namespace Lobby.SignalRWrapper.Messages
{
    public class SubscriptionChangeMessage
    {
        [JsonProperty("entityType")] public string EntityType { get; set; }
        [JsonProperty("entityId")] public string EntityId { get; set; }
        [JsonProperty("topic")] public string Topic { get; set; }
        [JsonProperty("status")] public string Status { get; set; }
        [JsonProperty("unsubscribeReason")] public string UnsubscribeReason { get; set; }
        [JsonProperty("traceId")] public string TraceId { get; set; }

        public override string ToString()
        {
            return
                $"EntityType: {EntityType}, EntityId: {EntityId}, Topic: {Topic}, Status: {Status}, UnsubscribeReason: {UnsubscribeReason}, TraceId: {TraceId}";
        }
    }
}