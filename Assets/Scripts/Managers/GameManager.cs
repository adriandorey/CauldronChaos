using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState gameState;
    private GameState _previousState;
    private GameState _newState;

    private bool _isInDebugMode;
    internal bool IsInTutorialMode;

    private Dictionary<GameState, Action> _stateActions;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
            Destroy(gameObject);

        _stateActions = new Dictionary<GameState, Action>
        {
            { GameState.MainMenu, () => ChangeMusic(Song.SongType.MainMenuMusic) },
            { GameState.Loading , () => ChangeMusic(Song.SongType.MainMenuMusic)},
            { GameState.Gameplay, () => ChangeMusic(Song.SongType.GameplayMusic) },
            { GameState.Pause, () => AudioManager.instance.musicManager.musicSource.pitch = 0.5f },
            { GameState.EndOfDay, () => ChangeMusic(Song.SongType.MainMenuMusic) },
            { GameState.LevelSelect, () => ChangeMusic(Song.SongType.MainMenuMusic) }
        };
    }

    private void Start()
    {
        SetState(GameState.MainMenu);
    }

    #region OnEnable / OnDisable / OnDestroy Events

    private void OnEnable()
    {
        Actions.OnStateChange += LoadState;
        Actions.OnTutorialDay += SetTutorialMode;
        InputManager.PauseAction += EscapeState;
    }

    private void OnDisable()
    {
        Actions.OnStateChange -= LoadState;
        Actions.OnTutorialDay -= SetTutorialMode;
        InputManager.PauseAction -= EscapeState;
    }

    private void OnDestroy()
    {
        Actions.OnStateChange -= LoadState;
        Actions.OnTutorialDay -= SetTutorialMode;
        InputManager.PauseAction -= EscapeState;
    }

    #endregion

    // This should be used for buttons
    public void LoadState(string state)
    {
        if (state == "previousState")
            _newState = _previousState;
        else
        {
            if (Enum.TryParse(state, out GameState gamestate))
                _newState = gamestate;
            else
                Debug.LogError(state + " doesn't exist");
        }

        SetState(_newState);
    }


    private void SetState(GameState state)
    {
        if (state == GameState.Settings)
            _previousState = gameState;

        gameState = state;

        if (_stateActions.TryGetValue(state, out Action action))
            action.Invoke();

        if (gameState == GameState.Loading) return;

        Actions.OnChangeUi?.Invoke(gameState);
    }

    private void ChangeMusic(Song.SongType songType)
    {
        if (AudioManager.instance.musicManager.GetCurrentMusic() != songType)
        {
            AudioManager.instance.musicManager.PlayMusic(songType);
        }
    }


    // this should be called when hitting the pause button. 
    private void EscapeState(InputAction.CallbackContext input)
    {
        if (!input.performed || (gameState != GameState.Gameplay && gameState != GameState.Pause)) return;
        switch (gameState)
        {
            case GameState.Pause: 
                SetState(GameState.Gameplay);
                AudioManager.instance.musicManager.musicSource.pitch = 1;
                break;
            case GameState.Gameplay: 
                SetState(GameState.Pause); 
                break;
        }
    }

    internal bool IsDebugging()
    {
        return _isInDebugMode;
    }

    internal void SetDebugMode(bool debug)
    {
        _isInDebugMode = debug;
    }

    private void SetTutorialMode()
    {
        IsInTutorialMode = !IsInTutorialMode;
    }
    

    public void QuitGame()
    {
        Debug.Log("Quitting Game");
        Application.Quit();
    }
}

public enum GameState
{
    MainMenu,
    Loading,
    Intro,
    LevelSelect,
    Gameplay,
    EndOfDay,
    Pause,
    Settings
}