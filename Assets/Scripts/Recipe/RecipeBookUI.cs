using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.UI;

public class RecipeBookUI : MonoBehaviour
{
    [SerializeField] private RecipeManager recipeManager;
    [SerializeField] private GameObject recipeUiLeftObj;
    [SerializeField] private GameObject recipeUiRightObj;

    private RecipeSO[] _availableRecipes;

    [Header("Page Buttons")]
    [SerializeField] private GameObject previousPage;

    [SerializeField] private GameObject nextPage;
    private Sprite _previousPageSprite;
    private Sprite _nextPageSprite;

    private int _pageNumber;

    // Recipe UI Components
    [Header("Recipe UI Components")]
    [SerializeField] private RecipeUI recipeUiLeft;

    [SerializeField] private RecipeUI recipeUiRight;

    [Header("Stirring Sprites")]
    [SerializeField] private Sprite clockSprite;

    [SerializeField] private Sprite counterClockSprite;

    [Header("Gamepad Icons")]
    [SerializeField] private GamepadIcons xboxIcons;

    [SerializeField] private GamepadIcons ps4Icons;

    private void Start()
    {
        _previousPageSprite = previousPage.GetComponent<Image>().sprite;
        _nextPageSprite = nextPage.GetComponent<Image>().sprite;

        _availableRecipes = recipeManager.FindAvailableRecipes();

        if (_pageNumber == 0)
            previousPage.SetActive(false);

        if (_availableRecipes.Length - 1 < 2)
            nextPage.SetActive(false);

        SetRecipes();
    }

    #region OnEnable / OnDisable / OnDestroy Events

    private void OnEnable()
    {
        InputManager.NextPageAction += NextPage;
        InputManager.PreviousPageAction += PreviousPage;
    }

    private void OnDisable()
    {
        InputManager.NextPageAction -= NextPage;
        InputManager.PreviousPageAction -= PreviousPage;
    }

    private void OnDestroy()
    {
        InputManager.NextPageAction -= NextPage;
        InputManager.PreviousPageAction -= PreviousPage;
    }

    #endregion


    private void SetRecipes()
    {
        if (FirstSelect.IsControllerControlling)
        {
            previousPage.GetComponent<Image>().sprite =
                PickIcon(InputManager.Instance.PreviousPageInputAction.GetBindingDisplayString(0));
            nextPage.GetComponent<Image>().sprite =
                PickIcon(InputManager.Instance.NextPageInputAction.GetBindingDisplayString(0));
        }


        ClearPage(); // Clear current UI elements

        var firstRecipeIndex = _pageNumber * 2; // First recipe on the current screen
        var secondRecipeIndex = firstRecipeIndex + 1; // Second recipe on the current screen

        // Fill left page if a recipe exists
        if (firstRecipeIndex < _availableRecipes.Length)
        {
            FillLeftPage(firstRecipeIndex);
        }

        // Fill right page if a recipe exists
        if (secondRecipeIndex < _availableRecipes.Length)
        {
            FillRightPage(secondRecipeIndex);
        }

        UpdatePageButtons();
    }


    private void FillLeftPage(int recipeIndex)
    {
        recipeUiLeftObj.SetActive(true);
        recipeUiLeft.recipeName.text = _availableRecipes[recipeIndex].recipeName;
        recipeUiLeft.potionIcon.sprite = _availableRecipes[recipeIndex].potionIcon;
        recipeUiLeft.potionIcon.preserveAspect = true;
        recipeUiLeft.potionPrice.text = _availableRecipes[recipeIndex].sellAmount.ToString();
        CreateIngredientUI(recipeUiLeft, recipeIndex);
    }

    private void FillRightPage(int recipeIndex)
    {
        recipeUiRightObj.SetActive(true);
        recipeUiRight.recipeName.text = _availableRecipes[recipeIndex].recipeName;
        recipeUiRight.potionIcon.sprite = _availableRecipes[recipeIndex].potionIcon;
        recipeUiRight.potionIcon.preserveAspect = true;
        recipeUiRight.potionPrice.text = _availableRecipes[recipeIndex].sellAmount.ToString();
        CreateIngredientUI(recipeUiRight, recipeIndex);
    }

    private void ClearPage()
    {
        recipeUiRightObj.SetActive(false);
        recipeUiLeftObj.SetActive(false);
    }

    #region Ui Ingredient
    
    // Creates the Ingredients UI
    private void CreateIngredientUI(RecipeUI recipeUI, int recipeIndex)
    {
        var recipe = _availableRecipes[recipeIndex];
        var totalSteps = recipe.steps.Length;

        for (var i = 0; i < recipeUI.recipeVisuals.Length; i++)
        {
            var visual = recipeUI.recipeVisuals[i];

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
            }
            else
            {
                var isController = FirstSelect.IsControllerControlling;
                SetStirElements(visual, isController, !isController);

                if (step.stepName == "Stir_C")
                {
                    // mainImage.sprite = clockSprite;
                    SetStirDisplay(visual, isController, InputManager.Instance.StirCAction);
                }
                else
                {
                    // mainImage.sprite = counterClockSprite;
                    SetStirDisplay(visual, isController, InputManager.Instance.StirCcAction);
                }
            }
        }
    }


    /// <summary> Enable or disable stir elements based on input mode </summary>
    private void SetStirElements(RecipeVisuals visual, bool enableIcon, bool enableText)
    {
        visual.stirIcon.enabled = enableIcon;
        visual.stirButtonText.enabled = enableText;
    }

    /// <summary> ets the appropriate stir icon or button text based on control mode. </summary>
    private void SetStirDisplay(RecipeVisuals visual, bool isController, InputAction inputAction)
    {
        if (isController)
        {
            visual.stirIcon.sprite = PickIcon(inputAction.GetBindingDisplayString(1));
            return;
        }

        visual.stirButtonText.text = inputAction.GetBindingDisplayString(0);
    }
    
    #endregion


    #region Recipe Book Navigation
    // This is used for the controller next page navigation
    private void NextPage(InputAction.CallbackContext input)
    {
        if (!input.performed) return;
        if ((_pageNumber + 1) * 2 < _availableRecipes.Length)
        {
            _pageNumber++;
            SetRecipes();
        }
    }

    // This is used for the controller previous page navigation
    private void PreviousPage(InputAction.CallbackContext input)
    {
        if (!input.performed) return;
        if (_pageNumber > 0)
        {
            _pageNumber--;
            SetRecipes();
        }
    }
    
    // This is used for the physical buttons on the book
    public void ButtonNavigation(bool isNext)
    {
        if (isNext)
        {
            if ((_pageNumber + 1) * 2 >= _availableRecipes.Length) return;
            
            _pageNumber++;
            SetRecipes();
        }
        else
        {
            if (_pageNumber <= 0) return;
            
            _pageNumber--;
            SetRecipes();
        }
    }

    private void UpdatePageButtons()
    {
        previousPage.SetActive(_pageNumber > 0);
        nextPage.SetActive((_pageNumber + 1) * 2 < _availableRecipes.Length);
    }

    #endregion

    // Picks icon for controller 
    private Sprite PickIcon(string displayString)
    {
        Sprite icon = null; // Ensure icon has a default value
        var gamepad = Gamepad.current;

        // if it's an xbox controller it will pick from xbox icons
        if (gamepad is XInputControllerWindows)
        {
            icon = xboxIcons.GetSprite(displayString);
        }
        // if an ps4 controller it will pick from xbox icons
        else if (gamepad is DualShockGamepad)
        {
            icon = ps4Icons.GetSprite(displayString);
        }
        else
        {
            // if it's neither, it will default to xbox icons
            //Debug.Log("Gamepad is not XInputController or DualShockGamepad");
            icon = xboxIcons.GetSprite(displayString);
        }

        return icon;
    }
}

[Serializable]
public class RecipeUI
{
    public TextMeshProUGUI recipeName;
    public Image potionIcon;
    public TextMeshProUGUI potionPrice;
    public RecipeVisuals[] recipeVisuals;
}

[Serializable]
public struct RecipeVisuals
{
    public GameObject mainObject;
    public Image stirIcon;
    public TextMeshProUGUI stirButtonText;
}