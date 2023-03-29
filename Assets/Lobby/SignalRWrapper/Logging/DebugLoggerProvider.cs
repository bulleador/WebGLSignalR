using Microsoft.Extensions.Logging;

namespace Lobby.SignalRWrapper.Logging
{
    internal class DebugLoggerProvider : ILoggerProvider
    {
        public void Dispose()
        {
        
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DebugLogger();
        }
    }
}