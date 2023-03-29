using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using Lobby.SignalR.PlayFab;
using Microsoft.AspNetCore.Http.Connections;
using UnityEngine;

#if !UNITY_EDITOR
using AOT;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
#endif

public class ConnectionEventArgs : EventArgs
{
    public string ConnectionId { get; set; }
}

public class SignalR
{
    private static SignalR instance;

    public SignalR()
    {
        instance = this;
    }

    private static void OnConnectionStarted(string connectionId)
    {
        var args = new ConnectionEventArgs
        {
            ConnectionId = connectionId
        };
        instance.ConnectionStarted?.Invoke(instance, args);
    }

    public event EventHandler<ConnectionEventArgs> ConnectionStarted;

    private static void OnConnectionClosed(string connectionId)
    {
        var args = new ConnectionEventArgs
        {
            ConnectionId = connectionId
        };
        instance.ConnectionClosed?.Invoke(instance, args);
    }

    public event EventHandler<ConnectionEventArgs> ConnectionClosed;

#if UNITY_EDITOR || PLATFORM_SUPPORTS_MONO
    private HubConnection connection;
    private static string lastConnectionId;

    public void Init(string url, string accessToken)
    {
        try
        {
            connection = new HubConnectionBuilder()
                .WithUrl(url, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(accessToken);
                    options.Transports = HttpTransportType.LongPolling;
                })
                .Build();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public async void Connect()
    {
        try
        {
            await connection.StartAsync();

            lastConnectionId = connection.ConnectionId;

            connection.Closed -= OnConnectionClosedEvent;
            connection.Reconnecting -= OnConnectionReconnectingEvent;
            connection.Reconnected -= OnConnectionReconnectedEvent;

            connection.Closed += OnConnectionClosedEvent;
            connection.Reconnecting += OnConnectionReconnectingEvent;
            connection.Reconnected += OnConnectionReconnectedEvent;

            OnConnectionStarted(lastConnectionId);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public async void Stop()
    {
        try
        {
            await connection.StopAsync();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public async void StartOrRecoverSession(string traceParent, Action<StartOrRecoverSessionResponse> onResponse)
    {
        var result = await connection.InvokeAsync<StartOrRecoverSessionResponse>("StartOrRecoverSession", new
        {
            traceParent
        });

        onResponse(result);
    }

    private static Task OnConnectionClosedEvent(Exception exception)
    {
        if (exception != null)
        {
            Debug.LogError(exception.Message);
        }

        OnConnectionClosed(lastConnectionId);

        return Task.CompletedTask;
    }

    private static Task OnConnectionReconnectingEvent(Exception exception)
    {
        Debug.Log($"Connection started reconnecting due to an error: {exception.Message}");

        return Task.CompletedTask;
    }

    private static Task OnConnectionReconnectedEvent(string connectionId)
    {
        Debug.Log($"Connection successfully reconnected. The ConnectionId is now: {connectionId}");

        lastConnectionId = connectionId;

        OnConnectionStarted(lastConnectionId);

        return Task.CompletedTask;
    }

    #region Invoke Editor

    public async void Invoke(string methodName, string arg1, Action<string> onResponse)
    {
        Debug.Log($"Invoke arg: {arg1}");
        var response = await connection.InvokeAsync<string>(methodName, arg1);
        onResponse(response);
    }

    #endregion

    #region On Editor

    public void On<T1>(string methodName, Action<T1> handler) =>
        connection.On(methodName, (T1 arg1) => handler.Invoke(arg1));

    #endregion

#elif UNITY_WEBGL
    #region Init JS

    [DllImport("__Internal")]
    private static extern void InitJs(string hubUrl, string accessToken);

    public void Init(string url, string accessToken)
    {
        InitJs(url, accessToken);
    }

    #endregion

    #region Stop JS

    [DllImport("__Internal")]
    private static extern void StopJs();

    public void Stop()
    {
        StopJs();
    }

    #endregion

    #region Connect JS

    [DllImport("__Internal")]
    private static extern void ConnectJs(Action<string> connectedCallback, Action<string> disconnectedCallback);


    #region StartOrRecoverSessionJs

    [DllImport("__Internal")]
    private static extern void StartOrRecoverSessionJs(string traceParent, Action<string> responseHandler);

    public void StartOrRecoverSession(string traceParent, Action<string> responseHandler)
    {
        _responseHandler = responseHandler;
        StartOrRecoverSessionJs(traceParent, ResponseCallback);
    }

    #endregion


    [MonoPInvokeCallback(typeof(Action<string>))]
    private static void ConnectedCallback(string connectionId)
    {
        OnConnectionStarted(connectionId);
    }

    [MonoPInvokeCallback(typeof(Action<string>))]
    private static void DisconnectedCallback(string connectionId)
    {
        OnConnectionClosed(connectionId);
    }

    public void Connect()
    {
        ConnectJs(ConnectedCallback, DisconnectedCallback);
    }

    #endregion

    #region Invoke JS

    [DllImport("__Internal")]
    private static extern void InvokeJs(string methodName, string arg);

    [MonoPInvokeCallback(typeof(Action<string>))]
    private static void ResponseCallback(string response)
    {
        _responseHandler?.Invoke(response);
    }

    private static Action<string> _responseHandler;

    public void Invoke(string methodName, string arg)
    {
        InvokeJs(methodName, arg);
    }

    #endregion

    #region On JS

    private delegate void HandlerAction(string arg);

    private static readonly Dictionary<string, HandlerAction> Handlers = new();

    [DllImport("__Internal")]
    private static extern void OnJs(string methodName, Action<string, string> handlerCallback);

    [MonoPInvokeCallback(typeof(Action<string, string>))]
    private static void HandlerCallback1(string methodName, string arg)
    {
        Handlers.TryGetValue(methodName, out HandlerAction handler);
        handler?.Invoke(arg);
    }

    public void On(string methodName, Action<string> handler)
    {
        Handlers.Add(methodName, arg => handler(arg));
        OnJs(methodName, HandlerCallback1);
    }

    #endregion

#else
#error PLATFORM NOT SUPPORTED

#endif
}