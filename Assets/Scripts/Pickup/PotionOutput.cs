using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class PotionOutput : MonoBehaviour
{
    [SerializeField] private Renderer rend;
    private MaterialPropertyBlock _propBlock;
    private Color _storedColor;
    private Rigidbody _rb;
    private Collider _col;

    public RecipeSO potionInside;
    internal bool GivenToCustomer;

    private bool _wasPaused;
    private bool _addedWobble;

    private Vector3 _lastPos;
    private Vector3 _velocity;
    private Vector3 _lastRot;
    private Vector3 _angularVelocity;

    public float maxWobble = 0.03f;
    public float wobbleSpeed = 1f;
    public float recovery = 1f;

    private float _wobbleAmountX;
    private float _wobbleAmountZ;
    private float _wobbleAmountToAddX;
    private float _wobbleAmountToAddZ;
    private float _pulse;
    private float _time = 0.5f;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
    }

    public void Update()
    {
        if(_wasPaused && Time.timeScale > 0)
        {
            _wasPaused = false;
            SetPotionColor();
        }
        else if (Time.timeScale == 0)
        {
            _wasPaused = true;
            _addedWobble = false;
        }

        if (!_wasPaused && _addedWobble)
            Wobble();
    }

    internal void GoToCustomer(CustomerBehaviour customer)
    {
        GivenToCustomer = true;
        _rb.isKinematic = true;
        _col.enabled = false;
        
        transform.SetParent(customer.customerHands);
        transform.DOJump(customer.customerHands.position, 1,1, 0.3f);
    }

    internal void NoCustomerAvailable()
    {
        GivenToCustomer = false;
        _rb.isKinematic = false;
        _col.enabled = true;
        
        var startPos = transform.position;
        var randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(0.5f, 2f));
        var endPos = startPos + randomDirection * 3f;
        transform.DOJump(endPos, 2, 1, 1);
    }
    
    public void SetPotionColor()
    {
        _storedColor = potionInside.potionColor;
        ApplyColor();
    }

    private void ApplyColor()
    {
        _propBlock = new MaterialPropertyBlock();
        rend.GetPropertyBlock(_propBlock);
        _propBlock.SetFloat("_Fill", 0.6f);
        _propBlock.SetColor("_Color", _storedColor);
        rend.SetPropertyBlock(_propBlock);

        _addedWobble = true;
    }

    private void Wobble()
    {
        _time += Time.deltaTime;

        // Decrease wobble over time
        _wobbleAmountToAddX = Mathf.Lerp(_wobbleAmountToAddX, 0, Time.deltaTime * recovery);
        _wobbleAmountToAddZ = Mathf.Lerp(_wobbleAmountToAddZ, 0, Time.deltaTime * recovery);

        // Create a sine wave wobble
        _pulse = 2 * Mathf.PI * wobbleSpeed;
        _wobbleAmountX = _wobbleAmountToAddX * Mathf.Sin(_pulse * _time);
        _wobbleAmountZ = _wobbleAmountToAddZ * Mathf.Sin(_pulse * _time);

        if (float.IsNaN(_wobbleAmountX) || float.IsNaN(_wobbleAmountZ))
        {
            _wobbleAmountX = 0;
            _wobbleAmountZ = 0;

            return;
        }

        // Apply rotation wobble instead of modifying the material
        transform.localRotation = Quaternion.Euler(
            _wobbleAmountX * 5f,   // Small tilt on X
            transform.localRotation.eulerAngles.y, // Keep original Y rotation
            _wobbleAmountZ * 5f    // Small tilt on Z
        );

        // Calculate velocity changes
        _velocity = (_lastPos - transform.position) / Time.deltaTime;
        _angularVelocity = transform.rotation.eulerAngles - _lastRot;

        // Add clamped velocity to wobble
        _wobbleAmountToAddX += Mathf.Clamp((_velocity.x + (_angularVelocity.z * 0.2f)) * maxWobble, -maxWobble, maxWobble);
        _wobbleAmountToAddZ += Mathf.Clamp((_velocity.z + (_angularVelocity.x * 0.2f)) * maxWobble, -maxWobble, maxWobble);

        // Keep last position and rotation
        _lastPos = transform.position;
        _lastRot = transform.rotation.eulerAngles;
    }
}