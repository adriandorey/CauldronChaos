using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    [Header("Available Recipes")]
    [SerializeField] private RecipeSO[] allRecipes;
    [SerializeField] private int numberOfRecipes;
    [SerializeField] private bool useAllRecipes;

    private List<RecipeSO> _weightedRecipes;
    private List<RecipeSO> _availableRecipes;

    private void Awake()
    {
        GenerateAvailableRecipes();
        GenerateWeightedRecipes();
        
    }

    private void GenerateAvailableRecipes()
    {
        // store unweighted recipes for UI / general availability
        _availableRecipes = useAllRecipes 
            ? allRecipes.ToList()
            : allRecipes.Take(numberOfRecipes).ToList();
    }
    
    private void GenerateWeightedRecipes()
    {
        _weightedRecipes = new List<RecipeSO>();

        // this will go through each recipe in available recipe and create a list.
        foreach (var recipe in _availableRecipes)
        {
            // this list will include the amount set in the weight part of the recipes.
            _weightedRecipes.AddRange(Enumerable.Repeat(recipe, recipe.weight));
        }
    }

    internal List<RecipeSO> GetWeightedRecipes() => _weightedRecipes;

    internal List<RecipeSO> GetAvailableRecipes() => _availableRecipes;

    internal RecipeSO GetHydrationRecipe() => allRecipes[0];

    internal RecipeSO GetTutorialRecipe() => allRecipes[1];
}