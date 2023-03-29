using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UpCadeButton : MonoBehaviour
{
    [SerializeField] private GameObject throbber;
    [SerializeField] private GameObject text;

    private Button _button;
    private ButtonState _state;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _state = _button.interactable ? ButtonState.Interactable : ButtonState.NonInteractable;
    }

    public void SetState(ButtonState state)
    {
        switch (state)
        {
            case ButtonState.NonInteractable:
                _button.interactable = false;
                throbber.SetActive(false);
                text.SetActive(true);
                break;
            case ButtonState.Interactable:
                _button.interactable = true;
                throbber.SetActive(false);
                text.SetActive(true);
                break;
            case ButtonState.Loading:
                _button.interactable = false;
                throbber.SetActive(true);
                text.SetActive(false);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }
}

public enum ButtonState
{
    NonInteractable,
    Interactable,
    Loading,
}