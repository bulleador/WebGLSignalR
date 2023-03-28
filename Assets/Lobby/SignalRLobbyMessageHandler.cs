using Lobby.Signal.Messages;
using UnityEngine;

namespace Lobby
{
    public class SignalRLobbyMessageHandler
    {
        private readonly LobbyController _lobbyController;
        private readonly MessageConverter _messageConverter;
        
        public SignalRLobbyMessageHandler(LobbyController lobbyController)
        { 
            _lobbyController = lobbyController;
            _messageConverter = new MessageConverter();
        }

        public void OnLobbyChangeMessage(Message message)
        {
            Debug.Log($"Lobby change message received - {_messageConverter.ToJson(message.Payload)}");
            var lobbyChangeMessage = _messageConverter.Convert<LobbyChangeMessage>(message);
        }

        public void OnLobbySubscriptionChangeMessage(SubscriptionChangeMessage message)
        {
            Debug.Log($"Lobby subscription change message received. Status: {message.Status}");
        }
    }
}