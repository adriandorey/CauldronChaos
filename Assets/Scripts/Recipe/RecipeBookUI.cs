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
    private Sprite previousPageSprite;
    private Sprite nextPageSprite;

    private int _pageNumber;

    [SerializeField] private GamepadIcons xboxIcons;
    [SerializeField] private GamepadIcons ps4Icons;

    // Recipe UI Components
    [SerializeField] private RecipeUI recipeUiLeft;
    [SerializeField] private RecipeUI recipeUiRight;


    private void Start()
    {
        previousPageSprite = previousPage.GetComponent<Image>().sprite;
        nextPageSprite = nextPage.GetComponent<Image>().sprite;

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
            previousPage.GetComponent<Image>().sprite = PickIcon(InputManager.Instance.PreviousPageInputAction.GetBindingDisplayString(0));
            nextPage.GetComponent<Image>().sprite = PickIcon(InputManager.Instance.NextPageInputAction.GetBindingDisplayString(0));
        }


        ClearPage(); // Clear current UI elements

        int firstRecipeIndex = _pageNumber * 2; // First recipe on the current screen
        int secondRecipeIndex = firstRecipeIndex + 1; // Second recipe on the current screen

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

    private void CreateIngredientUI(RecipeUI recipeUI, int recipeIndex)
    {
        int totalSteps = _availableRecipes[recipeIndex].steps.Length;

        for (int i = 0; i < recipeUI.recipeStepUI.Length; i++)
        {

            if (i < totalSteps)
            {
                recipeUI.recipeStepUI[i].SetActive(true);
                TextMeshProUGUI stirText = recipeUI.recipeStepUI[i].GetComponentInChildren<TextMeshProUGUI>();
                Sprite stepSprite;

                if (_availableRecipes[recipeIndex].steps[i].action == RecipeStepSO.ActionType.AddIngredient)
                {
                    recipeUI.recipeStepIcon[recipeIndex].enabled = false;
                    stepSprite = _availableRecipes[recipeIndex].steps[i].ingredientSprite;
                    recipeUI.recipeStepIcon[recipeIndex].enabled = false;
                    stirText.enabled = false;

                }
                else
                {
                    stepSprite = _availableRecipes[recipeIndex].steps[i].stirSprite;

                    if (FirstSelect.IsControllerControlling)
                    {
                        stirText.enabled = false;
                        recipeUI.recipeStepIcon[recipeIndex].enabled = true;

                        if (_availableRecipes[recipeIndex].steps[i].stepName == "Stir_C")
                        {
                            recipeUI.recipeStepIcon[recipeIndex].sprite = PickIcon(InputManager.Instance.StirCAction.GetBindingDisplayString(1));
                        }
                        else
                        {
                            recipeUI.recipeStepIcon[recipeIndex].sprite = PickIcon(InputManager.Instance.StirCcAction.GetBindingDisplayString(1));
                        }
                    }
                    else
                    {
                        recipeUI.recipeStepIcon[recipeIndex].enabled = false;
                        stirText.enabled = true;

                        if (_availableRecipes[recipeIndex].steps[i].stepName == "Stir_C")
                        {
                            stirText.text = InputManager.Instance.StirCAction.GetBindingDisplayString(0);
                        }
                        else
                        {
                            stirText.text = InputManager.Instance.StirCcAction.GetBindingDisplayString(0);
                        }

                    }

                }

                var stepImage = recipeUI.recipeStepUI[i].GetComponent<Image>();

                stepImage.sprite = stepSprite;
                stepImage.preserveAspect = true;
            }
            else
            {
                recipeUI.recipeStepUI[i].SetActive(false);
            }
        }
    }

    #region Navigation

    private void NextPage(InputAction.CallbackContext input)
    {
        if (!input.performed) return;
        if ((_pageNumber + 1) * 2 < _availableRecipes.Length)
        {
            _pageNumber++;
            SetRecipes();
        }
    }

    private void PreviousPage(InputAction.CallbackContext input)
    {
        if (!input.performed) return;
        if (_pageNumber > 0)
        {
            _pageNumber--;
            SetRecipes();
        }
    }

    public void ButtonNavigation(bool isNext)
    {
        if (isNext)
        {
            if ((_pageNumber + 1) * 2 < _availableRecipes.Length)
            {
                _pageNumber++;
                SetRecipes();
            }
        }
        else
        {
            if (_pageNumber > 0)
            {
                _pageNumber--;
                SetRecipes();
            }
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

        // if its an xbox controller it will pick from xbox icons
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
    public GameObject[] recipeStepUI;
    public Image[] recipeStepIcon;
}

