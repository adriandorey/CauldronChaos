using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class Cauldron : MonoBehaviour
{
    // RecipeManager script
    private RecipeManager _recipeManager;

    private RecipeSO[] _availableRecipes;
    private List<RecipeSO> _possibleRecipeList = new();
    private string _lastStep;
    private string _currentStep;
    private int _stepIndex;
    private string _nextStep;

    private List<string> _recipeSteps = new();


    // recipe variables
    private RecipeSO _currentRecipe;
    private GameObject _ingredientAdded;
    private int _potionIndex;
    private bool _canInteract;

    [Header("Potion Insert Spot")]
    [SerializeField] private Transform ingredientInsertPoint;

    [Header("Potion Throwing")]
    [SerializeField] private float throwStrength = 5f; // strength of the throw

    [SerializeField] private float throwHeight = 2f; // max height of the arc
    [SerializeField] private float throwDuration = 1f; // time to reach target

    [Header("StirStick Rotation")]
    [Tooltip("Lower the number the slower it goes")]
    [SerializeField] private float stirRotationSpeed = 0.6f;

    [SerializeField] private Transform stirStick;

    [Header("Model Renderers")]
    [SerializeField] private Renderer modelRenderer;

    [SerializeField] private Renderer stirStickRend;

    private PickupBehaviour _player;

    public void Start()
    {
        // Get the RecipeManager script & set the craft-able recipes
        _recipeManager = FindObjectOfType<RecipeManager>();
        _availableRecipes = _recipeManager.FindAvailableRecipes();
        _player = FindObjectOfType<PickupBehaviour>();
    }

    #region OnEnable / OnDisable / OnDestroy Events

    private void OnEnable()
    {
        InputManager.StirClockwiseAction += StirClockwise;
        InputManager.StirCounterClockwiseAction += StirCounterClockwise;
    }

    private void OnDisable()
    {
        InputManager.StirClockwiseAction -= StirClockwise;
        InputManager.StirCounterClockwiseAction -= StirCounterClockwise;
    }

    private void OnDestroy()
    {
        InputManager.StirClockwiseAction -= StirClockwise;
        InputManager.StirCounterClockwiseAction -= StirCounterClockwise;
        DOTween.KillAll();
    }

    #endregion


    public void AddIngredient(GameObject ingredientObject)
    {
        // Set the ingredient to the current ingredient added
        _ingredientAdded = ingredientObject;
        var ingredientStep = ingredientObject.GetComponent<PickupObject>().recipeIngredient.stepName;


        // if the current step is something set it to the last step
        _currentStep = null;
        _currentStep = ingredientStep;
        _recipeSteps.Add(_currentStep);
        _lastStep = _currentStep;

        // grabs the ingredient from the _recipes step that's holding it.
        var ingredientSequence = DOTween.Sequence();

        if (_ingredientAdded != null)
        {
            transform.DOScale(1.2f, 0.08f).SetLoops(2, LoopType.Yoyo);
            ingredientSequence.Append(_ingredientAdded.transform
                    .DOLocalJump(ingredientInsertPoint.position, 1f, 1, 0.5f).SetEase(Ease.InOutSine))
                .Join(_ingredientAdded.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InOutSine))
                .OnComplete(SetInactive); // Call SetInactive after both tweens finish
        }

        // Play a sound here
        // AudioManager.instance.sfxManager.PlaySFX(SFX_Type.StationSounds, addIngredientSounds.PickAudioClip(), true);

        if (_stepIndex == 0)
        {
            StartNewRecipe();
        }
        else
        {
            CheckRecipeProgress();
        }
    }


    // Sets ingredient inactive after it has been thrown into the cauldron
    private void SetInactive()
    {
        if (_currentStep == "Bottle_Potion") return;

        if (_ingredientAdded != null)
        {
            DOTween.Kill(_ingredientAdded.transform);
            Destroy(_ingredientAdded);
        }
    }

    #region Stirring

    private void StirClockwise(InputAction.CallbackContext input)
    {
        if (input.performed)
        {
            if (!_canInteract) return;

            stirStick.DOLocalRotate(new Vector3(0, 360, 16), stirRotationSpeed, RotateMode.FastBeyond360);
            Stir("Stir_C");
        }
    }

    private void StirCounterClockwise(InputAction.CallbackContext input)
    {
        if (input.performed)
        {
            if (!_canInteract) return;

            stirStick.DOLocalRotate(new Vector3(0, -360, 16), stirRotationSpeed, RotateMode.FastBeyond360);
            Stir("Stir_CC");
        }
    }

    private void Stir(string direction)
    {
        //play stirring sound
        // AudioManager.instance.sfxManager.PlaySFX(SFX_Type.StationSounds, stirSounds.PickAudioClip(), false);

        if (_lastStep == direction) return;

        _currentStep = direction;
        _lastStep = direction;
        _recipeSteps.Add(direction);

        CheckRecipeProgress();
    }

    #endregion


    /// <summary> Starts new recipe </summary>
    private void StartNewRecipe()
    {
        Debug.Log("New Recipe Started");
        // clear possible recipe list
        _possibleRecipeList.Clear();
        _stepIndex = 0;

        // Cycle through all the recipes to see if any of them have the same first step
        foreach (var recipe in _availableRecipes)
        {
            if (recipe.steps[_stepIndex].stepName == _currentStep)
            {
                // Adds all possibilities to the list
                _possibleRecipeList.Add(recipe);
            }
        }

        switch (_possibleRecipeList.Count)
        {
            case 0: // If no possible recipes were found, handle incorrect step
                HandleIncorrectStep();
                return;
            case 1: // if one recipe was found check if its on its last step
            {
                _currentRecipe = _possibleRecipeList[0];
                Debug.Log($"New Recipe Started: {_currentRecipe.recipeName}");

                if (CheckForLastStep(_currentRecipe))
                {
                    CompletePotion();
                    return;
                }
                break;
            }
        }

        _stepIndex++;
    }

    private void CheckRecipeProgress()
    {
        if (GameManager.Instance.IsInTutorialMode)
            CheckTutorialSteps();

        if (_currentRecipe != null)
        {
            AdvanceToNextStep();
        }
        else
        {
            FindPossibleRecipes();
        }
    }

    private void CheckTutorialSteps()
    {
        if (TutorialManager.TutorialPartCount != 1) return;

        switch (TutorialManager.CurrentStep)
        {
            case TutorialStep.InsertIngredient when _currentStep == "Mushroom":
                TutorialManager.InsertCorrectIngredient = true;
                TutorialManager.LastCauldronUsed = modelRenderer;
                TutorialManager.StirStickToHighlight = stirStickRend;
                break;
            case TutorialStep.FillPotionBottle when
                TutorialManager.LastCauldronUsed == modelRenderer:
                break;
            case TutorialStep.StirCauldron
                when TutorialManager.LastCauldronUsed == modelRenderer && _currentStep == "Stir_C":
                TutorialManager.StirredCauldronCorrectly = true;
                break;
            case TutorialStep.StirCauldron
                when TutorialManager.LastCauldronUsed != modelRenderer:
                HandleIncorrectStep();
                break;
        }
    }

    private void FindPossibleRecipes()
    {
        List<RecipeSO> filteredRecipes = new();

        // Loop through all possible recipes
        foreach (var recipe in _possibleRecipeList)
        {
            if (recipe.steps[_stepIndex].stepName != _currentStep) continue;

            filteredRecipes.Add(recipe);

            // check if it's the last step
            if (!CheckForLastStep(recipe)) continue;

            CompletePotion();
            return;
        }

        _possibleRecipeList = filteredRecipes;

        if (_possibleRecipeList.Count == 0)
        {
            HandleIncorrectStep();
            return;
        }

        _stepIndex++;

        Debug.Log($"current step: {_stepIndex}");

        // If there's only one possible recipe, set it as the current recipe
        if (_possibleRecipeList.Count != 1) return;
        _currentRecipe = _possibleRecipeList[0];
        Debug.Log($"current recipe: {_currentRecipe}");
    }

    // Advance to the next step in the recipe
    private void AdvanceToNextStep()
    {
        // if there isn't a current recipe, find the possible recipe
        if (_currentRecipe == null)
        {
            Debug.Log("No recipe found yet. Looking in advance");
            FindPossibleRecipes();
            return;
        }

        // If an unexpected ingredient is added, trigger incorrect step handling
        if (_currentRecipe.steps[_stepIndex].stepName != _currentStep)
        {
            Debug.LogError($"Incorrect step detected!");
            HandleIncorrectStep();
            return;
        }

        // Check if it's the final step
        if (CheckForLastStep(_currentRecipe))
        {
            CompletePotion();
            return;
        }

        Debug.Log($"AdvanceToNextStep:Before Increment- Step {_stepIndex} / {_currentRecipe.steps.Length}");
        _stepIndex++;
        Debug.Log($"AdvanceToNextStep:After Increment - Current Step: {_stepIndex}");
    }

    private void HandleIncorrectStep()
    {
        if (GameManager.Instance.IsInTutorialMode)
            TutorialManager.MadeIncorrectMove = true;

        // Play a sound here
        // AudioManager.instance.sfxManager.PlaySFX(SFX_Type.StationSounds, incorrectStepSounds.PickAudioClip(), true);

        if (_ingredientAdded != null)
            _ingredientAdded.GetComponent<Rigidbody>().isKinematic = true;

        //_incorrectStep.Play();
        ResetValues();
    }

    private bool CheckForLastStep(RecipeSO recipe)
    {
        if (recipe.steps[_stepIndex].stepName != "Bottle_Potion") return false;
        
        _currentRecipe = recipe;
        return true;

    }


    private void CompletePotion()
    {
        if (!_ingredientAdded) return;
        _ingredientAdded.GetComponent<Rigidbody>().isKinematic = false;
        _ingredientAdded.GetComponent<PotionOutput>().enabled = true;
        _ingredientAdded.GetComponent<PotionOutput>().potionInside = _currentRecipe;
        _ingredientAdded.transform.SetParent(null);

        StartCoroutine(ThrowPotion());
    }

    private IEnumerator ThrowPotion()
    {
        // Remove the ingredient added and change it into a thrown potion - this is needed
        // without it, if you make an incorrect potion while the potion is midair, it will
        // destroy itself
        var thrownPotion = _ingredientAdded;
        _ingredientAdded = null;

        yield return new WaitForSeconds(0.3f);

        if (thrownPotion.TryGetComponent<PotionOutput>(out var potionOutput))
        {
            potionOutput.SetPotionColor();
        }

        // Play a sound here
        // AudioManager.instance.sfxManager.PlaySFX(SFX_Type.StationSounds, finishPotionSounds.PickAudioClip(), true);

        var startPosition = ingredientInsertPoint.position;
        var randomDirection = new Vector3(Random.Range(-1f, 1f), 1, Random.Range(-1f, 1f)).normalized;
        var targetPosition = startPosition + randomDirection * throwStrength;

        if (_currentRecipe.recipeName != "Potion of Hydration")
            CountPotions();
        else
            ResetValues();

        thrownPotion.GetComponent<Collider>().enabled = true;

        var throwSequence = DOTween.Sequence();

        // Scale and throw at the same time
        throwSequence.Append(thrownPotion.transform.DOScale(new Vector3(1f, 1f, 1f), 1f).SetEase(Ease.InOutSine))
            .Join(thrownPotion.transform.DOLocalJump(targetPosition, throwHeight, 1, throwDuration));
    }

    private void CountPotions()
    {
        if (_potionIndex < 2) // Ensure we don't reset too soon
        {
            // visualCounter[_potionIndex].SetActive(false);
            _potionIndex++;
            // cauldronFill.DOLocalMove(cauldronFill.localPosition - new Vector3(0, 0.11f, 0), 0.8f);
        }
        else
            ResetValues();
    }

    // Resets all cauldron values
    private void ResetValues()
    {
        // foreach (var circle in visualCounter)
        // {
        //     circle.SetActive(true);
        // }
        // _cauldronFillMaterial.color = _cauldronFillDefaultColor;
        _possibleRecipeList.Clear();
        _potionIndex = 0;
        _stepIndex = 0;
        _currentStep = null;
        _currentRecipe = null;
        _recipeSteps.Clear();
        // cauldronFill.DOLocalMove(_cauldronStartingPosition, 0.5f);
    }

    // using this to check if the player is in range to stir the cauldron
    private void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        if (_player.isHoldingItem)
            _canInteract = false;
        else
        {
            if (GameManager.Instance.IsInTutorialMode)
            {
                if (TutorialManager.CurrentStep < TutorialStep.StirCauldron)
                    return;
            }

            _canInteract = true;
            Actions.OnShowStir?.Invoke();
        }
    }

    // using this to check if the player has left the range to stir the cauldron
    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        _canInteract = false;
        Actions.OnHideUI?.Invoke();
    }
}