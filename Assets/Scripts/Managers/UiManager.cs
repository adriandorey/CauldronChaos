using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public Page currentLocation;

    [Header("Settings")]
    [SerializeField] private SettingsManager settingsManager;

    [Header("Ui Canvas")]
    [SerializeField] private GameObject mainMenuPanelUI;

    [SerializeField] private GameObject gameplayPanelUI;
    [SerializeField] private GameObject introPanelUI;
    [SerializeField] private GameObject levelSelectPanelUI;
    [SerializeField] private GameObject endOfDayPanelUI;
    [SerializeField] private GameObject settingsPanelUI;
    [SerializeField] private GameObject pausePanelUI;
    [SerializeField] private GameObject howToPlayPanelUI;
    [SerializeField] private GameObject creditsPanelUI;
    [SerializeField] private GameObject loadingPanelUI;

    [Header("How To Play")]
    [SerializeField] private Image howToPlayBg;
    [SerializeField] private Button howToPlayBack;

    [Header("End Of Day Animation")]
    [SerializeField] private Animator endOfDayAnim;
    [SerializeField] private Animator playerAnimator;

    private Dictionary<GameState, (GameObject panel, Action action)> _uiElements;

    // Callback function to be invoked after fade animation completes
    private Action _fadeCallback;

    private void Awake()
    {
        // initialize panel dictionary
        _uiElements = new Dictionary<GameState, (GameObject, Action)>
        {
            { GameState.MainMenu, (mainMenuPanelUI, MainMenu) },
            { GameState.Intro, (introPanelUI, Intro) },
            { GameState.LevelSelect, (levelSelectPanelUI, LevelSelect) },
            { GameState.Gameplay, (gameplayPanelUI, Gameplay) },
            { GameState.EndOfDay, (endOfDayPanelUI, EndOfDay) },
            { GameState.Settings, (settingsPanelUI, Settings) },
            { GameState.Pause, (pausePanelUI, Pause) },
            { GameState.Credits , (creditsPanelUI, Credits)},
            { GameState.Loading , (loadingPanelUI, Loading)}
        };
    }


    #region OnEnable / OnDisable / OnDestroy Events

    private void OnEnable()
    {
        Actions.OnChangeUi += UpdateUIForGameState;
        Actions.ReachedWaypoint += CameraReached;
        Actions.OnActivateHowToPlay += ActivateHowToPlay;
        Actions.OnDeactivateHowToPlay += DeactivateHowToPlay;
    }

    private void OnDisable()
    {
        Actions.OnActivateHowToPlay -= ActivateHowToPlay;
        Actions.OnDeactivateHowToPlay -= DeactivateHowToPlay;
        Actions.OnChangeUi -= UpdateUIForGameState;
        Actions.ReachedWaypoint -= CameraReached;
    }

    private void OnDestroy()
    {
        Actions.OnActivateHowToPlay -= ActivateHowToPlay;
        Actions.OnDeactivateHowToPlay -= DeactivateHowToPlay;
        Actions.OnChangeUi -= UpdateUIForGameState;
        Actions.ReachedWaypoint -= CameraReached;
    }

    #endregion

    private void UpdateUIForGameState(GameState state)
    {
        // Debug.Log("Setting gamestate for " + state);

        foreach (var uiElement in _uiElements.Values)
        {
            uiElement.panel.SetActive(false);
        }

        if (!_uiElements.TryGetValue(state, out var element)) return;
        element.panel.SetActive(true);
        element.action?.Invoke();
    }

    // State UI Changes.

    #region State_UI_Changes

    private void MainMenu()
    {
        // InputManager.Instance.TurnOnInteraction();
        Actions.OnResetValues?.Invoke();
        MenuVirtualCamera.TurnCameraBrainOn?.Invoke();
        Actions.OnSetUiLocation(Page.MainMenu);
        Time.timeScale = 1;
    }

    private void CameraReached(string waypoint)
    {
        switch (waypoint)
        {
            case "Door":
                Actions.OnStateChange("Intro");
                return;
            case "Calendar": Actions.OnStateChange("LevelSelect"); break;
        }
    }


    private void LevelSelect()
    {
        playerAnimator.SetBool("EOD", false);
        endOfDayAnim.SetBool("EOD", false);
        Actions.UpdateLevelButtons();
        Time.timeScale = 1;

        Actions.OnResetValues?.Invoke();
        Actions.OnSetUiLocation(Page.LevelSelect);
        MenuVirtualCamera.OnResetCamera?.Invoke();
    }

    private void Intro()
    {
        Actions.OnSetUiLocation(Page.Intro);
    }

    private void Credits()
    {
        Actions.OnSetUiLocation(Page.Credits);
    }

    private void Loading()
    {
        MenuVirtualCamera.OnResetCamera?.Invoke();
        
    }


    private void Gameplay()
    {
        Actions.OnSetUiLocation(Page.Gameplay);
        Time.timeScale = 1;
    }

    private void EndOfDay()
    {
        Actions.OnSetUiLocation(Page.EndOfDay);
        playerAnimator.SetBool("EOD", true);
        endOfDayAnim.SetBool("EOD", true);
    }

    private void Pause()
    {
        Actions.OnSetUiLocation(Page.Pause);
        Time.timeScale = 0;
    }

    private void Settings()
    {
        settingsManager.OpenSettings();
    }

    #endregion

    // How to play
    public void ActivateHowToPlay(bool isLoading)
    {
        howToPlayPanelUI.SetActive(true);
        
        if (isLoading)
        {
            howToPlayBg.enabled = false;
            howToPlayBack.gameObject.SetActive(false);
        }
        else
        {
            howToPlayBg.enabled = true;
            howToPlayBack.gameObject.SetActive(true);
        }
    }

    public void DeactivateHowToPlay()
    {
        howToPlayPanelUI.SetActive(false);
    }
}

[Serializable]
public enum Page
{
    MainMenu,
    Intro,
    LevelSelect,
    Pause,
    Gameplay,
    Settings,
    EndOfDay,
    Audio,
    Video,
    System,
    ControlsKeyboard,
    ControlsGamepad,
    DeleteFile,
    HowToPlay,
    DebugInput,
    DebugToggle,
    RecipeBook,
    Credits,
}