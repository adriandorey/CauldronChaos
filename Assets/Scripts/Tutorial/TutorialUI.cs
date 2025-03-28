using System;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class TutorialUI : MonoBehaviour
{
    [Header("Pop Up")]
    [SerializeField] private GameObject popUp;
    [SerializeField] private TextMeshProUGUI tutorialText;


    private void Start()
    {
        popUp.SetActive(false);
        // popUp.transform.localScale = Vector3.zero;
    }

    internal void ActivatePopUp(string tutorial)
    {
        popUp.SetActive(true);
        tutorialText.text = tutorial;
        popUp.transform.DOPunchScale(Vector3.one, 1f, 1, 1f );
    }

    internal void DeactivatePopUp()
    {
        popUp.transform.DOScale(Vector3.zero, 0.8f).OnComplete(() =>
        {
            popUp.SetActive(false);
        });
        
    }
}
