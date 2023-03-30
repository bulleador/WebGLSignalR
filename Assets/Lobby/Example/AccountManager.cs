using System;
using Lobby;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using Random = UnityEngine.Random;

public class AccountManager : MonoBehaviour
{
    private void Start()
    {
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
        {
            CustomId = GetDeviceId(),
            CreateAccount = true
        }, OnLoginSuccess, OnLoginFailure);

        void OnLoginFailure(PlayFabError obj)
        {
            Debug.LogError(obj.GenerateErrorReport());
        }

        void OnLoginSuccess(LoginResult obj)
        {
            Debug.Log($"Logged in successfully as {obj.PlayFabId}");
            FindObjectOfType<LobbyController>().Initialise();
        }
    }

    private string GetDeviceId()
    {
        return SystemInfo.deviceUniqueIdentifier + Random.Range(0, 1000);
    }

}