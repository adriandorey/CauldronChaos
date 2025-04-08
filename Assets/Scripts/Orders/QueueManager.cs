using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QueueManager : MonoBehaviour
{
    [Header("Customer Prefabs")]
    [SerializeField] private GameObject tutorialCustomer;
    [SerializeField] private List<GameObject> customerPrefabs; // List of possible customer prefabs

    [Header("Customer Queue Variables")]
    [SerializeField] private int timeForCustomer = 3;
    private List<GameObject> _customers = new();
    private bool _startCustomers;
    private int _customerIndex;
    private const int MaxCustomers = 5;

    [Header("Customer Queue Positions")]
    [SerializeField] private Transform firstPos;
    [SerializeField] private Transform entryPoint; // Spawn point for new customers
    [SerializeField] private Transform exitPoint; // Spawn point for new customers
    private Vector3[] _queuePositions = new Vector3[5]; //queue positions

    [Header("References")]
    [SerializeField] private OrderManager orderManager;

    [Header("Order Holder")]
    [SerializeField] private GameObject orderHolder;

    [Header("SFX")]
    [SerializeField] private AudioClip potionSaleSfx;

    private TutorialManager _tutorialManager;

    private Coroutine _spawnCoroutine;

    public void Start()
    {
        SetQueue();
        _tutorialManager = FindObjectOfType<TutorialManager>();
    }

    #region OnEnable / OnDisable / OnDestroy Events
    private void OnEnable() => SubscribeToEvents();
    private void OnDisable() => UnsubscribeFromEvents();
    private void OnDestroy() => UnsubscribeFromEvents();
    #endregion

    /// <summary> Starts the Customer Queue, timer and spawns customer </summary>
    private void StartCustomerQueue()
    {
        _startCustomers = true;
        
        if (_spawnCoroutine != null) return; // Prevent multiple coroutines from being started.
        _spawnCoroutine = StartCoroutine(CustomerSpawner());
    }

    private IEnumerator CustomerSpawner()
    {
        while (_startCustomers)
        {
            yield return new WaitForSeconds(timeForCustomer);
            
            if(CanAddCustomer())
                SpawnCustomer();
        }
    }

    internal void SpawnCustomer()
    {
        var newCustomer = Instantiate(PickCustomer(), entryPoint.position,  Quaternion.identity);
        var customer = newCustomer.GetComponent<CustomerOrder>();

        AssignOrder(customer);
    }
    
    private GameObject PickCustomer()
    {
        var customer = GameManager.Instance.IsInTutorial() 
            ? tutorialCustomer 
            : customerPrefabs[Random.Range(0, customerPrefabs.Count)];
    
        return customer;
    }

    
    private void AssignOrder(CustomerOrder customer)
    {
        var order = orderManager.GiveOrder(customer.customerName);
        customer.Assign(order, orderHolder.transform);

        AddToQueue(customer.gameObject);
    }

    private bool CanAddCustomer() => _customers.Count < MaxCustomers;

    internal void CheckCustomerRecipes(PotionOutput potionOutput)
    {
        var potion = potionOutput.potionInside;

        foreach (var person in _customers)
        {
            var order = person.GetComponent<CustomerOrder>();
            var movement = person.GetComponent<CustomerMovement>();
            
            if(order.RequestedOrder != potion || !movement.HasJoinedQueue() || order.HasReceivedPotion()) 
                continue;
            
            order.Complete();
            //Debug.Log("Order received");
            potionOutput.JumpToCustomer(order, () => FinishOrder(order));
            return;
        }
        
        potionOutput.NoCustomerAvailable();
    }


    private void FinishOrder(CustomerOrder customer)
    {
        // Debug.Log("Finishing order");
        // customer.OrderComplete(); 
        RemoveCustomer(customer.gameObject);
        
        //playing SFX for potion sale
        AudioManager.instance.sfxManager.PlaySFX(SFX_Type.ShopSounds, potionSaleSfx, true);
        
        if(!GameManager.Instance.IsInTutorial()) return;
        
        Actions.OnPotionServed?.Invoke();
        _tutorialManager.ServedCustomer();
    }

    private void RemoveCustomer(GameObject customer)
    {
        if (_customers.Contains(customer) && customer.GetComponent<CustomerMovement>().HasJoinedQueue())
        {
            _customers.Remove(customer);
            customer.GetComponent<CustomerMovement>().LeaveQueue(exitPoint.position);
            UpdateQueuePositions();
        }
    }


    public GameObject GetRandomCustomer()
    {
        // Filters the customers list and keeps only those that are in queue
        var customersInQueue = _customers.
            Where(c => c.GetComponent<CustomerMovement>().HasJoinedQueue())
            .ToList();
    
        // returns a random customer that is in queue.
        return customersInQueue.Count == 0 ? null : customersInQueue[Random.Range(0, customersInQueue.Count)];
    }
    
    internal void ScareCustomer(GameObject customerToScare)
    {
        if (customerToScare == null) return;
        
        customerToScare.GetComponent<CustomerMovement>().ScareAway(exitPoint.position);
        RemoveCustomer(customerToScare);
    }
  
    #region Queue Methods
    /// <summary> Sets the position of the customer queue </summary>
    private void SetQueue()
    {
        _queuePositions[0] = firstPos.position;
        for (var i = 1; i < MaxCustomers; i++)
        {
            _queuePositions[i] = firstPos.position + new Vector3(i, 0, 0);
        }
    }

    /// <summary> Adds customer to list and updates the queue positions </summary>
    private void AddToQueue(GameObject customer)
    {
        _customers.Add(customer);
        UpdateQueuePositions();
    }
    
    /// <summary> Tells all customers in that line to update their positions </summary>
    private void UpdateQueuePositions()
    {
        for (var i = 0; i < _customers.Count; i++)
        {
            _customers[i].GetComponent<CustomerMovement>().SetTarget(_queuePositions[i]);
        }
    }
    
    private void ResetQueue()
    {
        if(_spawnCoroutine != null)
            StopCoroutine(_spawnCoroutine);
        
        _startCustomers = false;
    
        foreach (var customer in _customers)
        {
            Destroy(customer);
        }
    
        _customers.Clear();
    }
    #endregion

    private void SubscribeToEvents()
    {
        Actions.OnEndDay += ResetQueue;
        Actions.OnStartDay += StartCustomerQueue;
        Actions.OnResetValues += ResetQueue;
    }

    private void UnsubscribeFromEvents()
    {
        Actions.OnEndDay -= ResetQueue;
        Actions.OnStartDay -= StartCustomerQueue;
        Actions.OnResetValues -= ResetQueue;
    }
}