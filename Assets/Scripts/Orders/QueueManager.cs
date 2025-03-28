using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class QueueManager : MonoBehaviour
{
    [Header("Customer Queue Variables")]
    [SerializeField] private int timeForCustomer = 1;

    [SerializeField] private GameObject tutorialCustomer;
    [SerializeField] private List<GameObject> customerPrefabs; // List of possible customer prefabs
    private List<GameObject> _customers = new();
    private readonly int _maxCustomers = 5;
    private CustomTimer _newCustomer;
    private bool _startCustomers;
    private int _previousIndex;

    [Header("Customer Queue Positions")]
    [SerializeField] private Transform firstPos;

    [SerializeField] private Transform entryPoint; // Spawn point for new customers
    [SerializeField] private Transform exitPoint; // Spawn point for new customers
    private Vector3[] _queuePositions = new Vector3[5]; //queue positions

    [Header("Order Manager")]
    [SerializeField] private OrderManager orderManager;

    [Header("Order Holder")]
    [SerializeField] private GameObject orderHolder;

    [Header("SFX")]
    [SerializeField] private AudioClip potionSaleSfx;

    private TutorialManager _tutorialManager;


    public void Start()
    {
        _newCustomer = new CustomTimer(timeForCustomer, true);

        _queuePositions[0] = firstPos.position;
        for (int i = 1; i < 5; i++)
        {
            _queuePositions[i] = firstPos.position + new Vector3(i, 0, 0);
        }

        _tutorialManager = FindObjectOfType<TutorialManager>();
    }

    private void Update()
    {
        // // For testing purposes
        // if (Input.GetKeyDown(KeyCode.Tab))
        // {
        //     StartCustomers();
        // }

        if (_startCustomers)
        {
            if (_newCustomer.UpdateTimer())
            {
                SpawnNewCustomer();
                _newCustomer.ResetTimer();
            }
        }
    }

    #region OnEnable / OnDisable / OnDestroy Events

    private void OnEnable()
    {
        Actions.OnEndDay += RemoveAllCustomers;
        Actions.OnStartDay += StartCustomerQueue;
        Actions.OnResetValues += RemoveAllCustomers;
    }

    private void OnDisable()
    {
        Actions.OnEndDay -= RemoveAllCustomers;
        Actions.OnStartDay -= StartCustomerQueue;
        Actions.OnResetValues -= RemoveAllCustomers;
    }

    private void OnDestroy()
    {
        Actions.OnEndDay -= RemoveAllCustomers;
        Actions.OnStartDay -= StartCustomerQueue;
        Actions.OnResetValues -= RemoveAllCustomers;
    }

    #endregion

    /// <summary> Starts the Customer Queue, timer and spawns customer </summary>
    private void StartCustomerQueue()
    {
        _startCustomers = true;
        _newCustomer = new CustomTimer(3, false);
        _newCustomer.StartTimer();
        SpawnNewCustomer();
    }

    internal void CheckCustomerRecipes(PotionOutput potionOutput)
    {
        var sO = potionOutput.potionInside;
        var potionObj = potionOutput.gameObject;

        foreach (var t in _customers)
        {
            var customer = t.GetComponent<CustomerBehaviour>();

            if (customer.RequestedOrder == sO && customer.HasJoinedQueue)
            {
                potionOutput.givenToCustomer = true;
                potionObj.GetComponent<Collider>().enabled = false;
                potionObj.GetComponent<Rigidbody>().isKinematic = true;

                var servingCustomer = t.GetComponent<CustomerBehaviour>();

                potionObj.transform.SetParent(servingCustomer.customerHands);
                potionObj.transform.DOJump(servingCustomer.customerHands.position, 1, 1, 0.3f)
                    .OnComplete(() => FinishOrder(servingCustomer));
                return;
            }
        }

        var startPos = potionObj.transform.position;
        var randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(0.5f, 2f));
        var endPos = startPos + randomDirection * 3f;
        potionObj.transform.DOJump(endPos, 2, 1, 1);
    }

    private void FinishOrder(CustomerBehaviour servingCustomer)
    {
        RemoveCustomer(servingCustomer.gameObject);
        servingCustomer.OrderComplete();

        //playing SFX for potion sale
        AudioManager.instance.sfxManager.PlaySFX(SFX_Type.ShopSounds, potionSaleSfx, true);

        if (!GameManager.Instance.IsInTutorialMode) return;
        
        Actions.OnPotionServed?.Invoke();
        _tutorialManager.ServedCustomer();
    }

    #region Customer Queue Methods

    // Add a new customer to the end of the queue
    private void AddCustomer(GameObject customer)
    {
        _customers.Add(customer);
        UpdateQueuePositions();
    }

    // Remove a customer and spawn a new one
    private void RemoveCustomer(GameObject customer)
    {
        if (_customers.Contains(customer) && customer.GetComponent<CustomerBehaviour>().HasJoinedQueue)
        {
            _customers.Remove(customer);
            customer.GetComponent<CustomerBehaviour>().LeaveQueue(exitPoint.position, () =>
            {
                Destroy(customer);

                if (!GameManager.Instance.IsInTutorialMode)
                    SpawnNewCustomer();
            });
            UpdateQueuePositions();
        }
    }

    // Spawn a new customer at the entry point and add them to the queue
    private void SpawnNewCustomer()
    {
        if (customerPrefabs.Count > 0 && _customers.Count < _maxCustomers)
        {
            int randomIndex = Random.Range(0, customerPrefabs.Count);

            while (randomIndex == _previousIndex)
            {
                randomIndex = Random.Range(0, customerPrefabs.Count);
            }

            _previousIndex = randomIndex;
            var newCustomer = Instantiate(customerPrefabs[randomIndex], entryPoint.position, Quaternion.identity);

            var newCustomBehaviour = newCustomer.GetComponent<CustomerBehaviour>();

            newCustomBehaviour.AssignOrder(orderManager.GiveOrder(newCustomBehaviour.customerName),
                orderHolder.transform);

            AddCustomer(newCustomer);
        }
    }

    // Update customer positions in the queue
    private void UpdateQueuePositions()
    {
        for (int i = 0; i < _customers.Count; i++)
        {
            _customers[i].GetComponent<CustomerBehaviour>().SetTarget(_queuePositions[i]);
        }
    }

    #endregion

    internal void ScareCustomer(GameObject customerToScare)
    {
        customerToScare.GetComponent<CustomerBehaviour>().ScareAway();
        RemoveCustomer(customerToScare);
    }

    internal GameObject FindCustomer()
    {
        List<GameObject> customersInQueue = new();
        GameObject customerToScare = null;

        foreach (var customer in _customers)
        {
            if (customer.GetComponent<CustomerBehaviour>().HasJoinedQueue)
            {
                customersInQueue.Add(customer);
            }
        }

        if (customersInQueue.Count <= 0) return null;

        var random = Random.Range(0, customersInQueue.Count);
        customerToScare = customersInQueue[random];

        return customerToScare;
    }

    private void RemoveAllCustomers()
    {
        _startCustomers = false;

        foreach (var customer in _customers)
        {
            Destroy(customer);
        }

        _customers.Clear();
    }

    
    internal void SpawnSpecificCustomer()
    {
        var customer = Instantiate(tutorialCustomer, entryPoint.position, Quaternion.identity);
        var customBehaviour = customer.GetComponent<CustomerBehaviour>();
        customBehaviour.AssignOrder(orderManager.TutorialOrder(), orderHolder.transform);
        AddCustomer(customer);
    }
}