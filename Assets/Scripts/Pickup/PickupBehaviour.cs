using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PickupBehaviour : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PickupDetection pickupVolume; //the detector for picking up objects
    [SerializeField] private InteractionDetection interactionVolume; //the detector for interacting with objects
    [SerializeField] private Transform pickupHolder; //transform holding the held location of the pickup
    [SerializeField] private InteractionBehaviour interactionBehaviour; //component containing behaviour for object interactions
    private PickupObject _heldObject = null; //reference to object in hand
    [SerializeField] private Animator playerAnimator;

    [Header("UI")]
    [SerializeField] private Image pickupUIHolder;
    internal bool IsHoldingItem = false;
    
    private TutorialManager _tutorialManager;

    private void Awake()
    {
        if (pickupUIHolder != null)
            pickupUIHolder.enabled = false;
    }

    #region OnEnable / OnDisable / OnDestroy Events
    //Function that runs when Gameobject script is attached to is enabled
    private void OnEnable()
    {
        InputManager.PickupAction += Pickup; //subscribing to the action for picking up
        Actions.OnResetValues += RemoveItem;
    }

    //Function that runs when Gameobject script is attached to is disabled
    private void OnDisable()
    {
        InputManager.PickupAction -= Pickup; //un-subscribing to the action for picking up
        Actions.OnResetValues -= RemoveItem;
    }

    private void OnDestroy()
    {
        InputManager.PickupAction -= Pickup;
        Actions.OnResetValues -= RemoveItem;
    }

    #endregion


    //function that handles picking up an object
    private void Pickup(InputAction.CallbackContext input)
    {
        if (!input.performed) return;
        //Debug.Log("Input Activated");

        //player is holding something
        if (_heldObject != null)
        {
            DropItem();
            return;
        }

        //player is not holding anything and is by an ingredient crate
        var container = interactionVolume.GetContainer();
        if (container != null)
        {
            container.Interact(this);
            if (GameManager.Instance.IsInTutorial())
            {
                CheckTutorialSteps(container);
            }
        }
        //pick-up off the ground
        else
        {
            ////try to get held item from pickup detector
            _heldObject = pickupVolume.GetPickup();

            //if item is in detection range
            if (_heldObject == null) return;
            
            _heldObject.PickUp(pickupHolder);
            pickupUIHolder.enabled = true;
            SetHeldObject(_heldObject);

            if (_heldObject.TryGetComponent(out PotionOutput potionOutput) && potionOutput.potionInside != null)
            {
                pickupUIHolder.sprite = potionOutput.potionInside.potionIcon;
            }
            else if (_heldObject.TryGetComponent(out PickupObject ingredientHolder))
            {
                pickupUIHolder.sprite = ingredientHolder.recipeIngredient.stepSprite;
            }

            //try to get interactable component of the held object
            if (_heldObject.TryGetComponent<Interactable>(out var interactable))
            {
                //add interactable as being held
                interactionBehaviour.UpdateHeldInteractable(interactable);
            }
        }
    }

    // Check Tutorial Steps
    private void CheckTutorialSteps(Interactable container)
    {
        if (_tutorialManager == null)
            _tutorialManager = FindObjectOfType<TutorialManager>();

        if (container.GetComponent<CrateHolder>().crateType == CrateHolder.CrateType.Mushroom)
        {
            if (_tutorialManager.CurrentStep < TutorialStep.PickUpMushroom) return;
        }

        if (container.GetComponent<CrateHolder>().crateType == CrateHolder.CrateType.Bottle)
        {
            if (_tutorialManager.CurrentStep < TutorialStep.PickUpPotionBottle) return;
        }
    }

    //Mutator method that manually sets the held object
    public void SetHeldObject(PickupObject targetObject)
    {
        _heldObject = targetObject;
        playerAnimator.SetTrigger("Pickup");
        //Debug.Log("Player Pick up3");
        pickupVolume.RemovePickupFromList(_heldObject);
        _heldObject.PickUp(pickupHolder);
        IsHoldingItem = true;
    }

    private void DropItem()
    {
        playerAnimator.SetTrigger("Drop");
        //Debug.Log("Player Drop");
        pickupUIHolder.enabled = false;
        _heldObject.Drop();
        IsHoldingItem = false;

        // check if held item is an interactable
        if (interactionBehaviour.GetHeldInteractable() != null)
        {
            //if held interactable is detected set to null
            interactionBehaviour.UpdateHeldInteractable(null);
        }

        _heldObject = null;
    }

    private void RemoveItem()
    { 
        playerAnimator.SetTrigger("Drop");
        // Debug.Log("Player Drop");

        if (pickupHolder.childCount <= 0) return;
        
        foreach (Transform child in pickupHolder)
        {
            Destroy(child.gameObject);
        }
    }

    public Transform GetHolderLocation()
    {
        return pickupHolder;
    }
}
