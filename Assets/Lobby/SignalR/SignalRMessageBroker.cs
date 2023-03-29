using System;
using System.Collections.Generic;
using System.Linq;
using Lobby.SignalR.Messages;
using UnityEngine;

namespace Lobby.SignalR
{
    internal class SignalRMessageBroker
    {
        private readonly List<MessageHandler> _messageHandlers = new();
        private readonly List<SubscriptionChangeMessageHandler> _subscriptionChangeMessageHandlers = new();
        
        private struct MessageHandler
        {
            public string Topic;
            public Action<Message> Handler;

            public void Invoke(Message message)
            {
                Handler.Invoke(message);
            }
        }
        
        private struct SubscriptionChangeMessageHandler
        {
            public string Topic;
            public Action<SubscriptionChangeMessage> Handler;
        }

        public void OnMessage(Message obj)
        {
            var topic = obj.Topic;
            var handlers = _messageHandlers.Where(handler => topic.Contains(handler.Topic)).ToArray();

            if (!handlers.Any())
            {
                Debug.LogWarning($"No handlers found for Message {topic}, payload: {obj.Payload}. Message will be ignored.");
                return;
            }
            
            foreach (var handler in handlers) 
                handler.Invoke(obj);
        }

        public void OnSubscriptionChangeMessage(SubscriptionChangeMessage obj)
        {
            var topic = obj.Topic;
            var handlers = _subscriptionChangeMessageHandlers.Where(handler => topic.Contains(handler.Topic)).ToArray();

            if (!handlers.Any())
            {
                Debug.LogWarning($"No handlers found for SubscriptionChangeMessage {topic}, status: {obj.Status}. Message will be ignored.");
                return;
            }
            
            foreach (var handler in handlers) 
                handler.Handler.Invoke(obj);
        }

        public void AddMessageHandler(string topic, Action<Message> onMessage)
        {
            var handler = new MessageHandler
            {
                Topic = topic,
                Handler = onMessage
            };
            _messageHandlers.Add(handler);
        }

        public void AddSubscriptionChangeMessageHandler(string topic, Action<SubscriptionChangeMessage> onSubscriptionChangeMessage)
        {
            var handler = new SubscriptionChangeMessageHandler
            {
                Topic = topic,
                Handler = onSubscriptionChangeMessage
            };
            
            _subscriptionChangeMessageHandlers.Add(handler);
        }
    }
}