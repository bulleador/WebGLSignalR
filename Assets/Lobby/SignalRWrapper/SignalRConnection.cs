using System;
using Lobby.SignalRWrapper.Messages;
using Lobby.SignalRWrapper.PlayFab;
using Newtonsoft.Json;
using PlayFab;
using UnityEngine;
using UnityEngine.Networking;

namespace Lobby.SignalRWrapper
{
    public class SignalRConnection
    {
        private string NegotiateUrl =>
            $"https://{PlayFabSettings.staticSettings.TitleId}.playfabapi.com/PubSub/Negotiate";

        private global::SignalR.SignalR _signalR;

        private readonly Action<Message> _onReceiveMessage;
        private readonly Action<SubscriptionChangeMessage> _onReceiveSubscriptionChangeMessage;

        private readonly SignalRMessageBroker _messageBroker;

        public event Action OnStarted;
        public event Action OnStopped;

        public string ConnectionHandle { get; private set; }

        public SignalRConnection(SignalRMessageBroker messageBroker)
        {
            _messageBroker = messageBroker;
        }

        public void Start()
        {
            Negotiate();
        }

        private void Negotiate()
        {
            if (!PlayFabClientAPI.IsClientLoggedIn())
            {
                Debug.LogError("PlayFab client is not logged in");
                return;
            }

            Debug.Log("Negotiating...");
            var request = new UnityWebRequest(NegotiateUrl, "POST");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("X-EntityToken", PlayFabSettings.staticPlayer.EntityToken);

            var operation = request.SendWebRequest();
            operation.completed += OnNegotiateCompleted;

            void OnNegotiateCompleted(AsyncOperation obj)
            {
                var webRequest = obj as UnityWebRequestAsyncOperation;
                if (webRequest!.webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(webRequest.webRequest.error);
                }
                else
                {
                    var text = webRequest.webRequest.downloadHandler.text;
                    var response = JsonConvert.DeserializeObject<PubSubNegotiationResponse>(text);

                    Connect(response.Url, response.AccessToken);
                }
            }
        }

        private void Connect(string url, string accessToken)
        {
            Debug.Log("Connecting to SignalR...");

            _signalR = new global::SignalR.SignalR();
            _signalR.Init(url, accessToken);

#if UNITY_EDITOR
            _signalR.On<Message>("ReceiveMessage", _messageBroker.OnMessage);
            _signalR.On<SubscriptionChangeMessage>("ReceiveSubscriptionChangeMessage",
                _messageBroker.OnSubscriptionChangeMessage);
#elif UNITY_WEBGL
            _signalR.On("ReceiveMessage",
                json =>
                {
                    var message = JsonConvert.DeserializeObject<Message>(json);
                    _messageBroker.OnMessage(message);
                });
            _signalR.On("ReceiveSubscriptionChangeMessage",
                json =>
                {
                    var message = JsonConvert.DeserializeObject<SubscriptionChangeMessage>(json);
                    _messageBroker.OnSubscriptionChangeMessage(message);
                });
#endif

            _signalR.ConnectionStarted += delegate { StartOrRecoverSession(); };
            _signalR.ConnectionClosed += delegate { OnStopped?.Invoke(); };

            _signalR.Connect();
        }

        private void StartOrRecoverSession()
        {
            Debug.Log("Starting or recovering session...");

#if UNITY_EDITOR
            _signalR.StartOrRecoverSession("00-84678fd69ae13e41fce1333289bcf482-22d157fb94ea4827-01",
                (response) =>
                {
                    Debug.Log($"Session started or recovered - {response}");
                    ConnectionHandle = response.NewConnectionHandle;
                    OnStarted?.Invoke();
                });

#elif UNITY_WEBGL
            _signalR.StartOrRecoverSession("00-84678fd69ae13e41fce1333289bcf482-22d157fb94ea4827-01",
                (response) =>
                {
                    var responseObj = JsonConvert.DeserializeObject<StartOrRecoverSessionResponse>(response);
                    Debug.Log($"Raw data - {response}");
                    Debug.Log($"Session started or recovered - {responseObj}");
                    ConnectionHandle = responseObj.NewConnectionHandle;
                    OnStarted?.Invoke();
                });
#endif
        }

        public void Stop()
        {
            _signalR?.Stop();
        }
    }
}