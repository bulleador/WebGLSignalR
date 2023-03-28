using Newtonsoft.Json;

namespace Lobby.Signal
{
    public class StartOrRecoverSessionRequest
    {
        public string traceParent { get; set; }
    }
}