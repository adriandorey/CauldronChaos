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

    private Sprite PickIcon(string displayString)
    {
        Sprite icon = null; // Ensure icon has a default value
        var gamepad = Gamepad.current;

        if (gamepad is XInputControllerWindows)
        {
            icon = xboxIcons.GetSprite(displayString);
        }
        else if (gamepad is DualShockGamepad)
        {
            icon = ps4Icons.GetSprite(displayString);
        }
        else
        {
            Debug.Log("Gamepad is not XInputController or DualShockGamepad");
            icon = xboxIcons.GetSprite(displayString);
        }

        return icon;
    }


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
        
        if (FirstSelect.IsKeyboardControlling)
        {
            abovePlayerText.enabled = true;
            interactionSprite.sprite = keyboardIcon;
            interactionSprite.rectTransform.sizeDelta = new Vector2(1.2f, 0.6f);
            abovePlayerText.text = InputManager.Instance.PickupInputAction.GetBindingDisplayString(0);
        }
        else
        {
            abovePlayerText.enabled = false;
            interactionSprite.rectTransform.sizeDelta = new Vector2(0.6f, 0.6f);
            interactionSprite.sprite = PickIcon(InputManager.Instance.PickupInputAction.GetBindingDisplayString(1));
        }
    }

    private void ShowStir()
    {
        abovePlayerInteraction.enabled = true;
        stirUI.SetActive(true);
        
        if (FirstSelect.IsKeyboardControlling)
        {
            stirLeft.actionIcon.sprite = keyboardIcon;
            stirLeft.actionText.enabled = true;
            stirLeft.actionText.text = InputManager.Instance.StirCcAction.GetBindingDisplayString(0);
            
            stirRight.actionIcon.sprite = keyboardIcon;
            stirRight.actionText.enabled = true;
            stirRight.actionText.text = InputManager.Instance.StirCAction.GetBindingDisplayString(0);
        }
        else
        {
            stirLeft.actionIcon.sprite = PickIcon(InputManager.Instance.StirCcAction.GetBindingDisplayString(1));
            stirLeft.actionText.enabled = false;
            
            stirRight.actionIcon.sprite = PickIcon(InputManager.Instance.StirCAction.GetBindingDisplayString(1));
            stirRight.actionText.enabled = false;
        }
    }

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

[Serializable]
public struct GamepadIcons
{
    public Sprite buttonSouth;
    public Sprite buttonNorth;
    public Sprite buttonEast;
    public Sprite buttonWest;
    public Sprite startButton;
    public Sprite selectButton;
    public Sprite leftTrigger;
    public Sprite rightTrigger;
    public Sprite leftShoulder;
    public Sprite rightShoulder;
    public Sprite dpad;
    public Sprite dpadUp;
    public Sprite dpadDown;
    public Sprite dpadLeft;
    public Sprite dpadRight;
    public Sprite leftStick;
    public Sprite rightStick;
    public Sprite leftStickPress;
    public Sprite rightStickPress;

    public Sprite GetSprite(string controlPath)
    {
        // From the input system, we get the path of the control on device. So we can just
        // map from that to the sprites we have for gamepads.
        switch (controlPath)
        {
            case "buttonSouth": case "A": case "Cross": return buttonSouth;
            case "buttonNorth": case "Y": case "Triangle": return buttonNorth;
            case "buttonEast": case "B": case "Circle": return buttonEast;
            case "buttonWest": case "X": case "Square": return buttonWest;
            case "start": case "Options": return startButton;
            case "select": case "Share": return selectButton;
            case "leftTrigger": case "LT": case "L2": return leftTrigger;
            case "rightTrigger": case "RT": case "R2": return rightTrigger;
            case "leftShoulder": case "leftBumper": case "L1": return leftShoulder;
            case "rightShoulder": case "rightBumper": case "R1": return rightShoulder;
            case "dpad": return dpad;
            case "dpad/up": return dpadUp;
            case "dpad/down": return dpadDown;
            case "dpad/left": return dpadLeft;
            case "dpad/right": return dpadRight;
            case "leftStick": return leftStick;
            case "rightStick": return rightStick;
            case "leftStickPress": return leftStickPress;
            case "rightStickPress": return rightStickPress;
        }

        return null;
    }
}