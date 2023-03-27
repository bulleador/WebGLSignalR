using System;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class AccountManager : MonoBehaviour
{
    public static event Action OnAuthenticated;

    private void Start()
    {
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        }, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginFailure(PlayFabError obj)
    {
        Debug.LogError(obj.GenerateErrorReport());
    }

    private void OnLoginSuccess(LoginResult obj)
    {
        Debug.Log($"Logged in successfully as {obj.PlayFabId}");
        OnAuthenticated?.Invoke();
    }
}