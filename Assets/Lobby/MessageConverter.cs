using Lobby.Signal.Messages;
using Newtonsoft.Json;

namespace Lobby
{
    public class MessageConverter
    {
        public T Convert<T>(Message message)
        {
            var payload = message.Payload;
            return Convert<T>(payload);
        }

        public string ToJson(Message message)
        {
            var payload = message.Payload;
            return ToJson(payload);
        }

        public string ToJson(string payload)
        {
            var bytes = System.Convert.FromBase64String(payload);
            var decoded = System.Text.Encoding.UTF8.GetString(bytes);
            return decoded;
        }

        public T Convert<T>(string payload)
        {
            var json = ToJson(payload);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}