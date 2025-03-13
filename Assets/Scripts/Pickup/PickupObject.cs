using UnityEngine;
using static WindyDay;

public class PickupObject : MonoBehaviour
{
    [Header("Ingredient")]
    public RecipeStepSO recipeIngredient;
    public Color ingredientColor;

    [Header("Pick Up")]
    public bool isHeld = false; //bool tracking if the pickup is held
    private Transform _targetPos; //transform tracking the target position of the pickup
    private Rigidbody rb; //rigidbody component of the pickup
    private bool _addedToCauldron = false;
    private Collider _objCollider;

    [Header("SFX")]
    [SerializeField] private AudioClip pickUpSFX;
    [SerializeField] private AudioClip dropSFX;

    // Windy Day Variables
    private bool _inWindZone = false;
    private WindyDay _windArea;

    // Start is called before the first frame update
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _objCollider = GetComponent<Collider>();
    }

    // Update is called once per frame
    //private void Update()
    //{
    //    //moves pickup towards the target position if the positions do not match
    //    if (isHeld && rb.position != targetPos.position)
    //    {
    //        rb.MovePosition(targetPos.position);
    //    }
    //}

    private void FixedUpdate()
    {
        // If the item is in the wind zone, not being added to the cauldron and isn't being held
        if (_inWindZone && !_addedToCauldron && !isHeld)
        {
            // Add wind resistance (force) to the item
            rb.AddForce(_windArea.AddWindResistance());
        }
    }

    //Function that picks up the pickup
    public void PickUp(Transform newTargetPos)
    {
        //setting held to true and removing gravity
        isHeld = true;
        rb.isKinematic = true;
        _objCollider.enabled = false;
        //rb.useGravity = false;

        //setting target position & parenting
        _targetPos = newTargetPos;
        transform.position = newTargetPos.position;
        transform.parent = newTargetPos;

        //playing SFX
        AudioManager.instance.sfxManager.PlaySFX(SFX_Type.ItemInteraction, pickUpSFX, true);
    }

    //Function that drops the pickup
    public void Drop()
    {
        //setting held to false and enabling gravity
        isHeld = false;
        rb.isKinematic = false;
        _objCollider.enabled = true;

        //rb.useGravity = true;

        //removing target position & parent
        _targetPos = null;
        transform.parent = null;

        //playing SFX
        AudioManager.instance.sfxManager.PlaySFX(SFX_Type.ItemInteraction, dropSFX, true);
    }

    public bool AddedToCauldron()
    {
        return _addedToCauldron;
    }

    //Mutator method that marks the ingredient as added to the cauldron
    public void AddToCauldron()
    {
        _addedToCauldron = true;
    }

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

}
