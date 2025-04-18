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
        if(pickupHolder != null)
            pickupHolder.enabled = false;
    }

    #region OnEnable / OnDisable / OnDestroy Events

    //Function that runs when Game Object script is attached to is enabled
    private void OnEnable()
    {
        InputManager.InteractAction += TryInteract; //subscribing to the action for interacting
        InputManager.PickupAction += Pickup; //subscribing to the action for picking up
        Actions.OnResetValues += RemoveItem;
    }

    //Function that runs when Game object script is attached to is disabled
    private void OnDisable()
    {
        InputManager.InteractAction -= TryInteract; //un-subscribing to the action for interacting
        InputManager.PickupAction -= Pickup; //un-subscribing to the action for picking up
        Actions.OnResetValues -= RemoveItem;
    }

    private void OnDestroy()
    {
        InputManager.InteractAction -= TryInteract;
        InputManager.PickupAction -= Pickup; //un-subscribing to the action for picking up
        Actions.OnResetValues -= RemoveItem;
    }

    #endregion

    private void TryInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (_heldObject != null)
        {
            // do a thing. Currently, nothing we have uses this but this would have been for broom
            return;
        }

        if (actionZoneCollider == null) return;

        var center = actionZoneCollider.bounds.center;
        var halfExtents = actionZoneCollider.bounds.extents;

        var colliders = Physics.OverlapBox(center, halfExtents, Quaternion.identity, interactionMask);

        if (colliders.Length == 0) return;

        colliders[0].GetComponent<IInteractable>()?.Interact();
    }


    private void Pickup(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (_heldObject != null)
        {
            RemoveItem();
            return;
        }

        GetObject();
    }

    private void GetObject()
    {
        if (actionZoneCollider == null) return;

        var center = actionZoneCollider.bounds.center;
        var halfExtents = actionZoneCollider.bounds.extents;

        var colliders = Physics.OverlapBox(center, halfExtents, Quaternion.identity, pickupMask);

        List<IPickupable> candidates = new();
        foreach (var col in colliders)
        {
            if (col.TryGetComponent(out IPickupable pickupable))
            {
                candidates.Add(pickupable);
            }
        }

        if (candidates.Count == 0) return;

        var chosen = candidates[Random.Range(0, candidates.Count)];
        playerAnimator.SetBool(PickedUp, true);
        chosen.OnPickup(this);
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

    private void RemoveItem()
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

    private void OnTriggerEnter(Collider coll)
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
