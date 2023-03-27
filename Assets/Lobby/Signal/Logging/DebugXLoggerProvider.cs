using Microsoft.Extensions.Logging;

namespace Lobby.Signal.Logging
{
    internal class DebugXLoggerProvider : ILoggerProvider
    {
        public void Dispose()
        {
        
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DebugXLogger();
        }
    }
}