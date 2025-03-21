using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class CauldronInteraction : MonoBehaviour
{
    // Reference to the RecipeManager script
    private RecipeManager _recipeManager;

    private RecipeSO[] _availRecipes; // holds all recipes that can be crafted in this cauldron
    private List<RecipeSO> _possibleRecipes = new(); // list of possible recipes
    private string _currentStep; // holds current step
    private string _lastStep; // holds last step done - used to make sure we can stir multiple times

    // Particles for incorrect step
    private VisualEffect _veIncorrectStep;

    // Recipe variables
    private RecipeSO _recipe;
    private GameObject _ingredientAdded;
    private int _stepIndex;
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
    [SerializeField] private float stickRotationSpeed = 0.6f;
    [SerializeField] private Transform stirStick;

    [Header("Model Renderers")]
    [SerializeField] private Renderer modelRenderer;
    [SerializeField] private Renderer stirStickRend;
    
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
        _veIncorrectStep = GetComponentInChildren<VisualEffect>();

        // Get the RecipeManager script & set the craft-able recipes
        _recipeManager = FindObjectOfType<RecipeManager>();
        _availRecipes = _recipeManager.FindAvailableRecipes();
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

    #region Add Ingredients / Stir
    public void AddIngredient(GameObject ingredientObject)
    {
        // Set the ingredient to the current ingredient
        _ingredientAdded = ingredientObject;
        var ingredientStep = _ingredientAdded.GetComponent<PickupObject>().recipeIngredient.stepName;

        // if the current step is something, set the last step to it.
        if(_currentStep != null)
        {
            _lastStep = _currentStep;
        }

        // if the current step is something set it to the last step
        _currentStep = null;
        _currentStep = ingredientStep;
        _lastStep = _currentStep;

        // grabs the ingredient from the _recipes step that's holding it.
        var ingredientSequence = DOTween.Sequence();

        if (_ingredientAdded != null)
        {
            transform.DOScale(1.2f, 0.08f).SetLoops(2, LoopType.Yoyo);
            ingredientSequence.Append(_ingredientAdded.transform.DOLocalJump(ingredientInsertPoint.position, 1f, 1, 0.5f).SetEase(Ease.InOutSine))
                         .Join(_ingredientAdded.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InOutSine))
                         .OnComplete(SetInactive); // Call SetInactive after both tweens finish
        }

        // Play a sound here
        AudioManager.instance.sfxManager.PlaySFX(SFX_Type.StationSounds, addIngredientSounds.PickAudioClip(), true);

        CheckRecipeProgress();
    }
    
    /// <summary> Sets Object inactive once it goes into the cauldron unless it's a bottle </summary>
    private void SetInactive()
    {
        if (_currentStep == "Bottle_Potion") return;
        if (_ingredientAdded == null) return;
        
        DOTween.Kill(_ingredientAdded.transform);
        Destroy(_ingredientAdded);
    }

    private void StirClockwise(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(!_canInteract) return;

            stirStick.DOLocalRotate(new Vector3(0, 360, 16), stickRotationSpeed, RotateMode.FastBeyond360);
            Stir("Stir_C");
        }
    }
    
    private void StirCounterClockwise(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(!_canInteract) return;

            stirStick.DOLocalRotate(new Vector3(0, -360, 16), stickRotationSpeed, RotateMode.FastBeyond360);
            Stir("Stir_CC");
        }
    }
    
    private void Stir(string direction)
    {
        //play stirring sound
        AudioManager.instance.sfxManager.PlaySFX(SFX_Type.StationSounds, stirSounds.PickAudioClip(), false);

       if(_lastStep == direction) return;
       
       _currentStep = direction;
       _lastStep = direction;

       CheckRecipeProgress();
    }
    
    #endregion

    
    #region Recipe Progress

    private void CheckRecipeProgress()
    {
        if (GameManager.Instance.IsInTutorialMode)
            CheckTutorialSteps();
        
        if(_recipe == null)
            FindPossibleRecipes();
        else
            AdvanceToNextStep();
    }
    
    private void FindPossibleRecipes()
    {
        List<RecipeSO> filteredRecipes = new();

        // Determine the list to search through
        List<RecipeSO> searchList = _stepIndex == 0 ? _availRecipes.ToList() : new List<RecipeSO>(_possibleRecipes);

        // Iterate through the search list and find matching recipes
        foreach (var recipe in searchList)
        {
            if (recipe.steps[_stepIndex].stepName == _currentStep)
            {
                filteredRecipes.Add(recipe);
            }
        }
        
        _possibleRecipes.Clear(); // Clear before modifying
        _possibleRecipes.AddRange(filteredRecipes); // Add the stored recipes safely
        
        switch (_possibleRecipes.Count)
        {
            case 0: // If no possible recipes were found, handle incorrect step
                HandleIncorrectStep();
                return;
            case 1: // if one recipe was found check if its on its last step
            {
                _recipe = _possibleRecipes[0];
                Debug.Log($"New Recipe Started: {_recipe.recipeName}");

                if (CheckForLastStep(_recipe))
                {
                    CompletePotion();
                    return;
                }
                break;
            }
        }

        _stepIndex++;
    }

    private void AdvanceToNextStep()
    {
        // If an unexpected ingredient is added, trigger incorrect step handling
        if (_recipe.steps[_stepIndex].stepName != _currentStep)
        {
            // Debug.Log($"Incorrect step detected!");
            HandleIncorrectStep();
            return;
        }

        // Check if it's the final step
        if (CheckForLastStep(_recipe))
        {
            CompletePotion();
            return;
        }

        _stepIndex++;
    }

    private void CheckTutorialSteps()
    {
        if (TutorialManager.TutorialPartCount != 1) return;

        // This will check what step the tutorial is on only in part one
        switch (TutorialManager.CurrentStep)
        {
            case TutorialStep.InsertIngredient when _currentStep == "Mushroom":
                TutorialManager.InsertCorrectIngredient = true;
                TutorialManager.LastCauldronUsed = modelRenderer;
                TutorialManager.StirStickToHighlight = stirStickRend;
                break;
            case TutorialStep.FillPotionBottle when TutorialManager.LastCauldronUsed == modelRenderer:
                TutorialManager.FilledPotionBottle = true;
                break;
            case TutorialStep.StirCauldron when TutorialManager.LastCauldronUsed == modelRenderer && _currentStep == "Stir_C":
                TutorialManager.StirredCauldronCorrectly = true;
                break;
            case TutorialStep.StirCauldron when TutorialManager.LastCauldronUsed != modelRenderer:
                HandleIncorrectStep();
                break;
        }
    }

    private void HandleIncorrectStep()
    {
        // If its in tutorial mode it will show they've made an incorrect move
        if (GameManager.Instance.IsInTutorialMode)
            TutorialManager.MadeIncorrectMove = true;
        
        // audio manager will play sfx
        AudioManager.instance.sfxManager.PlaySFX(SFX_Type.StationSounds, incorrectStepSounds.PickAudioClip(), true);
        
        // if the item added exists it will be turned kinematic
        if(_ingredientAdded != null)
            _ingredientAdded.GetComponent<Rigidbody>().isKinematic = false;
        
        // plays the incorrect step visual effect then resets values
        _veIncorrectStep.Play();
        ResetValues();
    }

    private bool CheckForLastStep(RecipeSO recipe)
    {
        // if the recipe step isn't Bottle_Potion, it's not the last step
        if(recipe.steps[_stepIndex].stepName != "Bottle_Potion") return false;
        
        // if the recipe isn't set yet, set it to this recipe
        if(_recipe == null)
            _recipe = recipe;
        
        return true;
    }
    
    #endregion

  
    #region Potion Completion
    // Complete the potion and throw it
    private void CompletePotion()
    {
        if (_ingredientAdded == null) return;

        _ingredientAdded.GetComponent<Rigidbody>().isKinematic = false;
        _ingredientAdded.GetComponent<PotionOutput>().enabled = true;
        _ingredientAdded.GetComponent<PotionOutput>().potionInside = _recipe;
        _ingredientAdded.transform.SetParent(null);

        // Instantiate the completed potion prefab
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
        
        if(thrownPotion.TryGetComponent<PotionOutput>(out var potionOutput))
            potionOutput.SetPotionColor();

        // Play a sound here
        AudioManager.instance.sfxManager.PlaySFX(SFX_Type.StationSounds, finishPotionSounds.PickAudioClip(), true);

        // throw the potion from the cauldron in a random direction
        var startPosition = ingredientInsertPoint.position;
        var randomDirection = new Vector3(Random.Range(-1f, 1f), 1, Random.Range(-1f, 1f)).normalized;
        var targetPosition = startPosition + randomDirection * throwStrength;

        if (_recipe.recipeName != "Potion of Hydration")
            CountPotions();
        else
            ResetValues();

        thrownPotion.GetComponent<Collider>().enabled = true;

        var throwSequence = DOTween.Sequence();
        // Scale and throw at the same time
        throwSequence.Append(thrownPotion.transform.DOScale(new Vector3(1f, 1f, 1f), 1f).SetEase(Ease.InOutSine))
                         .Join(thrownPotion.transform.DOLocalJump(targetPosition, throwHeight, 1, throwDuration));
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
        _potionIndex = 0;
        _stepIndex = 0;
        _recipe = null;
        cauldronFill.DOLocalMove(_cauldronStartingPosition, 0.5f);
    }

    // using this to check if the player is in range to stir the cauldron
    private void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
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

    // using this to check if the player has left the range to stir the cauldron
    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        _canInteract = false;
        Actions.OnHideUI?.Invoke();
    }
}