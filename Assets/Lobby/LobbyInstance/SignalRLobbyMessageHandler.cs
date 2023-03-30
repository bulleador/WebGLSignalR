using System;
using Lobby.SignalRWrapper.Messages;
using UnityEngine;

namespace Lobby.LobbyInstance
{
    public class SignalRLobbyMessageHandler
    {
        private readonly ObservableLobby _lobby;
        private readonly SignalRLobbyMessageConverter _signalRLobbyMessageConverter;

        private readonly bool _debug;

        public SignalRLobbyMessageHandler(ObservableLobby lobby, bool debug = false)
        {
            _lobby = lobby;
            _signalRLobbyMessageConverter = new SignalRLobbyMessageConverter();
            _debug = debug;
        }

        public void OnLobbyChangeMessage(Message message)
        {
            if (_debug)
                Debug.Log($"Lobby change message received - {_signalRLobbyMessageConverter.ToJson(message.Payload)}");

            try
            {
                var lobbyChangeMessage = _signalRLobbyMessageConverter.Convert<LobbyChangeMessage>(message);
                _lobby.ApplyOrQueueChanges(lobbyChangeMessage.Changes);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        public void OnLobbySubscriptionChangeMessage(SubscriptionChangeMessage message)
        {
            if (_debug)
                Debug.Log($"Lobby subscription change message received. Status: {message}");
            try
            {

                switch (message.Status)
                {
                    case "unsubscribeSuccess" when message.UnsubscribeReason == "MemberLeft":
                        _lobby.OnSubscriptionMessage(SubscriptionMessageType.UnsubscribedMemberLeft);
                        break;
                    case "unsubscribeSuccess" when message.UnsubscribeReason == "LobbyDeleted":
                        _lobby.OnSubscriptionMessage(SubscriptionMessageType.UnsubscribedLobbyDeleted);
                        break;
                    case "unsubscribeSuccess" when message.UnsubscribeReason == "MemberRemoved":
                        _lobby.OnSubscriptionMessage(SubscriptionMessageType.UnsubscribedMemberRemoved);
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
    }
}