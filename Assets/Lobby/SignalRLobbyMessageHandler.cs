using Lobby.SignalRWrapper.Messages;
using UnityEngine;

namespace Lobby
{
    public class SignalRLobbyMessageHandler
    {
        private readonly LobbyController _lobbyController;
        private readonly SignalRLobbyMessageConverter _signalRLobbyMessageConverter;

        private readonly bool _debug;
        
        public SignalRLobbyMessageHandler(LobbyController lobbyController, bool debug = false)
        { 
            _lobbyController = lobbyController;
            _signalRLobbyMessageConverter = new SignalRLobbyMessageConverter();
            _debug = debug;
        }

        public void OnLobbyChangeMessage(Message message)
        {
            if (_debug)
                Debug.Log($"Lobby change message received - {_signalRLobbyMessageConverter.ToJson(message.Payload)}");
            
            var lobbyChangeMessage = _signalRLobbyMessageConverter.Convert<LobbyChangeMessage>(message);
            _lobbyController.ApplyChanges(lobbyChangeMessage.Changes);
        }

        public void OnLobbySubscriptionChangeMessage(SubscriptionChangeMessage message)
        {
            if (_debug)
                Debug.Log($"Lobby subscription change message received. Status: {message}");
            
            // TODO connection status handling
        }
    }
}