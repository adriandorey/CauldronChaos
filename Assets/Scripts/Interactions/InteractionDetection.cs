using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionDetection : MonoBehaviour
{
    [SerializeField] private List<Interactable> interactables = new List<Interactable>(); //list of interactables within the interactable space

    //Function that is called when collider enters the trigger volume
    private void OnTriggerEnter(Collider other)
    {
        //try to get interactable component
        Interactable interactable = other.GetComponent<Interactable>();

        //if component exists add to list
        if (interactable != null)
        {
            interactables.Add(interactable);

            if (interactable.IsContainer())
            {
                Actions.OnShowPickup?.Invoke();
            }
            else
            {
                Actions.OnShowInteraction?.Invoke();
            }
        }
    }

    //Function that is called when a collider exits the trigger volume
    private void OnTriggerExit(Collider other)
    {
        //try to get interactable component
        Interactable interactable = other.GetComponent<Interactable>();

        //if component exists add to list
        if (interactable != null)
        {
            interactables.Remove(interactable);

            if (interactables.Count == 0)
                Actions.OnHideUI?.Invoke();
        }
    }

    //Function that returns the first interactable in the list that do not require being held
    public Interactable GetFirstNonHeldInteractable()
    {
        //loop through interactables list
        for (int i = 0; i < interactables.Count; i++)
        {
            //if does not require being picked up for use return
            if (!interactables[i].MustBePickedUp() && !interactables[i].IsContainer())
            {
                return interactables[i];
            }
        }

        return null; //return null if nothing was found
    }

    //Function that searched through the interactables and returns the first crate
    public Interactable GetContainer()
    {
        if(interactables.Count == 0) return null;
        
        //loop through interactables list
        for (int i = 0; i < interactables.Count; i++)
        {
            
            if(interactables[i] == null) continue;
            
            //if does not require being picked up for use return
            if (interactables[i].IsContainer())
            {
                return interactables[i];
            }
        }

        return null; //return null if nothing was found
    }
}
