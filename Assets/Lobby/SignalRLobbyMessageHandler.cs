using System;
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

            try
            {
                var lobbyChangeMessage = _signalRLobbyMessageConverter.Convert<LobbyChangeMessage>(message);
                _lobbyController.Lobby.ApplyOrQueueChanges(lobbyChangeMessage.Changes);
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
                        _lobbyController.OnSubscriptionMessage(SubscriptionMessageType.UnsubscribedMemberLeft);
                        break;
                    case "unsubscribeSuccess" when message.UnsubscribeReason == "LobbyDeleted":
                        _lobbyController.OnSubscriptionMessage(SubscriptionMessageType.UnsubscribedLobbyDeleted);
                        break;
                    case "unsubscribeSuccess" when message.UnsubscribeReason == "MemberRemoved": // todo might be wrong
                        _lobbyController.OnSubscriptionMessage(SubscriptionMessageType.UnsubscribedMemberRemoved);
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