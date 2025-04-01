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
    private bool _isInTutorialMode;

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
            { GameState.MainMenu, () => ChangeMusic(Song.SongType.MainMenuMusic, 1) },
            { GameState.Loading , () => ChangeMusic(Song.SongType.MainMenuMusic, 1f)},
            { GameState.Gameplay, () => ChangeMusic(Song.SongType.GameplayMusic, 1f) },
            { GameState.Pause, () => ChangeMusic(Song.SongType.GameplayMusic, 0.5f) },
            { GameState.EndOfDay, () => ChangeMusic(Song.SongType.MainMenuMusic, 1f) },
            { GameState.LevelSelect, () => ChangeMusic(Song.SongType.MainMenuMusic, 1f) }
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
        InputManager.PauseAction += EscapeState;
    }

    private void OnDisable()
    {
        Actions.OnStateChange -= LoadState;
        InputManager.PauseAction -= EscapeState;
    }

    private void OnDestroy()
    {
        Actions.OnStateChange -= LoadState;
        InputManager.PauseAction -= EscapeState;
    }

    #endregion

    // This should be used for buttons parses states from strings
    public void LoadState(string state)
    {
        // if state is previous state, it should return back to the state it was.
        // this is used for the settings button to return to either pause or main menu
        if (state == "previousState")
            _newState = _previousState;
        else
        {
            // parses the gamestate if that gamestate exists
            if (Enum.TryParse(state, out GameState gamestate))
                _newState = gamestate;
            else
                Debug.LogError(state + " doesn't exist");
        }

        // sets the state to the new state
        SetState(_newState);
    }

    // Set state, can be used for anything except buttons.
    private void SetState(GameState state)
    {
        // this will save the previous state if the state is going into settings
        if (state == GameState.Settings)
            _previousState = gameState;

        // sets gamestate
        gameState = state;
       
        // Moving to Change Music as all game states go through change music
        // // null check for audio manager
        // if (AudioManager.instance != null)
        // {
        //     // if the state isn't gameplay, this will end environment sfx
        //     if (gameState != GameState.Gameplay)
        //     {
        //         AudioManager.instance.environmentManager.EndEnvironmentSFX();
        //     }
        //
        //     // if the gamestate isn't pause it will return the music pitch to 1
        //     if (gameState != GameState.Pause)
        //     {
        //         AudioManager.instance.musicManager.musicSource.pitch = 1f;
        //     }
        // }

        // takes the state and finds the action that's connected to that state and invokes it.
        if (_stateActions.TryGetValue(gameState, out var action))
            action.Invoke();

        // this invokes the UI manager to change the UI for the gamestate.
        Actions.OnChangeUi?.Invoke(gameState);
    }

    // this will change the music & pitch for game states.
    private void ChangeMusic(Song.SongType songType, float pitch)
    {
        // Null check for audio manager
        if(AudioManager.instance == null) return;
        
        // if the music isn't what's currently playing, switch the music
        if (AudioManager.instance.musicManager.GetCurrentMusic() != songType)
        {
            AudioManager.instance.musicManager.PlayMusic(songType);
        }
        
        // change the pitch to whatever was requested
        AudioManager.instance.musicManager.musicSource.pitch = pitch;
        
        // if gamestate isnt gameplay end environment sfx
        if (gameState != GameState.Gameplay)
        {
            AudioManager.instance.environmentManager.EndEnvironmentSFX();
        }
    }


    // this should be called when hitting the pause button. 
    private void EscapeState(InputAction.CallbackContext input)
    {
        // if the input wasn't performed, if the gamestate isn't gameplay or pause. The pause button will do nothing.
        if (!input.performed || (gameState != GameState.Gameplay && gameState != GameState.Pause)) return;
        
        // if it can, it will switch the state based on the current gamestate.
        switch (gameState)
        {
            case GameState.Pause: SetState(GameState.Gameplay); break;
            case GameState.Gameplay:  SetState(GameState.Pause); break;
        }
    }

    // Used to check if the game is in debug mode or not.
    internal bool IsDebugging()
    {
        return _isInDebugMode;
    }

    // Sets the game to debug or not.
    internal void SetDebugMode(bool debug)
    {
        _isInDebugMode = debug;
    }

    // Sets the game to tutorial mode or not.
    internal void SetTutorialMode(bool isTutorial)
    {
        _isInTutorialMode = isTutorial;
        
        if(isTutorial)
            Actions.OnStartTutorialDay?.Invoke();
    }

    internal bool IsInTutorial()
    {
        return _isInTutorialMode;
    }
    

    // Quits the game
    public void QuitGame()
    {
        Debug.Log("Quitting Game");
        Application.Quit();
    }
}

// Game states required for the game manager.
public enum GameState
{
    MainMenu,
    Credits,
    Loading,
    Intro,
    LevelSelect,
    Gameplay,
    EndOfDay,
    Pause,
    Settings
}