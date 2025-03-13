using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [Header("Order Variables")]
    [SerializeField] private RecipeManager recipeManager;
    private List<RecipeSO> _availableRecipes = new();


    private void Start()
    {
        recipeManager = FindObjectOfType<RecipeManager>();
        GetAvailableRecipes();
    }

    private void GetAvailableRecipes()
    {
        foreach (var recipe in recipeManager.FindAvailableRecipes())
        {
            for(var i = 0; i < recipe.weight; i++)
            {
                _availableRecipes.Add(recipe);
            }
        }
    }

    // Generate a random order for a customer
    internal RecipeSO GiveOrder(string customerName)
    {
        // If there are no available recipes, return
        if (_availableRecipes.Count == 0) return null;

        RecipeSO assignedOrder;

        if(customerName == "Evil Mage")
        {
            assignedOrder = _availableRecipes[0];
        }
        else
        {
            var randomIndex = Random.Range(0, _availableRecipes.Count);
            assignedOrder = _availableRecipes[randomIndex];
        }

        return assignedOrder;
    }

    internal RecipeSO TutorialOrder()
    {
        if(_availableRecipes.Count == 0) return null;
        var assignedOrder = _availableRecipes[4];
        return assignedOrder;
    }
} 