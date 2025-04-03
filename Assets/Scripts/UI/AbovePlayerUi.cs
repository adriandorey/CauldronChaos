using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

public class AbovePlayerUi : MonoBehaviour
{
    [SerializeField] private Canvas abovePlayerInteraction;
    [SerializeField] private Sprite keyboardIcon;

    [Header("Action UI")]
    [SerializeField] private Image interactionSprite;
    [SerializeField] private TextMeshProUGUI abovePlayerText;

    [Header("Stir UI")]
    [SerializeField] private GameObject stirUI;
    [SerializeField] private Stir stirLeft;
    [SerializeField] private Stir stirRight;

    [Header("Gamepad Icons")]
    [SerializeField] private GamepadIcons xboxIcons;
    [SerializeField] private GamepadIcons ps4Icons;

    private void Start()
    {
        HideUI();
    }


    #region Enable / Disable / Destroy
    private void OnEnable()
    {
        Actions.OnShowInteraction += ShowInteract;
        Actions.OnShowStir += ShowStir;
        Actions.OnShowPickup += ShowPickup;
        Actions.OnHideUI += HideUI;
    }

    private void OnDisable()
    {
        Actions.OnShowInteraction -= ShowInteract;
        Actions.OnShowStir -= ShowStir;
        Actions.OnShowPickup -= ShowPickup;
        Actions.OnHideUI -= HideUI;
    }

    private void OnDestroy()
    {
        Actions.OnShowInteraction -= ShowInteract;
        Actions.OnShowStir -= ShowStir;
        Actions.OnShowPickup -= ShowPickup;
        Actions.OnHideUI -= HideUI;
    }

    #endregion

    private void Update()
    {
        // if interaction sprite is enabled turn off the stir ui
        if(interactionSprite.enabled)
        {
            stirUI.SetActive(false);
        }
    }

    // Picks icon for controller 
    private Sprite PickIcon(string displayString)
    {
        return Gamepad.current switch
        {
            XInputControllerWindows => xboxIcons.GetSprite(displayString),
            DualShockGamepad => ps4Icons.GetSprite(displayString),
            _ => xboxIcons.GetSprite(displayString) // Fallback to Xbox
        };
    }

    // Show interaction above player
    private void ShowInteract()
    {
        abovePlayerInteraction.enabled = true;
        interactionSprite.enabled = true;
        
        if (FirstSelect.IsKeyboardControlling)
        {
            abovePlayerText.enabled = true;
            interactionSprite.sprite = keyboardIcon;
            interactionSprite.rectTransform.sizeDelta = new Vector2(0.6f, 0.6f);
            abovePlayerText.text = InputManager.Instance.InteractInputAction.GetBindingDisplayString(0);
        }
        else
        {
            abovePlayerText.enabled = false;
            interactionSprite.rectTransform.sizeDelta = new Vector2(0.6f, 0.6f);
            interactionSprite.sprite = PickIcon(InputManager.Instance.InteractInputAction.GetBindingDisplayString(1));
        }
    }

    private void ShowPickup()
    {
        abovePlayerInteraction.enabled = true;
        interactionSprite.enabled = true;
        
        if (FirstSelect.IsControllerControlling)
        {
            abovePlayerText.enabled = false;
            interactionSprite.rectTransform.sizeDelta = new Vector2(0.6f, 0.6f);
            interactionSprite.sprite = PickIcon(InputManager.Instance.PickupInputAction.GetBindingDisplayString(1));
        }
        else
        {
            abovePlayerText.enabled = true;
            interactionSprite.sprite = keyboardIcon;
            interactionSprite.rectTransform.sizeDelta = new Vector2(1.2f, 0.6f);
            abovePlayerText.text = InputManager.Instance.PickupInputAction.GetBindingDisplayString(0);
        }
    }

    private void ShowStir()
    {
        abovePlayerInteraction.enabled = true;
        stirUI.SetActive(true);
        
        if (FirstSelect.IsControllerControlling)
        {
            stirLeft.actionIcon.sprite = PickIcon(InputManager.Instance.StirCcAction.GetBindingDisplayString(1));
            stirLeft.actionText.enabled = false;
            
            stirRight.actionIcon.sprite = PickIcon(InputManager.Instance.StirCAction.GetBindingDisplayString(1));
            stirRight.actionText.enabled = false;
        }
        else
        {
            stirLeft.actionIcon.sprite = keyboardIcon;
            stirLeft.actionText.enabled = true;
            stirLeft.actionText.text = InputManager.Instance.StirCcAction.GetBindingDisplayString(0);
            
            stirRight.actionIcon.sprite = keyboardIcon;
            stirRight.actionText.enabled = true;
            stirRight.actionText.text = InputManager.Instance.StirCAction.GetBindingDisplayString(0);
        }
    }

    // Turns off all the UI
    private void HideUI()
    {
        abovePlayerInteraction.enabled = false;
        interactionSprite.enabled = false;
        abovePlayerText.enabled = false;
        stirUI.SetActive(false);
    }
}

[Serializable]
public struct Stir
{
    public GameObject stirObj;
    public Image actionIcon;
    public TextMeshProUGUI actionText;
}