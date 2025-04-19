using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PickupInteractionController : MonoBehaviour
{
    private static readonly int PickedUp = Animator.StringToHash("PickedUp");

    [Header("Detection Settings")]
    [SerializeField] private LayerMask interactionMask;
    [SerializeField] private LayerMask pickupMask;
    [SerializeField] private Collider actionZoneCollider;

    [Header("Hold Settings")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Image pickupHolder;

    private IPickupable _heldObject;
    private bool isInteraction;
    private bool isPickup;

    private void Start()
    {
        if (pickupHolder != null)
            pickupHolder.enabled = false;
    }

    #region OnEnable / OnDisable / OnDestroy Events

    //Function that runs when Game Object script is attached to is enabled
    private void OnEnable()
    {
        InputManager.InteractAction += TryInteract; //subscribing to the action for interacting
        InputManager.PickupAction += TryPickUp; //subscribing to the action for picking up
        Actions.OnResetValues += DropItem;
    }

    //Function that runs when Game object script is attached to is disabled
    private void OnDisable()
    {
        InputManager.InteractAction -= TryInteract; //un-subscribing to the action for interacting
        InputManager.PickupAction -= TryPickUp; //un-subscribing to the action for picking up
        Actions.OnResetValues -= DropItem;
    }

    private void OnDestroy()
    {
        InputManager.InteractAction -= TryInteract;
        InputManager.PickupAction -= TryPickUp; //un-subscribing to the action for picking up
        Actions.OnResetValues -= DropItem;
    }

    #endregion

    /// <summary>
    /// Attempts to interact with the first interactable object in range.
    /// Uses the interactionMask and IInteractable interface.
    /// </summary>
    private void TryInteract(InputAction.CallbackContext context)
    {
        if (!context.performed || _heldObject != null) return;

        var interactables = GetComponentsInActionZone<IInteractable>(interactionMask);
        if (interactables.Count == 0) return;

        Debug.Log("Interact with the thing: " + interactables[0]);
        interactables[0].Interact();
    }


    /// <summary>
    /// Attempts to find a pickupable object in range and pick it up.
    /// Uses pickupMask and IPickupable interface.
    /// </summary>
    private void TryPickUp(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (_heldObject != null)
        {
            DropItem();
            return;
        }

        var pickupables = GetComponentsInActionZone<IPickupable>(pickupMask);
        if (pickupables.Count == 0) return;

        var chosen = pickupables[Random.Range(0, pickupables.Count)];
        playerAnimator.SetBool(PickedUp, true);
        chosen.OnPickup(this);
    }


    /// <summary>
    /// Generic method to get components of type T within the action zone using the specified LayerMask.
    /// This is used to find either IInteractable or IPickupable objects.
    /// </summary>
    /// <typeparam name="T">Interface type to search for (e.g., IInteractable, IPickupable)</typeparam>
    /// <param name="mask">LayerMask to filter relevant objects</param>
    /// <returns>List of components of type T within the zone</returns>
    private List<T> GetComponentsInActionZone<T>(LayerMask mask) where T : class
    {
        var center = actionZoneCollider.bounds.center;
        var halfExtents = actionZoneCollider.bounds.extents;

        var colliders = Physics.OverlapBox(center, halfExtents, Quaternion.identity, mask);

        List<T> components = new();
        foreach (var col in colliders)
        {
            if (col.TryGetComponent(out T component))
            {
                components.Add(component);
            }
        }

        return components;
    }



    internal void NotifyHeldObject(IPickupable newObject)
    {
        if (_heldObject != null) return;

        _heldObject = newObject;
        pickupHolder.enabled = true;
        pickupHolder.sprite = _heldObject.GetSprite();
    }

    internal Transform GetHoldPoint()
    {
        return holdPoint;
    }

    private void DropItem()
    {
        // needed due to resetting at end of levels 
        if (_heldObject == null) return;

        // drops item and resets animation
        _heldObject.OnDrop();
        _heldObject = null;
        pickupHolder.enabled = false;
        playerAnimator.SetBool(PickedUp, false);
    }

    private void CheckForSomething()
    {
        if (isInteraction)
        {
            Actions.OnShowInteraction?.Invoke();
        }

        if (isPickup)
        {
            Actions.OnShowPickup?.Invoke();
        }

        if (!isInteraction && !isPickup)
            Actions.OnHideUI();
    }

    private void OnTriggerStay(Collider coll)
    {
        if (coll.TryGetComponent(out IInteractable interactable))
        {
            isInteraction = true;
            CheckForSomething();
        }

        if (coll.TryGetComponent(out IPickupable pickupable))
        {
            isPickup = true;
            CheckForSomething();
        }
    }

    private void OnTriggerExit(Collider coll)
    {
        if (coll.TryGetComponent(out IInteractable interactable))
        {
            isInteraction = false;
            CheckForSomething();
        }

        if (coll.TryGetComponent(out IPickupable pickupable))
        {
            isPickup = false;
            CheckForSomething();
        }
    }
}
