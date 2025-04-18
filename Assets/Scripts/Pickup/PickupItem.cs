using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItem : MonoBehaviour, IPickupable
{
    [Header("General Info")]
    public ItemType type;
    internal bool IsHeld = false; // bool tracking if the pick up is being held
    private bool _addedToCauldron = false; // bool tracking if it was added to the cauldron
    private Rigidbody _rb;
    private Collider _objCollider;
    private Transform _targetPos; //transform tracking the target position of the pickup

    [Header("Ingredient Reference")]
    public RecipeStepSO ingredient;

    [Header("Potion Reference")]
    public RecipeSO potionRecipe;
    [SerializeField] private Renderer rend;
    private MaterialPropertyBlock _propBlock;
    private Color _storedColor;
    internal bool GivenToCustomer;


    [Header("SFX")]
    [SerializeField] private AudioClip pickUpSfx;
    [SerializeField] private AudioClip dropSfx;

    // windy day variables
    private bool _inWindZone = false;
    private WindyDay _windArea;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _objCollider = GetComponent<Collider>();
    }


    private void Update()
    {
        //moves pickup towards the target position if the positions do not match
        if (_targetPos != null && transform.position != _targetPos.position)
        {
            transform.position = _targetPos.position;
        }
    }

    void FixedUpdate()
    {
        // If the item is in the wind zone, not being added to the cauldron and isn't being held
        if (_inWindZone && !_addedToCauldron && !IsHeld)
        {
            // Add wind resistance (force) to the item
            _rb.AddForce(_windArea.AddWindResistance());
        }
    }

    #region Pickupable Logic

    public void OnPickup(PickupInteractionController controller)
    {
        //setting held to true and removing gravity
        IsHeld = true;
        _rb.isKinematic = true;
        _objCollider.enabled = false;

        controller.NotifyHeldObject(this);

        //setting target position & parenting
        _targetPos = controller.GetHoldPoint();
        transform.position = _targetPos.position;
        transform.parent = _targetPos;

        //playing SFX
        AudioManager.instance.sfxManager.PlaySFX(SFX_Type.ItemInteraction, pickUpSfx, true);
    }

    //Function that drops the pickup
    public void OnDrop()
    {
        //setting held to false and enabling gravity
        IsHeld = false;
        _rb.isKinematic = false;
        _objCollider.enabled = true;

        //removing target position & parent
        _targetPos = null;
        transform.SetParent(GameObject.Find("DroppedObjects").transform, true);

        //playing SFX
        AudioManager.instance.sfxManager.PlaySFX(SFX_Type.ItemInteraction, dropSfx, true);
    }

    public Sprite GetSprite()
    {
        switch (type)
        {
            case ItemType.Potion: return potionRecipe.potionIcon;
            case ItemType.Ingredient: return ingredient.stepSprite;
            default: return null;
        }
    }
    #endregion

    #region Potion Colour Logic
    public void SetPotionColor()
    {
        _storedColor = potionRecipe.potionColor;
        ApplyColor();
    }

    private void ApplyColor()
    {
        _propBlock = new MaterialPropertyBlock();
        rend.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat("_Fill", 0.6f);
        _propBlock.SetColor("_Color", _storedColor);
        rend.SetPropertyBlock(_propBlock);
    }
    #endregion

    #region Potion Customer Jumping or not
    internal void JumpToCustomer(CustomerOrder customer, System.Action onComplete)
    {
        GivenToCustomer = true;
        _rb.isKinematic = true;
        _objCollider.enabled = false;

        transform.SetParent(customer.customerHands);
        transform.DOJump(customer.customerHands.position, 1, 1, 0.3f)
            .OnComplete(() => onComplete?.Invoke());
    }

    internal void NoCustomerAvailable()
    {
        if (_rb == null)
            return;

        GivenToCustomer = false;
        _rb.isKinematic = false;
        _objCollider.enabled = true;

        var startPos = transform.position;
        var randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(0.5f, 2f));
        var endPos = startPos + randomDirection * 3f;
        transform.DOJump(endPos, 2, 1, 1);
    }
    #endregion


    public bool AddedToCauldron() => _addedToCauldron;

    //Mutator method that marks the ingredient as added to the cauldron
    public void InsertInCauldron()
    {
        _rb.isKinematic = true;
        _objCollider.enabled = false;
        _addedToCauldron = true;
    }

    #region Trigger Checks
    // Checks if it's in the wind zone and gets the wind component
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("WindArea")) return;

        _inWindZone = true;
        _windArea = other.GetComponent<WindyDay>();
    }

    // Checks if it's exit the wind zone and removes wind component
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("WindArea")) return;

        _inWindZone = false;
        _windArea = null;
    }
    #endregion
}

public enum ItemType
{
    None,
    Ingredient,
    Potion,
    Tool
}