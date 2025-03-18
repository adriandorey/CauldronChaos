using System.Collections.Generic;
using UnityEngine;

public class PickupDetection : MonoBehaviour
{
    private List<PickupObject> pickupObjects = new List<PickupObject>(); //list of objects in the pickup floor

    //Function that is called when a collider enters the trigger volume
    private void OnTriggerEnter(Collider other)
    {
        //try to get pickup component
        PickupObject pickup = other.GetComponent<PickupObject>();

        //if component exists add to list
        if (pickup != null)
        {
            //Debug.Log("Pickup_Drop detected");
            pickupObjects.Add(pickup);
            Actions.OnShowPickup?.Invoke();
        }
    }

    //Function that is called when a collider exits the trigger volume
    private void OnTriggerExit(Collider other)
    {
        //try to get pickup component
        PickupObject pickup = other.GetComponent<PickupObject>();

        //if component exists remove to list
        if (pickup != null)
        {
            //Debug.Log("Pickup_Drop no longer detected");
            pickupObjects.Remove(pickup);

            if (pickupObjects.Count == 0)
                Actions.OnHideUI?.Invoke();
        }
    }

    //Function that returns the first stored pickup in the pickup
    public PickupObject GetPickup()
    {
        //declare return value
        PickupObject pickup = null;

        //check if there are any objects in pickup volume
        if (pickupObjects.Count > 0)
        {
            //store pickup in return value and remove from list
            pickup = pickupObjects[0];
            pickupObjects.RemoveAt(0);
        }

        return pickup;
    }

    //Function that adds a pickup to the list of pickups
    public void AddPickupToList(PickupObject pickup)
    {
        //Debug.Log("Pickup_Drop detected");
        pickupObjects.Add(pickup);
    }

    //Function that removes a specified pickup from the list of pickups
    public void RemovePickupFromList(PickupObject pickup)
    {
        //Debug.Log("Remove pickup called");
        pickupObjects.Remove(pickup);
    }
}
