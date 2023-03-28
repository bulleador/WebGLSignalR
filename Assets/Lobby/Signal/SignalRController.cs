using System;
using Lobby.Signal.Messages;
using UnityEngine;

namespace Lobby.Signal
{
    public class SignalRController : MonoBehaviour
    {
        private SignalRConnection _signalR;
        private SignalRMessageHandler _messageHandler;
        
        public string ConnectionHandle => _signalR.ConnectionHandle;
        
        public void Initialise(Action<string> onSessionStarted)
        {
            _messageHandler = new SignalRMessageHandler();
            
            _signalR = new SignalRConnection(_messageHandler.OnMessage, _messageHandler.OnSubscriptionChangeMessage);
            _signalR.Start();
            _signalR.OnStarted += delegate(string connectionHandle) { onSessionStarted?.Invoke(connectionHandle); };
        }

        public void AddMessageHandler(string topic, Action<Message> onMessage)
        {   
            _messageHandler.AddMessageHandler(topic, onMessage);
        }

        public void AddSubscriptionChangeMessageHandler(string topic, Action<SubscriptionChangeMessage> onSubscriptionChangeMessage)
        {
            _messageHandler.AddSubscriptionChangeMessageHandler(topic, onSubscriptionChangeMessage);
        }
        
        private void OnDestroy()
        {
            _signalR.Stop();
        }
    }
}