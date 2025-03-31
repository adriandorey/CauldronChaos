using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using System;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private LoadingUI loadingUI;
    [SerializeField] private SceneTransition sceneTransition;
    [SerializeField] private DayManager dayManager;

    private string _sceneName;

    public void Start()
    {
        sceneLoader.OnProgressUpdated += loadingUI.UpdateProgress;
        sceneLoader.OnSceneReady += ShowPressAnyKeyPrompt;
    }


    public void LoadScene(string sceneName)
    {
        // Prevents reloading the main menu if you're already in the main menu
        if (SceneManager.GetActiveScene().name == "MainMenu" && sceneName == "MainMenu")
        {
            Actions.OnStateChange("MainMenu");
            return;
        }

        // Scene Transition, Fades out then completes these actions
        sceneTransition.FadeOut(() =>
        {
            _sceneName = sceneName;
            loadingUI.Show();
            Actions.OnStateChange("Loading");
            sceneLoader.LoadScene(sceneName);

            if (_sceneName.StartsWith("Day"))
                Actions.OnActivateHowToPlay?.Invoke(true);
        });
    }

    private void ShowPressAnyKeyPrompt()
    {
        if (_sceneName == "MainMenu" || _sceneName == "LevelSelect")
        {
            Actions.OnStateChange(_sceneName == "MainMenu" ? "MainMenu" : "LevelSelect");
            loadingUI.Hide();
            sceneTransition.FadeIn();
            return;
        }

        loadingUI.ShowPressAnyKey();
        StartCoroutine(WaitForAnyKeyPress());
    }


    // Used for loading screen between level selection or main menu and gameplay
    private IEnumerator WaitForAnyKeyPress()
    {
        // Cache the keyboard and gamepad references
        var keyboard = Keyboard.current;
        var gamepad = Gamepad.current;

        while (true)
        {
            if (keyboard?.anyKey.wasPressedThisFrame == true ||
                (gamepad != null && gamepad.allControls.Any(c => c.IsPressed())))
            {
                break;
            }

            yield return null;
        }

        loadingUI.Hide();
        Actions.OnDeactivateHowToPlay?.Invoke();

        sceneTransition.FadeIn();

        Actions.OnStateChange("Gameplay");
        dayManager.ShowStartDayPanel();
    }
}

[Serializable]
public class LoadingScreen
{
    [SerializeField] private Image howToPlayImage;
    [SerializeField] private TextMeshProUGUI howToPlayTextTitle;
    [SerializeField] private Button[] nextPreviousButtons;
}