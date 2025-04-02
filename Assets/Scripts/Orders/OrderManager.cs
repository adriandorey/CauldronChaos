using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [Header("Order Variables")]
    [SerializeField] private RecipeManager recipeManager;
    private List<RecipeSO> _availableRecipes = new();
    private RecipeSO _tutorialRecipe;
    private RecipeSO _hydrationRecipe;


    private Dictionary<string, RecipeSO> _assignedOrders;

    private void Start()
    {
        recipeManager = FindObjectOfType<RecipeManager>();
        _availableRecipes = recipeManager.GetWeightedRecipes();
        _tutorialRecipe = recipeManager.GetTutorialRecipe();
        _hydrationRecipe = recipeManager.GetHydrationRecipe();

        // Sets up the two assigned orders
        _assignedOrders = new Dictionary<string, RecipeSO>()
        {
            { "Evil Mage", _hydrationRecipe },
            {"Tutorial", _tutorialRecipe },
        };
    }

    // Generate a random order for a customer
    internal RecipeSO GiveOrder(string customerName)
    {
        // If there are no available recipes, return
        if (_availableRecipes.Count == 0) return null;

        // Check to see if it's an assigned order, if not, pick a random recipe
        return _assignedOrders.TryGetValue(customerName, out var recipe)
            ? recipe
            : PickRandomRecipe();
    }

    // returns a random recipe out of all available recipes.
    private RecipeSO PickRandomRecipe()
    {
        var randomIndex = Random.Range(0, _availableRecipes.Count);
        return _availableRecipes[randomIndex];
    }
} 