using System;
using Lobby.SignalR.Messages;
using UnityEngine;

namespace Lobby.SignalR
{
    public class SignalRController : MonoBehaviour
    {
        private SignalRConnection _signalR;
        private SignalRMessageBroker _messageBroker;
        
        public string ConnectionHandle => _signalR.ConnectionHandle;
        
        public void Initialise(Action<string> onSessionStarted)
        {
            _messageBroker = new SignalRMessageBroker();
            
            _signalR = new SignalRConnection(_messageBroker.OnMessage, _messageBroker.OnSubscriptionChangeMessage);
            _signalR.Start();
            _signalR.OnStarted += delegate(string connectionHandle) { onSessionStarted?.Invoke(connectionHandle); };
        }

        public void AddMessageHandler(string topic, Action<Message> onMessage)
        {   
            _messageBroker.AddMessageHandler(topic, onMessage);
        }

        public void AddSubscriptionChangeMessageHandler(string topic, Action<SubscriptionChangeMessage> onSubscriptionChangeMessage)
        {
            _messageBroker.AddSubscriptionChangeMessageHandler(topic, onSubscriptionChangeMessage);
        }
        
        private void OnDestroy()
        {
            _signalR.Stop();
        }
    }
}