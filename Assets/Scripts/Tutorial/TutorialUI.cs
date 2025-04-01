using System;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class TutorialUI : MonoBehaviour
{
    [Header("Pop Up")]
    [SerializeField] private GameObject popUp;
    [SerializeField] private TextMeshProUGUI tutorialText;

    [Header("Tutorial Pop up")]
    [Tooltip("This time is in seconds")]
    [SerializeField] private float popUpTime = 5f;
    private CustomTimer _popUpTimer;


    private void Start()
    {
        popUp.SetActive(false);
        _popUpTimer = new CustomTimer(popUpTime, false);
    }

    private void Update()
    {
        if (!_popUpTimer.isRunning) return;

        if (_popUpTimer.UpdateTimer())
        {
            _popUpTimer.StopTimer();
            DeactivatePopUp();
        }
    }

    /// <summary> Shows pop up for tutorial. Shows the string input in and then starts timer </summary>
    internal void ActivatePopUp(string tutorial)
    {
        popUp.SetActive(true);
        tutorialText.text = tutorial;
        // popUp.transform.DOPunchScale(Vector3.one, 1f, 1, 1f );
        popUp.transform.DOScale(Vector3.one, 1f);
        _popUpTimer.StartTimer();
    }

    /// <summary> Deactivates Pop up </summary>
    internal void DeactivatePopUp()
    {
        // Debug.Log("Deactivating pop up");
        popUp.transform.DOScale(Vector3.zero, 0.8f).OnComplete(() =>
        {
            popUp.SetActive(false);
        });
    }
}
