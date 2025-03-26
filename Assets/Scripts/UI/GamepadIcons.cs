using System;
using UnityEngine;

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
            case "leftShoulder": case "leftBumper": case "L1": case "LB": return leftShoulder;
            case "rightShoulder": case "rightBumper": case "R1": case "RB": return rightShoulder;
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