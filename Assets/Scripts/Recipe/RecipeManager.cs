using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class RecipeManager : MonoBehaviour
{
    [Header("Available Recipes")]
    [SerializeField] private RecipeSO[] allRecipes;
    [SerializeField] private int numberOfRecipes;
    [SerializeField] private GameObject recipeBookUi;
    [SerializeField] private bool useAllRecipes;
    [SerializeField] private GameObject closeButton;

    private void Update()
    {
        if (!recipeBookUi.activeSelf) return;
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleRecipeBook();
        }
    }

    #region OnEnable / OnDisable / OnDestroy Events

    private void OnEnable()
    {
        Actions.OnToggleRecipeBook += ToggleRecipeBook;
    }

    private void OnDisable()
    {
        Actions.OnToggleRecipeBook -= ToggleRecipeBook;
    }

    private void OnDestroy()
    {
        Actions.OnToggleRecipeBook -= ToggleRecipeBook;
    }

    #endregion

    public RecipeSO[] FindAvailableRecipes()
    {
        var availableRecipes = new RecipeSO[numberOfRecipes];

        if (useAllRecipes)
        {
            availableRecipes = allRecipes;
        }
        else
        {
            for (var i = 0; i < numberOfRecipes; i++)
            {
                availableRecipes[i] = allRecipes[i];
            }
        }

        return availableRecipes;
    }

    internal RecipeSO GetRandomRecipe()
    {
        return allRecipes[Random.Range(0, numberOfRecipes)];
    }

    internal RecipeSO GetRecipeByName(string recipeName)
    {
        return allRecipes.FirstOrDefault(recipe => recipe.recipeName == recipeName);
    }

    public void ToggleRecipeBook()
    {
        if (recipeBookUi.activeSelf)
        {
            Time.timeScale = 1;
            recipeBookUi.SetActive(false);
            Actions.OnSetUiLocation(Page.Gameplay);
            InputManager.OnGameplayInputs();
        }
        else
        {
            Time.timeScale = 0;
            recipeBookUi.SetActive(true);
            Actions.OnSetUiLocation(Page.RecipeBook);
            Actions.OnSelectRecipeButton(closeButton);
            InputManager.OnRecipeBookInputs();
        }
    }
}