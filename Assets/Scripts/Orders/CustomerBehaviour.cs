using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CustomerBehaviour : MonoBehaviour
{
    private static readonly int IsWalking = Animator.StringToHash("isWalking");

    [Header("Movement Settings")]
    public float moveSpeed = 3f;

    [SerializeField] private Animator animator;
    private Vector3 _targetPosition;
    private bool _leavingQueue;
    internal bool HasJoinedQueue;
    internal bool HasReceivedPotion;

    [Header("SFX")]
    [SerializeField] private AudioClip sfxOrderIn;
    [SerializeField] private SFXLibrary sfxCustomerWalk;
    [SerializeField] private float customerStepTime = 0.3f;
    private float stepTimer = 0f;
    private bool isWalking = false;

    private Coroutine _customerCoroutine;
    

    private void Start()
    {
        moveSpeed = 3f;
        animator.speed = 1f;
    }

    private void Update()
    {
        if (!isWalking) return; //clause statement to stop logic below if not walking

        if (stepTimer <= 0f)
        {
            AudioManager.instance.sfxManager.PlaySFX(SFX_Type.ShopSounds, sfxCustomerWalk.PickAudioClip(), true);
            stepTimer = customerStepTime / animator.speed;
        }
        else
        {
            stepTimer -= Time.deltaTime;
        }
    }

    #region Order Events

    // internal void AssignOrder(RecipeSO order, Transform parent)
    // {
    //     this.RequestedOrder = order;
    //     _orderUiParent = parent;
    // }

    private void DisplayOrderUI()
    {
        // if(_hasShownOrder)  return;
        //
        // _orderUiInstance = Instantiate(orderUiPrefab, _orderUiParent);
        // // Find the Image component in the instantiated object, not globally
        //
        // var child = _orderUiInstance.transform.GetChild(0);
        // _orderIcon = child.GetComponent<Image>();
        // _orderIcon.sprite = RequestedOrder.potionIcon;
        //
        // AudioManager.instance.sfxManager.PlaySFX(SFX_Type.ShopSounds, sfxOrderIn, true);
        // _hasShownOrder = true;
    }

    // internal void OrderComplete()
    // {
    //     Actions.OnCustomerServed?.Invoke(RequestedOrder.sellAmount);
    //     coin.Play();
    // }

    #endregion

    #region Positioning

    private IEnumerator MoveToPosition(Vector3 position)
    {
        animator.SetBool(IsWalking, true);
        isWalking = true;

        while (Vector3.Distance(transform.position, position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, position, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(position - transform.position);
            yield return null; // Wait until next frame
        }

        transform.rotation = Quaternion.identity;
        HasJoinedQueue = true;
        animator.SetBool(IsWalking, false);
        isWalking = false;

       
    }
    
    
    internal void SetTarget(Vector3 position)
    {
        if (transform.position == position) return;
     
        _customerCoroutine = StartCoroutine(MoveToPosition(position));     
    }

    // Leave the queue and call a callback once finished
    internal void LeaveQueue(Vector3 exitPosition, System.Action onExitComplete)
    {
        _leavingQueue = true;
        
        StartCoroutine(LeaveAndExit(exitPosition, onExitComplete));
    }

    private IEnumerator LeaveAndExit(Vector3 exitPosition, System.Action onExitComplete)
    {
        animator.SetBool(IsWalking, true);
        isWalking = true;

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
        while (Vector3.Distance(transform.position, exitPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, exitPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Notify that the customer has left
        onExitComplete?.Invoke();
    }



    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5f);
    // }

    internal void ScareAway()
    {
        HasJoinedQueue = false;
        _leavingQueue = true;
        animator.speed = 2f;
        moveSpeed = 12f;
    }

    #endregion
}