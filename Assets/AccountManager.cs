using System;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using Random = UnityEngine.Random;

public class AccountManager : MonoBehaviour
{
    public static event Action OnAuthenticated;

    private void Start()
    {
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
        {
            CustomId = GetDeviceId(),
            CreateAccount = true
        }, OnLoginSuccess, OnLoginFailure);
    }

    private string GetDeviceId()
    {
        return SystemInfo.deviceUniqueIdentifier + Random.Range(0, 1000);
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