using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class CauldronInteraction : MonoBehaviour
{
    // Reference to the RecipeManager script
    private RecipeManager _recipeManager;

    // Holds all the recipes that can be crafted in this Cauldron
    private RecipeSO[] _craftableRecipes;
    // List of possible recipes
    private List<RecipeSO> _possibleRecipes = new();
    private string _nextStep;
    private string _currentStep;
    // List of possible next steps
    private List<string> _possibleNextSteps = new();
    private string _lastStep;

    // Particles for incorrect step
    private VisualEffect _incorrectStep;

    // Recipe variables
    private RecipeSO _recipe;
    private GameObject _ingredientGo;
    private int _curStepIndex;
    private bool _canInteract;
    private int _potionIndex;

    [Header("Potion Visual Counter")]
    [SerializeField] private GameObject[] visualCounter;

    [Header("Cauldron Fill")]
    [SerializeField] private Transform cauldronFill;
    private Color _cauldronFillDefaultColor;
    private Material _cauldronFillMaterial;
    private Vector3 _cauldronStartingPosition;

    [Header("Potion Insert Spot")]
    [SerializeField] private Transform ingredientInsertPoint;

    [Header("Potion Throwing")]
    [SerializeField] private float throwStrength = 5f; // strength of the throw
    [SerializeField] private float throwHeight = 2f; // max height of the arc
    [SerializeField] private float throwDuration = 1f; // time to reach target

    [Header("Spoon Rotation")]
    [Tooltip("Lower the number the slower it goes")]
    [SerializeField] private float spoonRotationSpeed = 0.6f;
    [SerializeField] private Transform spoon;

    //sound libraries and clips
    [Header("Sounds")]
    [SerializeField] private SFXLibrary addIngredientSounds;
    [SerializeField] private SFXLibrary finishPotionSounds;
    [SerializeField] private SFXLibrary incorrectStepSounds;
    [SerializeField] private SFXLibrary stirSounds;

    private PickupBehaviour _player;


    public void Start()
    {
        // Set the starting position of the cauldron
        _cauldronStartingPosition = cauldronFill.transform.localPosition;

        _cauldronFillMaterial = cauldronFill.GetComponent<MeshRenderer>().material;
        _cauldronFillDefaultColor = _cauldronFillMaterial.color;

        // Get the incorrect step particles
        _incorrectStep = GetComponentInChildren<VisualEffect>();

        // Get the RecipeManager script & set the craftable recipes
        _recipeManager = FindObjectOfType<RecipeManager>();
        _craftableRecipes = _recipeManager.FindAvailableRecipes();
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

    #region Recipe Steps
    public void AddIngredient(PickupObject ingredientHolder, GameObject ingredientObject)
    {
        // Set the ingredient to the current ingredient
        _ingredientGo = ingredientObject;

        // Set the current step to the ingredient's step

        if(_currentStep != null)
        {
            _lastStep = _currentStep;
        }

        _currentStep = null;
        _currentStep = ingredientHolder.recipeIngredient.stepName;

        // grabs the ingredient from the _recipes step that's holding it.
        var ingredientSequence = DOTween.Sequence();

        if (_ingredientGo != null)
        {
            transform.DOScale(1.2f, 0.08f).SetLoops(2, LoopType.Yoyo);
            ingredientSequence.Append(_ingredientGo.transform.DOLocalJump(ingredientInsertPoint.position, 1f, 1, 0.5f).SetEase(Ease.InOutSine))
                         .Join(_ingredientGo.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InOutSine))
                         .OnComplete(SetInactive); // Call SetInactive after both tweens finish
        }

        // Play a sound here
        AudioManager.instance.sfxManager.PlaySFX(SFX_Type.StationSounds, addIngredientSounds.PickAudioClip(), true);

        if (GameManager.Instance.IsInTutorialMode)
        {
            TutorialSteps();
        }
        
        
        // Check if it's the first step in the recipe or not
        if (_curStepIndex == 0)
            StartNewRecipe();
        else
            AdvanceToNextStep();
    }

    private void TutorialSteps()
    {
        if (TutorialManager.tutorialPartCount != 1) return;

        switch (TutorialManager.CurrentStep)
        {
            case TutorialStep.InsertIngredient:  if (_currentStep == "Mushroom") TutorialManager.InsertCorrectIngredient = true;
                break;
            case TutorialStep.FillPotionBottle:  if(_currentStep == "Bottle_Potion")  TutorialManager.FilledPotionBottle = true;
                break;
            default: HandleIncorrectStep(); break;
        }
    }

    private void Stir()
    {
        //play stirring sound
        AudioManager.instance.sfxManager.PlaySFX(SFX_Type.StationSounds, stirSounds.PickAudioClip(), false);

        // If the recipe is null, find the possible recipes
        if (_recipe == null)
        {
            FindPossibleRecipes();
            return;
        }

        // If the recipe is not null, advance to the next step
        AdvanceToNextStep();
    }

    // Handles the incorrect step
    private void HandleIncorrectStep()
    {
        TutorialManager.MadeIncorrectMove = true;
        // Play a sound here
        //AudioManager.Instance.sfxManager.playSFX()
        if (_ingredientGo != null)
            _ingredientGo.GetComponent<Rigidbody>().isKinematic = true;

        _incorrectStep.Play();
        ResetValues();
    }

    // Find the possible recipes based on the current step
    private void FindPossibleRecipes()
    {
        List<RecipeSO> filterRecipes = new();
        _possibleNextSteps.Clear();

        // Loop through all possible recipes
        foreach (var recipes in _possibleRecipes)
        {
            // Ensure the current step index is within the bounds of the recipe steps
            if (_curStepIndex >= recipes.steps.Length)
                continue;

            // Check if the current step matches the recipe step
            if (recipes.steps[_curStepIndex].stepName == _currentStep)
            {
                filterRecipes.Add(recipes);

                // Ensure next step exists
                if (_curStepIndex + 1 < recipes.steps.Length)
                {
                    _nextStep = recipes.steps[_curStepIndex + 1].stepName;
                    _possibleNextSteps.Add(_nextStep);
                }
                else
                {
                    _nextStep = null;
                }

                // Check if it's the final step
                if (recipes.steps[_curStepIndex].stepName == "Bottle_Potion")
                {
                    _recipe = recipes;
                    CompletePotion();
                    return;
                }
            }
        }

        _possibleRecipes = filterRecipes;

        if (_possibleRecipes.Count == 0)
        {
            HandleIncorrectStep();
            return;
        }

        _curStepIndex++;

        // if there's only one possible recipe, set it as the recipe
        if (_possibleRecipes.Count == 1)
        {
            _recipe = _possibleRecipes[0];

            if (_curStepIndex < _recipe.steps.Length)
            {
                _nextStep = _recipe.steps[_curStepIndex].stepName;
            }
        }
    }

    // Start a new recipe based on the current step
    private void StartNewRecipe()
    {
        _possibleRecipes.Clear();
        _possibleNextSteps.Clear();

        //cauldronFillMaterial.color = ingredientGO.GetComponent<PickupObject>().ingredientColor;
        _cauldronFillMaterial.color = Color.Lerp(_cauldronFillMaterial.color, _ingredientGo.GetComponent<PickupObject>().ingredientColor, 3f);

        // Loop through all possible recipes
        foreach (RecipeSO recipe in _craftableRecipes)
        {
            if (recipe.steps.Length > 0 && recipe.steps[0].stepName == _currentStep)
            {
                _possibleRecipes.Add(recipe);
                if (recipe.steps.Length > 1)
                    _possibleNextSteps.Add(recipe.steps[1].stepName);
            }
        }

        // If no possible recipes were found, handle the incorrect step
        if (_possibleRecipes.Count == 0)
        {
            HandleIncorrectStep();
            return;
        }

        _curStepIndex = 1; // Move to next step

        // if there's only one possible recipe, set it as the recipe
        if (_possibleRecipes.Count == 1)
        {
            _recipe = _possibleRecipes[0];

            if (_recipe.steps.Length > 1)
            {
                _nextStep = _recipe.steps[_curStepIndex].stepName;
            }

            if (_recipe.steps[0].stepName == "Bottle_Potion")
            {
                CompletePotion();
            }
        }
    }

    // Advance to the next step in the recipe
    private void AdvanceToNextStep()
    {
        // If the recipe is null, find the possible recipes
        if (_recipe == null)
        {
            FindPossibleRecipes();
            return;
        }

        // Ensure the current step index is within the bounds of the recipe steps
        if (_curStepIndex >= _recipe.steps.Length)
        {
            HandleIncorrectStep();
            return;
        }

        //Debug.Log($"Current Step: {currentStep}, Expected Step: {recipe.steps[curStepIndex].stepName}");

        // If an unexpected ingredient is added, trigger incorrect step handling
        if (_recipe.steps[_curStepIndex].stepName != _currentStep)
        {
            //Debug.LogError($"Incorrect step detected! Current: {currentStep}, Expected: {recipe.steps[curStepIndex].stepName}");
            HandleIncorrectStep();
            return;
        }

        // Check if it's the final step
        if (_recipe.steps[_curStepIndex].stepName == "Bottle_Potion")
        {
            CompletePotion();
            return;
        }

        if (GameManager.Instance.IsInTutorialMode && TutorialManager.CurrentStep == TutorialStep.StirCauldron)
        {
            if (_currentStep == "Stir_C") TutorialManager.StirredCauldronCorrectly = true;
        }

        _curStepIndex++;

        // Set the next step
        if (_curStepIndex < _recipe.steps.Length)
        {
            _nextStep = _recipe.steps[_curStepIndex].stepName;
            //Debug.Log($"Next expected step: {nextStep}");
        }
    }
    #endregion

    #region Potion Completion
    // Complete the potion and throw it
    private void CompletePotion()
    {
        if (_ingredientGo == null) return;

        _ingredientGo.GetComponent<Rigidbody>().isKinematic = false;
        _ingredientGo.GetComponent<PotionOutput>().enabled = true;
        _ingredientGo.GetComponent<PotionOutput>().potionInside = _recipe;
        _ingredientGo.transform.SetParent(null);

        // Instantiate the completed potion prefab
        StartCoroutine(ThrowPotion());
    }

    private IEnumerator ThrowPotion()
    {
        GameObject thrownPotion;
        thrownPotion = _ingredientGo;
        _ingredientGo = null;

        yield return new WaitForSeconds(0.3f);
        
        
        if(thrownPotion.TryGetComponent<PotionOutput>(out var potionOutput))
            potionOutput.SetPotionColor();
        else
            Debug.LogError("PotionOutput script not found on the potion");

        // Play a sound here
        AudioManager.instance.sfxManager.PlaySFX(SFX_Type.StationSounds, finishPotionSounds.PickAudioClip(), true);

        // throw the potion from the cauldron in a random direction
        Vector3 startPosition = ingredientInsertPoint.position;
        Vector3 randomDirection = new Vector3(Random.Range(-1f, 1f), 1, Random.Range(-1f, 1f)).normalized;
        Vector3 targetPosition = startPosition + randomDirection * throwStrength;

        if (_recipe.recipeName != "Potion of Hydration")
            CountPotions();
        else
            ResetValues();

        if(thrownPotion != null)
        {
            thrownPotion.GetComponent<Collider>().enabled = true;

            Sequence throwSequence = DOTween.Sequence();

            // Scale and throw at the same time
            throwSequence.Append(thrownPotion.transform.DOScale(new Vector3(1f, 1f, 1f), 1f).SetEase(Ease.InOutSine))
                         .Join(thrownPotion.transform.DOLocalJump(targetPosition, throwHeight, 1, throwDuration));
        }
    }

    // Count the potions and reset the values
    private void CountPotions()
    {
        //Debug.Log("Potion Counted " + potionIndex);
        if (_potionIndex < 2) // Ensure we don't reset too soon
        {
            visualCounter[_potionIndex].SetActive(false);
            _potionIndex++;
            cauldronFill.DOLocalMove(cauldronFill.localPosition - new Vector3(0, 0.11f, 0), 0.8f);
        }
        else
            ResetValues();
    }
    #endregion

    private void SetInactive()
    {
        if (_currentStep == "Bottle_Potion") return;

        if (_ingredientGo != null)
        {
            DOTween.Kill(_ingredientGo.transform);
            Destroy(_ingredientGo);
        }
    }

    internal void GoblinInteraction()
    {
        _recipe = null;
        cauldronFill.DOLocalMove(cauldronFill.localPosition - new Vector3(0, 0.11f * 2, 0), 1f).SetEase(Ease.InOutSine).OnComplete(ResetValues);
    }

    // Resets all cauldron values
    private void ResetValues()
    {
        foreach (var circle in visualCounter)
        {
            circle.SetActive(true);
        }
        _cauldronFillMaterial.color = _cauldronFillDefaultColor;
        _possibleRecipes.Clear();
        _possibleNextSteps.Clear();
        _potionIndex = 0;
        _curStepIndex = 0;
        _recipe = null;
        _nextStep = null;
        cauldronFill.DOLocalMove(_cauldronStartingPosition, 0.5f);
    }

    #region Stirring
    // Stir the cauldron clockwise
    private void StirClockwise(InputAction.CallbackContext input)
    {
        if (input.performed)
        {
            if (!_canInteract) return;

            spoon.DOLocalRotate(new Vector3(0, 360, 16), spoonRotationSpeed, RotateMode.FastBeyond360);
            if (_lastStep == "Stir_C") return;
            
            _currentStep = "Stir_C";
            Stir();
        }
    }

    // Stir the cauldron counter-clockwise
    private void StirCounterClockwise(InputAction.CallbackContext input)
    {
        if (input.performed)
        {
            if (!_canInteract) return;
            spoon.DOLocalRotate(new Vector3(0, -360, 16), spoonRotationSpeed, RotateMode.FastBeyond360);
            if (_lastStep == "Stir_CC") return;
            
            _currentStep = "Stir_CC";
            Stir();
        }
    }
    #endregion

    // using this to check if the player is in range to stir the cauldron
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (_player.isHoldingItem)
                _canInteract = false;
            else
            {
                if(GameManager.Instance.IsInTutorialMode)
                {
                    if (TutorialManager.CurrentStep < TutorialStep.StirCauldron)
                        return;
                }

                _canInteract = true;
                Actions.OnShowStir?.Invoke();
            }

        }
    }

    // using this to check if the player has left the range to stir the cauldron
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _canInteract = false;
            Actions.OnHideUI?.Invoke();
        }
    }
}