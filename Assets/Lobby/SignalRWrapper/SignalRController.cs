using System;
using Lobby.SignalRWrapper.Messages;
using UnityEngine;

namespace Lobby.SignalRWrapper
{
    public class SignalRController
    {
        private readonly SignalRConnection _signalR;
        private readonly SignalRMessageBroker _messageBroker;
        
        public string ConnectionHandle => _signalR.ConnectionHandle;

        public SignalRController()
        {
            _messageBroker = new SignalRMessageBroker();
            _signalR = new SignalRConnection(_messageBroker);
        }

        public void Initialise(Action<string> onSessionStarted)
        {
            _signalR.Start();
            _signalR.OnStarted += delegate(string connectionHandle) { onSessionStarted?.Invoke(connectionHandle); };
            _signalR.OnStopped += delegate { Debug.LogError("SignalR connection stopped!"); };
        }

        public void AddMessageHandler(string topic, Action<Message> onMessage)
        {   
            _messageBroker.AddMessageHandler(topic, onMessage);
        }

        public void AddSubscriptionChangeMessageHandler(string topic, Action<SubscriptionChangeMessage> onSubscriptionChangeMessage)
        {
            _messageBroker.AddSubscriptionChangeMessageHandler(topic, onSubscriptionChangeMessage);
        }
        
        public void Dispose()
        {
            _signalR?.Stop();
        }
    }
}