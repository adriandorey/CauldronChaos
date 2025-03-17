using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbovePlayerUi : MonoBehaviour
{
    [SerializeField] private Image abovePlayerInteraction;
    [SerializeField] private Image interactionSprite;
    [SerializeField] private TextMeshProUGUI abovePlayerText;

        
    private void ChangeAboveUi(string interaction)
    {
        switch(interaction)
        {
            case "pickup":
                break;
            case "interaction":
                break;
            case "stir":
                break;
        }
    }
}
