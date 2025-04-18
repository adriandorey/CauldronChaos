using UnityEngine;
using System;

public class CauldronTrigger : MonoBehaviour
{
    [SerializeField] private CauldronInteraction cauldron; //cached reference to the cauldron
  
    //Function called whenever a collider enters the trigger volume
    private void OnTriggerStay(Collider other)
    {
        //try to get ingredient component of collider
        var ingredientHolder = other.GetComponent<PickupItem>();
        if (ingredientHolder != null && !ingredientHolder.AddedToCauldron() && !ingredientHolder.IsHeld)
        {
            ingredientHolder.InsertInCauldron();
            cauldron.AddIngredient(ingredientHolder.gameObject);
        }
    }
}
