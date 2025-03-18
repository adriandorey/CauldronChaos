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

    public static Action OnGameplayInputs;
    public static Action OnRecipeBookInputs;

    #region Input Actions
    private PlayerInput _playerControls;
    internal InputAction InteractInputAction;
    internal InputAction PickupInputAction;
    internal InputAction StirCAction;
    internal InputAction StirCcAction;
    internal InputAction MoveInputAction;
    internal InputAction PauseInputAction;
    internal InputAction NextPageInputAction;
    internal InputAction PreviousPageInputAction;
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

        MoveInputAction = _playerControls.actions.FindAction("Move");
        PauseInputAction = _playerControls.actions.FindAction("Pause");
        NextPageInputAction = _playerControls.actions.FindAction("Next Page");
        PreviousPageInputAction = _playerControls.actions.FindAction("Previous Page");
        InteractInputAction = _playerControls.actions.FindAction("Interact");
        PickupInputAction = _playerControls.actions.FindAction("Pickup");
        StirCAction = _playerControls.actions.FindAction("StirClockwise");
        StirCcAction = _playerControls.actions.FindAction("StirCounterClockwise");
        

        PreviousPageInputAction.Disable();
        NextPageInputAction.Disable();
        HideInteractionPickup();
    }

    #region OnEnable / OnDisable / OnDestroy Events
    private void OnEnable()
    {
        // Actions.OnEndDay += TurnOffInteraction;
        // Actions.OnStartDay += TurnOnInteraction;
        // Actions.OnTutorialDay += TurnOnInteraction;
        OnRecipeBookInputs += RecipeBookInputs;
        OnGameplayInputs += GameplayInputs;
    }

    private void OnDisable()
    {
        // Actions.OnEndDay -= TurnOffInteraction;
        // Actions.OnStartDay -= TurnOnInteraction;
        // Actions.OnTutorialDay -= TurnOffInteraction;
        OnRecipeBookInputs -= RecipeBookInputs;
        OnGameplayInputs -= GameplayInputs;
    }

    private void OnDestroy()
    {
        // Actions.OnEndDay -= TurnOffInteraction;
        // Actions.OnStartDay -= TurnOnInteraction;
        // Actions.OnTutorialDay -= TurnOffInteraction;
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
            abovePlayerInteractionText.text = InteractInputAction.GetBindingDisplayString(1);
        else
            abovePlayerInteractionText.text = InteractInputAction.GetBindingDisplayString(0);
    }

    private void ShowPickup()
    {
        abovePlayerInteraction.enabled = true;

        if (IsControllerConnected())
            abovePlayerInteractionText.text = PickupInputAction.GetBindingDisplayString(1);
        else
            abovePlayerInteractionText.text = PickupInputAction.GetBindingDisplayString(0);
    }

    private void ShowStir()
    {
        abovePlayerInteraction.enabled = true;
        if (IsControllerConnected())
            abovePlayerInteractionText.text = StirCcAction.GetBindingDisplayString(1) + " / " + StirCAction.GetBindingDisplayString(1);
        else
            abovePlayerInteractionText.text = StirCcAction.GetBindingDisplayString(0) + " / " + StirCAction.GetBindingDisplayString(0);
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
        MoveInputAction.Enable();
        PauseInputAction.Enable();
        NextPageInputAction.Disable();
        PreviousPageInputAction.Disable();
    }

    private void RecipeBookInputs()
    {
        MoveInputAction.Disable();
        PauseInputAction.Disable();
        NextPageInputAction.Enable();
        PreviousPageInputAction.Enable();
    }
}
