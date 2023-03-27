using Newtonsoft.Json;

namespace Lobby.Signal.Messages
{
    public class Message
    {
        [JsonProperty("topic")] 
        public string Topic { get; set; }
        [JsonProperty("payload")] 
        public string Payload { get; set; }
        [JsonProperty("traceId")] 
        public string TraceId { get; set; }

        public override string ToString()
        {
            return $"Topic: {Topic}, Payload: {Payload}, TraceId: {TraceId}";
        }
    }
}