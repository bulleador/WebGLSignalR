using Lobby.Signal.Messages;
using Newtonsoft.Json;
using PlayFab;
using UnityEngine;
using UnityEngine.Networking;

namespace Lobby.Signal
{
    public class SignalRController : MonoBehaviour
    {
        private static string NegotiateUrl =>
            $"https://{PlayFabSettings.staticSettings.TitleId}.playfabapi.com/PubSub/Negotiate";

        public static string ConnectionHandle { get; private set; }

        private static SignalR _signalR;
        
        private void Awake()
        {
            AccountManager.OnAuthenticated += Initialise;
        }

        private void OnDestroy()
        {
            _signalR.Stop();
        }

        private static void Initialise()
        {
            Negotiate();
        }

        private static void Negotiate()
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
                    var response =
                        JsonConvert.DeserializeObject<PubSubNegotiationResponse>(webRequest.webRequest.downloadHandler
                            .text);
                    
                    Connect(response.Url, response.AccessToken);
                }
            }
        }

        private static void Connect(string url, string accessToken)
        {
            Debug.Log($"Connecting to SignalR...\n" +
                      $"URL: {url}\n" +
                      $"AccessToken: {accessToken}");

            _signalR = new SignalR();

            _signalR.Init(url, accessToken);

            _signalR.On("ReceiveMessage", (Message message) => { Debug.Log("Received message: " + message); });
            _signalR.On("ReceiveSubscriptionChangeMessage",
                (SubscriptionChangeMessage message) =>
                {
                    Debug.Log("Received subscription change message: " + message);
                });

            _signalR.ConnectionClosed += (sender, args) => { Debug.Log("Connection closed"); };

            _signalR.ConnectionStarted += (sender, args) =>
            {
                Debug.Log($"Connection started");
                StartOrRecoverSession();
            };

            _signalR.Connect();
        }

        private static void StartOrRecoverSession()
        {
            Debug.Log("Starting or recovering session...");
            
            #if UNITY_EDITOR
            _signalR.StartOrRecoverSession("00-84678fd69ae13e41fce1333289bcf482-22d157fb94ea4827-01",
                (response) =>
                {
                    Debug.Log($"Session started or recovered - {response}");
                    ConnectionHandle = response.NewConnectionHandle;
                });
            
            #elif UNITY_WEBGL
            
            _signalR.StartOrRecoverSession("00-84678fd69ae13e41fce1333289bcf482-22d157fb94ea4827-01",
                (response) =>
                {
                    var responseObj = JsonConvert.DeserializeObject<StartOrRecoverSessionResponse>(response);
                    Debug.Log($"Raw data - {response}");
                    Debug.Log($"Session started or recovered - {responseObj}");
                    ConnectionHandle = responseObj.NewConnectionHandle;
                });
            
            #endif
        }
    }
}