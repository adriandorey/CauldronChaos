using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using System;
using UnityEngine.EventSystems;

public class LevelManager : MonoBehaviour
{
    [Header("Loading Screen")]
    [SerializeField] private Canvas loadingScreen;

    [SerializeField] private Slider loadingBar;
    [SerializeField] private float fakeProgressSpeed;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Scene Fade")]
    private Animator _fadeAnimator;

    [Header("Event System")]
    [SerializeField] private EventSystem eventSystem;
    
    private DayManager _dayManager;

    // Callback function to be invoked after fade animation completes
    private Action _fadeCallback;

    public void Start()
    {
        _dayManager = FindObjectOfType<DayManager>();
        _fadeAnimator = GetComponent<Animator>();
        _fadeAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    public void LoadScene(string sceneName)
    {
        // Prevents reloading the main menu if you're already in the main menu
        if (SceneManager.GetActiveScene().name == "MainMenu" && sceneName == "MainMenu")
        {
            Actions.OnStateChange("MainMenu");
            return;
        }

        // // Disable player interactions during transition
        // InputManager.Instance.TurnOffInteraction();


        // start fade out animation before loading the new scene
        Fade("FadeOut", () =>
        {
            // subscribe to the scene loaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Notify listeners that the game is loading
            Actions.OnStateChange("Loading");

        
            loadingScreen.enabled = true;
            loadingText.text = "Loading...";

            if (sceneName.StartsWith("Day"))
            {
                Actions.OnActivateHowToPlay?.Invoke(true);
                fakeProgressSpeed = 0.2f;
            }
            else
            {
                fakeProgressSpeed = 0.6f;
            }


            StartCoroutine(LoadAsync(sceneName));
        });
    }

    private IEnumerator LoadAsync(string sceneName)
    {
        // Load the scene asynchronously
        var loadOperation = SceneManager.LoadSceneAsync(sceneName);

        // Prevent the scene from activating immediately, even if it's fully loaded
        loadOperation.allowSceneActivation = false;

        var fakeProgress = 0f;

        while (fakeProgress < 1f)
        {
            // Gradually increase the fake progress
            fakeProgress += Time.unscaledDeltaTime * fakeProgressSpeed;

            // Ensure fake progress doesn't exceed the actual loading progress
            var actualProgress = Mathf.Clamp01(loadOperation.progress / 0.9f);
            fakeProgress = Mathf.Min(fakeProgress, actualProgress);

            // Update the loading bar
            loadingBar.value = fakeProgress;

            // Wait for the next frame
            yield return null;
        }

        loadOperation.allowSceneActivation = true;
    }

    // Used for loading screen between level selection or main menu and gameplay
    private IEnumerator WaitForAnyKeyPress()
    {
        // Cache the keyboard and gamepad references
        var keyboard = Keyboard.current;
        var gamepad = Gamepad.current;

        while (true)
        {
            // Check for any key press on the keyboard
            if (keyboard?.anyKey.wasPressedThisFrame == true)
            {
                yield break; // Exit the loop and the coroutine
            }

            // Check for any button press on the gamepad
            if (gamepad != null)
            {
                if (gamepad.buttonSouth.wasPressedThisFrame ||
                    gamepad.buttonNorth.wasPressedThisFrame ||
                    gamepad.buttonEast.wasPressedThisFrame ||
                    gamepad.buttonWest.wasPressedThisFrame ||
                    gamepad.leftStick.ReadValue().magnitude > 0.1f ||
                    gamepad.rightStick.ReadValue().magnitude > 0.1f ||
                    gamepad.dpad.ReadValue().magnitude > 0.1f)
                {
                    yield break; // Exit the loop and the coroutine
                }
            }

            yield return null;
        }
    }

    // Called when the scene is loaded and ready to be activated
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        StartCoroutine(WaitForPressThenFade(SceneManager.GetActiveScene()));
    }
    
    private IEnumerator WaitForPressThenFade(Scene currentScene)
    {
        loadingText.text = "Press Any Key To Continue";
    
        yield return WaitForAnyKeyPress(); // Wait until the player presses a key

        loadingScreen.enabled = false; // Only disable after input
        Fade("FadeIn"); // Now fade into the scene
       
        if (currentScene.name.StartsWith("Day"))
        {
            Actions.OnDeactivateHowToPlay?.Invoke();
            Actions.OnStateChange("Gameplay");
            _dayManager.ShowStartDayPanel();
            // InputManager.Instance.TurnOnInteraction();
        }

        if (currentScene.name == "MainMenu" || currentScene.name == "LevelSelect")
        {
            Actions.OnStateChange(currentScene.name == "MainMenu" ? "MainMenu" : "LevelSelect");
        }
    }


    // Fade the screen to black or clear
    private void Fade(string fadeDir, Action callback = null)
    {
        _fadeCallback = callback;
        _fadeAnimator.SetTrigger(fadeDir);
    }

    // Is called at the end of the fade out animation
    public void FadeAnimationComplete()
    {
        _fadeCallback?.Invoke();
        _fadeCallback = null;
    }
}

[Serializable]
public class LoadingScreen
{
    [SerializeField] private Image howToPlayImage;
    [SerializeField] private TextMeshProUGUI howToPlayTextTitle;
    [SerializeField] private Button[] nextPreviousButtons;
}