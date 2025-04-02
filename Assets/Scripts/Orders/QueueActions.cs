using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QueueActions : MonoBehaviour
{
    [SerializeField] private GameObject tutorialCustomer;
    [SerializeField] private List<GameObject> customerPrefabs; // List of possible customer prefabs
    
  
    // internal void RemoveCustomer(GameObject customer)
    // {
    //     if (!_customers.Contains(customer)) return;
    //
    //     _customers.Remove(customer);
    //     UpdateQueuePositions();
    // }
    //
    // internal void ScareCustomer()
    // {
    //     var customerToScare = GetRandomCustomer();
    //     if (customerToScare == null) return;
    //     
    //     RemoveCustomer(customerToScare);
    // }
    //
    // internal GameObject PickCustomer()
    // {
    //     var customer = GameManager.Instance.IsInTutorial() 
    //         ? tutorialCustomer 
    //         : customerPrefabs[Random.Range(0, customerPrefabs.Count)];
    //
    //     return customer;
    // }

    // public GameObject GetRandomCustomer()
    // {
    //     // Filters the customers list and keeps only those that are in queue
    //     var customersInQueue = _customers.
    //         Where(c => c.GetComponent<CustomerBehaviour>().HasJoinedQueue)
    //         .ToList();
    //
    //     // returns a random customer that is in queue.
    //     return customersInQueue.Count == 0 ? null : customersInQueue[Random.Range(0, customersInQueue.Count)];
    // }
}