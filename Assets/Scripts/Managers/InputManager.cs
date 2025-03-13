using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Image abovePlayerInteraction;
    [SerializeField] private TextMeshProUGUI abovePlayerInteractionText;

    //static variable holding an Instance of the InputManager;
    private static InputManager _instance;

    //input actions
    #region Input Actions Callbacks
    public static Action<InputAction.CallbackContext> MoveAction;
    public static Action<InputAction.CallbackContext> InteractAction;
    public static Action<InputAction.CallbackContext> PickupAction;
    public static Action<InputAction.CallbackContext> StirClockwiseAction;
    public static Action<InputAction.CallbackContext> StirCounterClockwiseAction;
    public static Action<InputAction.CallbackContext> PauseAction;
    public static Action<InputAction.CallbackContext> NextPageAction;
    public static Action<InputAction.CallbackContext> PreviousPageAction;
    #endregion

    // actions for above player interaction
    public static Action OnInteract;
    public static Action OnPickup;
    public static Action OnStir;
    public static Action OnHide;

    public static Action OnGameplayInputs;
    public static Action OnRecipeBookInputs;

    #region Input Actions
    private PlayerInput _playerControls;
    private InputAction _interactInputAction;
    private InputAction _pickupInputAction;
    private InputAction _stirCAction;
    private InputAction _stirCcAction;
    private InputAction _moveInputAction;
    private InputAction _pauseInputAction;
    private InputAction _nextPageInputAction;
    private InputAction _previousPageInputAction;
    #endregion


    //function that checks if Instance exists and spawns one if it does not
    // this is spawning a new input manager when quitting play
    public static InputManager Instance
    {
        get
        {
            //check if Instance is null
            if (_instance == null)
            {
                //spawn Instance
                _instance = Instantiate(Resources.Load("InputManager") as GameObject).GetComponent<InputManager>();
                _instance.name = "InputManager"; //renames the game object to InputManager
            }
            return _instance; //returns 
        }
    }

    // Awake is called before the first frame update and before start
    void Awake()
    {
        //check if this is the active Instance
        if (!_instance || _instance == this)
        {
            _instance = this;
        }
        else
        {
            //remove copy
            Destroy(gameObject);
        }

        abovePlayerInteractionText.text = "";
    }

    private void Start()
    {
        _playerControls = GetComponent<PlayerInput>();

        _moveInputAction = _playerControls.actions.FindAction("Move");
        _pauseInputAction = _playerControls.actions.FindAction("Pause");
        _nextPageInputAction = _playerControls.actions.FindAction("Next Page");
        _previousPageInputAction = _playerControls.actions.FindAction("Previous Page");
        _interactInputAction = _playerControls.actions.FindAction("Interact");
        _pickupInputAction = _playerControls.actions.FindAction("Pickup");
        _stirCAction = _playerControls.actions.FindAction("StirClockwise");
        _stirCcAction = _playerControls.actions.FindAction("StirCounterClockwise");
        

        _previousPageInputAction.Disable();
        _nextPageInputAction.Disable();
        HideInteractionPickup();
    }

    #region OnEnable / OnDisable / OnDestroy Events
    private void OnEnable()
    {
        // Actions.OnEndDay += TurnOffInteraction;
        // Actions.OnStartDay += TurnOnInteraction;
        // Actions.OnTutorialDay += TurnOnInteraction;
        OnInteract += ShowInteraction;
        OnPickup += ShowPickup;
        OnStir += ShowStir;
        OnHide+= HideInteractionPickup;
        OnRecipeBookInputs += RecipeBookInputs;
        OnGameplayInputs += GameplayInputs;
    }

    private void OnDisable()
    {
        // Actions.OnEndDay -= TurnOffInteraction;
        // Actions.OnStartDay -= TurnOnInteraction;
        // Actions.OnTutorialDay -= TurnOffInteraction;
        OnInteract -= ShowInteraction;
        OnPickup -= ShowPickup;
        OnStir -= ShowStir;
        OnHide-= HideInteractionPickup;
        OnRecipeBookInputs -= RecipeBookInputs;
        OnGameplayInputs -= GameplayInputs;
    }

    private void OnDestroy()
    {
        // Actions.OnEndDay -= TurnOffInteraction;
        // Actions.OnStartDay -= TurnOnInteraction;
        // Actions.OnTutorialDay -= TurnOffInteraction;
        OnInteract -= ShowInteraction;
        OnPickup -= ShowPickup;
        OnStir -= ShowStir;
        OnHide -= HideInteractionPickup;
        OnRecipeBookInputs -= RecipeBookInputs;
        OnGameplayInputs -= GameplayInputs;
    }
    #endregion

    #region Player Controls
    //function that reads the move input
    public void MoveInput(InputAction.CallbackContext input)
    {
        //if (Time.timeScale != 0)
        MoveAction?.Invoke(input);
    }

    //function that reads the interact input
    public void InteractInput(InputAction.CallbackContext input)
    {
        // if (Time.timeScale != 0)
            InteractAction?.Invoke(input);
    }

    //function that reads the interact input
    public void PickupInput(InputAction.CallbackContext input)
    {
        // if (Time.timeScale != 0)
            PickupAction?.Invoke(input);
    }


    public void PauseInput(InputAction.CallbackContext input)
    {
        PauseAction?.Invoke(input);
    }

    public void StirClockwiseInput(InputAction.CallbackContext input)
    {
        if (Time.timeScale != 0)
            StirClockwiseAction?.Invoke(input);
    }

    public void StirCounterClockwiseInput(InputAction.CallbackContext input)
    {
        if (Time.timeScale != 0)
            StirCounterClockwiseAction?.Invoke(input);
    }

    public void TurnNextPage(InputAction.CallbackContext input)
    {
        NextPageAction?.Invoke(input);
    }

    public void TurnPreviousPage(InputAction.CallbackContext input)
    {
        PreviousPageAction?.Invoke(input);
    }


    // internal void TurnOffInteraction()
    // {
    //     _playerControls.SwitchCurrentActionMap("UI");
    // }
    //
    // internal void TurnOnInteraction()
    // {
    //     _playerControls.SwitchCurrentActionMap("Player");
    // }
    #endregion

    #region Above Player Interaction
    private void ShowInteraction()
    {
        abovePlayerInteraction.enabled = true;

        if(IsControllerConnected())
            abovePlayerInteractionText.text = _interactInputAction.GetBindingDisplayString(1);
        else
            abovePlayerInteractionText.text = _interactInputAction.GetBindingDisplayString(0);
    }

    private void ShowPickup()
    {
        abovePlayerInteraction.enabled = true;

        if (IsControllerConnected())
            abovePlayerInteractionText.text = _pickupInputAction.GetBindingDisplayString(1);
        else
            abovePlayerInteractionText.text = _pickupInputAction.GetBindingDisplayString(0);
    }

    private void ShowStir()
    {
        abovePlayerInteraction.enabled = true;
        if (IsControllerConnected())
            abovePlayerInteractionText.text = _stirCcAction.GetBindingDisplayString(1) + " / " + _stirCAction.GetBindingDisplayString(1);
        else
            abovePlayerInteractionText.text = _stirCcAction.GetBindingDisplayString(0) + " / " + _stirCAction.GetBindingDisplayString(0);
    }

    private void HideInteractionPickup()
    {
        abovePlayerInteraction.enabled = false;
        abovePlayerInteractionText.text = "";
    }

    private bool IsControllerConnected()
    {
        return Gamepad.all.Count > 0;
    }

    #endregion

    private void GameplayInputs()
    {
        _moveInputAction.Enable();
        _pauseInputAction.Enable();
        _nextPageInputAction.Disable();
        _previousPageInputAction.Disable();
    }

    private void RecipeBookInputs()
    {
        _moveInputAction.Disable();
        _pauseInputAction.Disable();
        _nextPageInputAction.Enable();
        _previousPageInputAction.Enable();
    }
}
