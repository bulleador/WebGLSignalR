using System;
using System.Collections.Generic;
using System.Linq;
using Lobby.Signal.Messages;
using UnityEngine;

namespace Lobby.Signal
{
    internal class SignalRMessageHandler
    {
        // todo - use a better data structure for this (dictionary is not ideal as some topics may be subscribed to multiple times by different handlers)
        private readonly Dictionary<string, Action<Message>> _messageHandlers = new();
        private readonly Dictionary<string, Action<SubscriptionChangeMessage>> _subscriptionChangeMessageHandlers = new();

        public void OnMessage(Message obj)
        {
            var topic = obj.Topic;
            var handlers = _messageHandlers.Keys.Where(topicString => topic.Contains(topicString)).ToArray();

            if (!handlers.Any())
            {
                Debug.LogWarning($"No handlers found for Message {topic}, payload: {obj.Payload}. Message will be ignored.");
                return;
            }
            
            foreach (var handler in handlers) 
                _messageHandlers[handler].Invoke(obj);
        }

        public void OnSubscriptionChangeMessage(SubscriptionChangeMessage obj)
        {
            var topic = obj.Topic;
            var handlers = _subscriptionChangeMessageHandlers.Keys.Where(topicString => topic.Contains(topicString)).ToArray();

            if (!handlers.Any())
            {
                Debug.LogWarning($"No handlers found for SubscriptionChangeMessage {topic}, status: {obj.Status}. Message will be ignored.");
                return;
            }
            
            foreach (var handler in handlers) 
                _subscriptionChangeMessageHandlers[handler].Invoke(obj);
        }

        public void AddMessageHandler(string topic, Action<Message> onMessage)
        {
            _messageHandlers.Add(topic, onMessage);
        }

        public void AddSubscriptionChangeMessageHandler(string topic, Action<SubscriptionChangeMessage> onSubscriptionChangeMessage)
        {
            _subscriptionChangeMessageHandlers.Add(topic, onSubscriptionChangeMessage);
        }
    }
}