using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public class ThrowItemsFromFloorState : GoblinState
{
    private GameObject _itemToThrow;
        
    public ThrowItemsFromFloorState(GoblinAI goblinAI) : base(goblinAI)
    {
    }

    public override void EnterState()
    {
        Debug.Log("Entering Throw Items from floor state");
        _goblinAI.StartCoroutine(ThrowItems());
    }

    public override void ExitState()
    {
        _itemToThrow = null;
        _goblinAI.StopCoroutine(ThrowItems());
    }
    
    private IEnumerator ThrowItems()
    {
        _itemToThrow = GetItem();

        if (_itemToThrow == null)
        {
            _goblinAI.ChangeState();
            yield break;
        }

        // find the items last position
        var lastKnownPosition = _itemToThrow.transform.position;
        _goblinAI.SetDestination(lastKnownPosition);

        while (!ReachedFloorObject())
        {
            if (!IsItemValid(_itemToThrow))
            {
                AudioManager.instance.sfxManager.StopConstantSFX();
                _goblinAI.ChangeState(); // ensures transition to new state
                yield break;
            }

            // Update destination if the item moved
            if (_itemToThrow != null && _itemToThrow.transform.position != lastKnownPosition)
            {
                lastKnownPosition = _itemToThrow.transform.position;
                _goblinAI.SetDestination(lastKnownPosition);
            }

            yield return null;
        }
        
        // If item became invalid after reaching it
        if (!IsItemValid(_itemToThrow))
        {
            _goblinAI.ChangeState();
            yield break;
        }

        // Play throwing animation
        AudioManager.instance.sfxManager.StopConstantSFX();
        var randomDir = new Vector3(Random.Range(-1f, 1f), 1, Random.Range(-1f, 1f)).normalized;
        _itemToThrow.transform.DOJump(randomDir, 1, 1, 0.5f);

        yield return new WaitForSeconds(_goblinAI.GetPauseTime()); 

        // Ensure state transition after throwing
        _goblinAI.ChangeState();
    }

    private bool IsItemValid(GameObject item)
    {
        if (item == null) return false;

        // Check if item is a PickupObject and has been added to the cauldron or picked up
        var pickup = item.GetComponent<PickupObject>();
        if (pickup != null)
        {
            if (pickup.isHeld || pickup.AddedToCauldron()) return false;
        }

        // Check if item is a PotionOutput and has been given to a customer
        var potion = item.GetComponent<PotionOutput>();
        if (potion != null)
        {
            if (potion.GivenToCustomer) return false;
        }

        return true;
    }

    private bool ReachedFloorObject()
    {
        var distance = Vector3.Distance(_goblinAI.transform.position, _itemToThrow.transform.position);
        return distance <= _goblinAI.AgentStoppingDistance() || distance <= _goblinAI.ThrowingDistance();
    }
    
    
    /// <summary> Used to find all the ingredients on the floor </summary>
    private GameObject GetItem()
    {
        var ingredients = new List<GameObject>();
        var ing = Object.FindObjectsOfType<PickupObject>();
        var potions = Object.FindObjectsOfType<PotionOutput>();

        foreach (var ingredient in ing)
        {
            if (!ingredient.AddedToCauldron() && !ingredient.isHeld)
                ingredients.Add(ingredient.gameObject);
        }

        foreach (var potion in potions)
        {
            var pickup = potion.GetComponent<PickupObject>();

            if (!potion.GivenToCustomer && !pickup.isHeld)
                ingredients.Add(potion.gameObject);
        }

        if (ingredients.Count == 0) return null;
        
        return ingredients[Random.Range(0, ingredients.Count)];
    }

}