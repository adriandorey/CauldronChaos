using System.Collections;
using UnityEngine;

public class CustomerMovement : MonoBehaviour
{
    private static readonly int IsWalking = Animator.StringToHash("isWalking");

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Animator animator;

    private Vector3 _targetPosition;
    private bool _joinedQueue;
    
    [SerializeField] private SFXLibrary sfxCustomerWalk;
    [SerializeField] private float customerStepTime = 0.3f;
    private float _stepTimer = 0f;
    private bool _isWalking = false;

    private Coroutine _customerCoroutine;
    private CustomerOrder _customerOrder;


    private void Awake()
    {
        _customerOrder = GetComponent<CustomerOrder>();
    }

    private void Update()
    {
        if (!_isWalking) return; //clause statement to stop logic below if not walking

        if (_stepTimer <= 0f)
        {
            AudioManager.instance.sfxManager.PlaySFX(SFX_Type.ShopSounds, sfxCustomerWalk.PickAudioClip(), true);
            _stepTimer = customerStepTime / animator.speed;
        }
        else
        {
            _stepTimer -= Time.deltaTime;
        }
    }

    internal void SetTarget(Vector3 target)
    {
        if (transform.position == target) return;
        _targetPosition = target;
        
        if(_customerCoroutine != null)
            StopCoroutine(_customerCoroutine);
        
        _customerCoroutine = StartCoroutine(MoveToTarget());
    }

    private IEnumerator MoveToTarget()
    {
        animator.SetBool(IsWalking, true);
        _isWalking = true;

        while (Vector3.Distance(transform.position, _targetPosition) > 0.1f)
        {
            // Set position and rotation of customer
            transform.SetPositionAndRotation(
                Vector3.MoveTowards(transform.position, _targetPosition, moveSpeed * Time.deltaTime), 
                Quaternion.LookRotation(_targetPosition - transform.position));
            yield return null;
        }
        
        transform.rotation = Quaternion.identity;
        _joinedQueue = true;
        animator.SetBool(IsWalking, false);
        _isWalking = false;
        
        _customerOrder.DisplayOrderUI();
    }
    
    internal void LeaveQueue(Vector3 exitPosition)
    {
        _targetPosition = exitPosition;
        _joinedQueue = false;
        _customerOrder.RemoveOrderUI();
        StartCoroutine(LeaveAndExit());
    }
    
    private IEnumerator LeaveAndExit()
    {
        animator.SetBool(IsWalking, true);
        _isWalking = true;

        const float stepDistance = 1.2f; // Distance to step back
        var backwardStep = transform.position - transform.forward * stepDistance; // Step back 1 unit

        transform.rotation = Quaternion.Euler(0, 180, 0);

        // Step 1: Move 1 unit backward
        while (Vector3.Distance(transform.position, backwardStep) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, backwardStep, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0, 90, 0); // Rotate -90 degrees

        // Step 3: Move to the exit
        while (Vector3.Distance(transform.position, _targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    internal void ScareAway(Vector3 exitPosition)
    {
        _targetPosition = exitPosition;
        animator.speed = 2f;
        moveSpeed = 12f;
        _joinedQueue = false;
        _customerOrder.RemoveOrderUI();
        StartCoroutine(LeaveAndExit());
    }
    
    internal bool HasJoinedQueue() => _joinedQueue;
}
