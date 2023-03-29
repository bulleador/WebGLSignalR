using System;
using Microsoft.Extensions.Logging;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Lobby.SignalRWrapper.Logging
{
    internal class DebugLogger : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    Debug.Log(formatter(state, exception));
                    break;
                case LogLevel.Debug:
                    Debug.Log(formatter(state, exception));
                    break;
                case LogLevel.Information:
                    Debug.Log(formatter(state, exception));
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(formatter(state, exception));
                    break;
                case LogLevel.Error:
                    Debug.LogError(formatter(state, exception));
                    break;
                case LogLevel.Critical:
                    Debug.LogError(formatter(state, exception));
                    break;
                case LogLevel.None:
                    Debug.Log(formatter(state, exception));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}