using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.UI;

public class RecipeBookRenderer : MonoBehaviour
{
    [Header("Recipe Ui Components")]
    [SerializeField] private RecipeUI recipeUiLeft;
    [SerializeField] private RecipeUI recipeUiRight;
    
    [Header("Button Navigation")]
    [SerializeField] private BookNavigation next;
    [SerializeField] private BookNavigation previous;
   
    [Header("Gamepad Icons")]
    [SerializeField] private GamepadIcons xboxIcons;
    [SerializeField] private GamepadIcons ps4Icons;
    
    private int _firstRecipeIndex;
    private int _secondRecipeIndex;


    internal void RenderPage(int pageNumber, List<RecipeSO> recipes)
    {
        ClearPage();
        UpdatePageButtons(pageNumber, recipes.Count);
        
        _firstRecipeIndex = pageNumber * 2;
        _secondRecipeIndex = _firstRecipeIndex + 1;

        if(_firstRecipeIndex < recipes.Count)
            FillPage(recipes[_firstRecipeIndex], recipeUiLeft);
        
        if(_secondRecipeIndex < recipes.Count)
            FillPage(recipes[_secondRecipeIndex], recipeUiRight);
    }

    private void FillPage(RecipeSO recipe, RecipeUI recipeUI)
    {
        recipeUI.recipeObject.SetActive(true);
        recipeUI.potionIcon.sprite = recipe.potionIcon;
        recipeUI.recipeNameSprite.sprite = recipe.recipeNameIcon;
        // recipeUI.recipeName.text = recipe.recipeName;
        recipeUI.potionPrice.text = $"Sells For: {recipe.sellAmount.ToString()}";
        
        CreateSteps(recipe,  recipeUI);
    }

    private void ClearPage()
    {
        recipeUiLeft.recipeObject.SetActive(false);
        recipeUiRight.recipeObject.SetActive(false);
    }


    private void CreateSteps(RecipeSO recipe, RecipeUI recipeUI)
    {
        var totalSteps = recipe.steps.Length;

        for (var i = 0; i < recipeUI.stepVisual.Length; i++)
        {
            var visual = recipeUI.stepVisual[i];

            if (i >= totalSteps)
            {
                visual.mainObject.SetActive(false);
                continue;
            }
            
            visual.mainObject.SetActive(true);
            var step = recipe.steps[i];
            var mainImage = visual.mainObject.GetComponent<Image>();

            mainImage.sprite = step.stepSprite;

            if (step.action == RecipeStepSO.ActionType.AddIngredient)
            {
                SetStirElements(visual, false, false);
                continue;
            }

            var isController = FirstSelect.IsControllerControlling;
            SetStirElements(visual, isController, !isController);
            SetStirDisplay(visual, step, isController);
        }
    }

    private void SetStirElements(RecipeVisuals visual, bool enableIcon, bool enableText)
    {
        visual.stirIcon.enabled = enableIcon;
        visual.stirButtonText.enabled = enableText;
    }

    /// <summary> ets the appropriate stir icon or button text based on control mode. </summary>
    private void SetStirDisplay(RecipeVisuals visual, RecipeStepSO step, bool isController)
    {
        var stirActions = new Dictionary<string, InputAction>
        {
            {"Stir_C", InputManager.Instance.StirCAction},
            {"Stir_CC", InputManager.Instance.StirCcAction}
        };
        
        stirActions.TryGetValue(step.stepName, out var action);
        
        if (isController)
        {
            visual.stirIcon.sprite = PickIcon(action.GetBindingDisplayString(1));
            return;
        }

        visual.stirButtonText.text = action.GetBindingDisplayString(0);
    }

    private void UpdatePageButtons(int pageNumber, int totalRecipes)
    {
        previous.button.SetActive(pageNumber > 0);
        next.button.SetActive((pageNumber + 1) * 2 < totalRecipes);
        
        SetNavigationButtons();
    }


    private void SetNavigationButtons()
    {
        var isController = FirstSelect.IsControllerControlling;
        
        next.icon.enabled = isController;
        previous.icon.enabled = isController;
        
        next.text.enabled = !isController;
        previous.text.enabled = !isController;

        if (isController)
        {
            next.icon.sprite = PickIcon(InputManager.Instance.NextPageInputAction.GetBindingDisplayString(1));
            previous.icon.sprite = PickIcon(InputManager.Instance.PreviousPageInputAction.GetBindingDisplayString(1));
            return;
        }
        
        next.text.text = InputManager.Instance.NextPageInputAction.GetBindingDisplayString(0);
        previous.text.text = InputManager.Instance.PreviousPageInputAction.GetBindingDisplayString(0);
    }
    
    // Picks icon for controller 
    private Sprite PickIcon(string displayString)
    {
        var gamepad = Gamepad.current;

        // Debug.Log($"Looking for icon for: {displayString}");

        // returns an icon depending on what's connected. Default is xbox icons
        return gamepad switch
        {
            XInputControllerWindows => xboxIcons.GetSprite(displayString),
            DualShockGamepad => ps4Icons.GetSprite(displayString),
            _ => xboxIcons.GetSprite(displayString)
        };
    }
}
