using System;
using System.Threading.Tasks;
using Lobby.Signal.Logging;
using Lobby.Signal.Messages;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
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
                    var response = JsonConvert.DeserializeObject<PubSubNegotiationResponse>(webRequest.webRequest
                            .downloadHandler.text);
                    Connect(response.Url, response.AccessToken);
                }
            }
        }
        
        private static void Connect(string url, string accessToken)
        {
            Debug.Log("Connecting to SignalR... " + url);

            _signalR = new SignalR();
            
            _signalR.Init(url, accessToken);

            _signalR.On("ReceiveMessage", (Message message) => { Debug.Log("Received message: " + message); });
            _signalR.On("ReceiveSubscriptionChangeMessage", (SubscriptionChangeMessage message) => { Debug.Log("Received subscription change message: " + message); });
            
            _signalR.ConnectionClosed += (sender, args) =>
            {
                Debug.Log("Connection closed");
            };

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

            _signalR.Invoke("StartOrRecoverSession", new
            {
                traceParent = "00-0af7651916cd43dd8448eb211c80319c-b9c7f8223d7e8ad4-01",
            }, response =>
            {
                Debug.Log("Start or recover session response: " + response);
                var responseObj = JsonConvert.DeserializeObject<StartOrRecoverSessionResponse>(response.ToString());
                ConnectionHandle = responseObj.NewConnectionHandle;
            });
        }

    }
}