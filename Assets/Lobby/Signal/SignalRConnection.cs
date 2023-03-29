using System;
using Lobby.Signal.Messages;
using Newtonsoft.Json;
using PlayFab;
using UnityEngine;
using UnityEngine.Networking;

namespace Lobby.Signal
{
    public class SignalRConnection
    {
        private string NegotiateUrl =>
            $"https://{PlayFabSettings.staticSettings.TitleId}.playfabapi.com/PubSub/Negotiate";

        private SignalR _signalR;

        private readonly Action<Message> _onReceiveMessage;
        private readonly Action<SubscriptionChangeMessage> _onReceiveSubscriptionChangeMessage;

        public event Action<string> OnStarted;
        public event Action OnStopped;

        public string ConnectionHandle { get; private set; }

        public SignalRConnection(Action<Message> onReceiveMessage,
            Action<SubscriptionChangeMessage> onReceiveSubscriptionChangeMessage)
        {
            _onReceiveMessage = onReceiveMessage;
            _onReceiveSubscriptionChangeMessage = onReceiveSubscriptionChangeMessage;
        }

        public void Start()
        {
            Negotiate();
        }

        private void Negotiate()
        {
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

            _signalR = new SignalR();
            _signalR.Init(url, accessToken);

#if UNITY_EDITOR
            _signalR.On("ReceiveMessage", _onReceiveMessage);
            _signalR.On("ReceiveSubscriptionChangeMessage", _onReceiveSubscriptionChangeMessage);
#elif UNITY_WEBGL
            _signalR.On("ReceiveMessage",
                json => { _onReceiveMessage.Invoke(JsonConvert.DeserializeObject<Message>(json)); });
            _signalR.On("ReceiveSubscriptionChangeMessage",
                json =>
                {
                    _onReceiveSubscriptionChangeMessage.Invoke(
                        JsonConvert.DeserializeObject<SubscriptionChangeMessage>(json));
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
                    OnStarted?.Invoke(ConnectionHandle);
                });

#elif UNITY_WEBGL
            _signalR.StartOrRecoverSession("00-84678fd69ae13e41fce1333289bcf482-22d157fb94ea4827-01",
                (response) =>
                {
                    var responseObj = JsonConvert.DeserializeObject<StartOrRecoverSessionResponse>(response);
                    Debug.Log($"Raw data - {response}");
                    Debug.Log($"Session started or recovered - {responseObj}");
                    ConnectionHandle = responseObj.NewConnectionHandle;
                    OnStarted?.Invoke(ConnectionHandle);
                });
#endif
        }

        public void Stop()
        {
            _signalR?.Stop();
        }
    }
}