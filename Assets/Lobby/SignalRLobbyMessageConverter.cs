using Lobby.SignalRWrapper.Messages;
using Newtonsoft.Json;

namespace Lobby
{
    public class SignalRLobbyMessageConverter
    {
        public T Convert<T>(Message message)
        {
            var payload = ToJson(message.Payload);
            return JsonConvert.DeserializeObject<T>(payload);
        }
        
        public string ToJson(string payload)
        {
            var bytes = System.Convert.FromBase64String(payload);
            var decoded = System.Text.Encoding.UTF8.GetString(bytes);
            return decoded;
        }
    }
}