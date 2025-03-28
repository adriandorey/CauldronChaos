using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
   [SerializeField] private Canvas loadingScreen;
   [SerializeField] private Slider loadingBar;
   [SerializeField] private TextMeshProUGUI loadingText;

   private void Awake()
   {
      loadingScreen.enabled = false;
   }

   internal void Show()
   {
      loadingScreen.enabled = true;
      loadingText.text = "Loading...";
   }

   internal void Hide()
   {
      loadingScreen.enabled = false;
   }

   internal void UpdateProgress(float progress)
   {
      loadingBar.value = progress;
   }

   internal void ShowPressAnyKey()
   {
      loadingText.text = "Press any key to continue...";
   }
}
