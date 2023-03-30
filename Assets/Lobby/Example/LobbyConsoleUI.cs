using System;
using System.Text.RegularExpressions;
using Lobby;
using Lobby.LobbyInstance;
using TMPro;
using UnityEngine;

public class LobbyConsoleUI : MonoBehaviour
{
    [SerializeField] private LobbyController lobbyController;

    [SerializeField] private TMP_InputField unityLog;
    [SerializeField] private TMP_InputField lobbyLog;

    private void OnEnable()
    {
        Application.logMessageReceivedThreaded += OnLogMessageReceived;
        
        lobbyController.OnLobbyJoined += OnLobbyJoined;
        lobbyController.OnLobbyLeft += OnLobbyLeft;
    }
    
    private void OnLobbyJoined(ObservableLobby lobby, bool asOwner)
    {
        LogLobby(asOwner ? $"Lobby {lobby.LobbyId} created" : $"Lobby {lobby.LobbyId} joined");

        lobby.OnLobbyMemberAdded += member => LogLobby($"Member added: {member.MemberEntity.Id}");
        lobby.OnLobbyMemberRemoved += member => LogLobby($"Member removed: {member.MemberEntity.Id}");
        lobby.OnLobbyMemberDataChanged += member => LogLobby($"Member data changed: {member.MemberEntity.Id}");
        lobby.OnLobbyOwnerChanged += owner => LogLobby($"Owner changed: {owner.Id}");
        lobby.OnLobbyDataChanged += data => LogLobby($"Lobby data changed: {string.Join(",", data.Keys)}");
    }

    private void OnLobbyLeft(ObservableLobby lobby, LobbyLeaveReason reason)
    {
        LogLobbyLocal($"Lobby {lobby.LobbyId} left: {reason}");
    }

    private void OnLogMessageReceived(string condition, string stacktrace, LogType type)
    {
        var color = type switch
        {
            LogType.Error => Color.red,
            LogType.Exception => Color.red,
            LogType.Warning => Color.yellow,
            _ => Color.gray
        };

        LogUnity(condition, color);
    }

    private void LogLobbyLocal(string message)
    {
        message = FormatMessage(message, "Lobby (Local)", Color.cyan);
        lobbyLog.text += message;
    }

    private void LogLobby(string message, Color? color = null)
    {
        message = FormatMessage(message, "Lobby", color ?? Color.green);
        lobbyLog.text += message;
    }

    private void LogUnity(string message, Color? color = null)
    {
        message = FormatMessage(message, "Unity", color ?? Color.white);
        unityLog.text += message;
    }

    private string FormatMessage(string message, string tag, Color color)
    {
        var intended = Regex.Replace(message, $"{Environment.NewLine}", $"\t{Environment.NewLine}");
        var tagged = $"[{DateTime.Now.ToShortTimeString()}] [{tag}] {intended}\n";
        var colored = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{tagged}</color>";

        return colored;
    }
}