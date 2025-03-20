using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerBehaviour : MonoBehaviour
{
    // Order details
    [Header("Order Details")]
    internal RecipeSO RequestedOrder;

    public string customerName;
    public Transform customerHands;

    [Header("UI for Order")]
    [SerializeField] private GameObject orderUiPrefab;
    [SerializeField] private ParticleSystem coin;
    private Image _orderIcon;
    private Transform _orderUiParent;
    private bool _hasShownOrder;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;

    [SerializeField] private Animator animator;
    private Vector3 _targetPosition;
    private bool _leavingQueue;
    internal bool HasJoinedQueue;

    private GameObject _orderUiInstance;
    

    private void Start()
    {
        moveSpeed = 3f;
        animator.speed = 1f;
    }

    private void Update()
    {
        if (!_leavingQueue)
            MoveToTarget();
    }

    #region Order Events

    internal void AssignOrder(RecipeSO order, Transform parent)
    {
        this.RequestedOrder = order;
        _orderUiParent = parent;
    }

    private void DisplayOrderUI()
    {
        _orderUiInstance = Instantiate(orderUiPrefab, _orderUiParent);
        // Find the Image component in the instantiated object, not globally

        var child = _orderUiInstance.transform.GetChild(0);
        _orderIcon = child.GetComponent<Image>();
        _orderIcon.sprite = RequestedOrder.potionIcon;
    }

    internal RecipeSO HasOrder()
    {
        return RequestedOrder;
    }

    internal void OrderComplete()
    {
        Actions.OnCustomerServed?.Invoke(RequestedOrder.sellAmount);
        coin.Play();
    }

    #endregion

    #region Positioning

    internal void SetTarget(Vector3 position)
    {
        animator.SetBool("isWalking", true);
        _targetPosition = position;
        _leavingQueue = false;
    }

    // Leave the queue and call a callback once finished
    internal void LeaveQueue(Vector3 exitPosition, System.Action onExitComplete)
    {
        _leavingQueue = true;
        Destroy(_orderUiInstance);
        StartCoroutine(LeaveAndExit(exitPosition, onExitComplete));
    }

    private void MoveToTarget()
    {
        if (Vector3.Distance(transform.position, _targetPosition) > 0.1f)
        {
            animator.SetBool("isWalking", true);
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 270, 0);
        }
        else
        {
            transform.rotation = Quaternion.identity;
            HasJoinedQueue = true;
            animator.SetBool("isWalking", false);

            if (!_hasShownOrder)
            {
                _hasShownOrder = true;
                DisplayOrderUI();
            }
        }
    }

    private IEnumerator LeaveAndExit(Vector3 exitPosition, System.Action onExitComplete)
    {
        animator.SetBool("isWalking", true);

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

    internal void ScareAway()
    {
        animator.speed = 2f;
        moveSpeed = 12f;
    }

    #endregion
}