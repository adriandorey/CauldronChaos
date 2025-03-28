using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QueueManager queueManager;
    [SerializeField] private TutorialHighlighter highlighter;
    [SerializeField] private TutorialUI tutorialUI;

    internal TutorialStep CurrentStep;
    private int _tutorialPart;

    private int _customersSpawned;
    private int _customersServed;

    [Header("Tutorial Pop up")]
    [Tooltip("This time is in seconds")]
    [SerializeField] private float popUpTime = 5f;

    private CustomTimer _popUpTimer;

    [Header("Tutorial Text")]
    [TextArea]
    [SerializeField] private string partOneText;

    [TextArea]
    [SerializeField] private string partTwoText;

    [TextArea]
    [SerializeField] private string partThreeText;

    private bool _hasShownPopUp;
    private bool _hasInteracted;

    private Dictionary<TutorialStep, (string highlightTarget, TutorialStep nextStep)> _tutorialSteps =
        new()
        {
            { TutorialStep.HighlightRecipeBook, ("recipeBook", TutorialStep.PickUpMushroom) },
            { TutorialStep.PickUpMushroom, ("crate", TutorialStep.InsertIngredient) },
            { TutorialStep.InsertIngredient, ("cauldrons", TutorialStep.StirCauldron) },
            { TutorialStep.StirCauldron, ("stirStick", TutorialStep.PickUpPotionBottle) },
            { TutorialStep.PickUpPotionBottle, ("potionBottles", TutorialStep.FillPotionBottle) },
            { TutorialStep.FillPotionBottle, ("potionFilling", TutorialStep.ServePotion) },
            { TutorialStep.ServePotion, ("servingCounter", TutorialStep.Completed) } // Final step
        };

    private void Start()
    {
        CurrentStep = TutorialStep.HighlightRecipeBook;
        _popUpTimer = new CustomTimer(popUpTime, false);
    }

    private void Update()
    {
        if (!_popUpTimer.isRunning) return;

        if (_popUpTimer.UpdateTimer())
        {
            _popUpTimer.StopTimer();
            tutorialUI.DeactivatePopUp();
        }
    }

    #region Enable / Disable / Destroy

    private void OnEnable()
    {
        Actions.OnStartTutorialDay += StartTutorial;
        Actions.OnResetValues += ResetAll;
        Actions.OnBookInteracted += HandleBookInteraction;
        Actions.OnPotionServed += HandlePotionServing;
    }

    private void OnDisable()
    {
        Actions.OnStartTutorialDay -= StartTutorial;
        Actions.OnResetValues -= ResetAll;
        Actions.OnBookInteracted -= HandleBookInteraction;
        Actions.OnPotionServed -= HandlePotionServing;
    }

    private void OnDestroy()
    {
        Actions.OnStartTutorialDay -= StartTutorial;
        Actions.OnResetValues -= ResetAll;
        Actions.OnBookInteracted -= HandleBookInteraction;
        Actions.OnPotionServed -= HandlePotionServing;
    }

    #endregion

    private void StartTutorial()
    {
        Debug.Log("Starting Tutorial");
        _tutorialPart = 1;
        queueManager.SpawnSpecificCustomer();
        _customersSpawned++;

        highlighter.ChangeMaterial("recipeBook", true);
    }

    
    // Just makes sure the book can be interacted with multiple times
    internal void HandleBookInteraction()
    {
        if (_hasInteracted) return;

        HandleTutorialStep(TutorialStep.HighlightRecipeBook);
        _hasInteracted = true;
    }
    private void HandlePotionServing() => HandleTutorialStep(TutorialStep.ServePotion);


    internal void HandleTutorialStep(TutorialStep step, Renderer rend = null)
    {
        // Debug.Log(step);
        if (_tutorialPart != 1) return;
        if (CurrentStep != step)
        {
            RestartTutorial();
            return;
        }

        // Specific conditions
        if (step == TutorialStep.StirCauldron && highlighter.StirStick != rend ||
            step == TutorialStep.FillPotionBottle && highlighter.LastCauldron != rend)
        {
            RestartTutorial();
            return;
        }

        // Get next step details
        if (!_tutorialSteps.TryGetValue(step, out var stepData)) return;

        // Disable current highlight
        highlighter.ChangeMaterial(stepData.highlightTarget, false);

        if (stepData.nextStep == TutorialStep.Completed)
        {
            CurrentStep = stepData.nextStep;
            return;
        } 

        // Proceed to the next step
        NextStep(stepData.nextStep);
        highlighter.ChangeMaterial(_tutorialSteps[stepData.nextStep].highlightTarget, true);
    }


    private void NextStep(TutorialStep nextStep)
    {
        CurrentStep = nextStep;
    }

    private void NextPartOfTutorial()
    {
        ResetFlags();

        switch (_tutorialPart)
        {
            // Spawn two customers to show that one cauldron can give multiple potions
            case 2:
                StartCoroutine(SpawnTwoCustomers());
                return;
            // This will be unguided the player needs to complete it themselves.
            case 3:
                queueManager.SpawnSpecificCustomer();
                _customersSpawned++;
                return;
            // Completing the previous ones will start the day.
            case 4:
                Debug.Log("Tutorial Completed!");
                StartCoroutine(StartDay());
                GameManager.Instance.IsInTutorialMode = false;
                return;
        }
    }


    // Spawns Two Customers with a pause between the two so they're not spawned on each other.
    private IEnumerator SpawnTwoCustomers()
    {
        queueManager.SpawnSpecificCustomer();
        _customersSpawned++;
        yield return new WaitForSeconds(3);

        queueManager.SpawnSpecificCustomer();
        _customersSpawned++;
        yield return new WaitForSeconds(1);

        // CurrentStep = TutorialStep.PickUpPotionBottle;
    }

    // Waits 5 seconds after the last tutorial part is completed, then opens the store and starts the timer.
    // Also disables the Tutorial Manager.
    private IEnumerator StartDay()
    {
        yield return new WaitForSeconds(5);
        Actions.OnStartDay?.Invoke();
        enabled = false;
    }

    internal void RestartTutorial()
    {
        if (_tutorialPart != 1) return;
        
        Actions.BlowUpCauldron?.Invoke(); // Reset both cauldrons
        highlighter.ResetAllMaterials(); // Reset all the highlighted materials
        _hasInteracted = false; // reset the has interacted - used for the book
        
        tutorialUI.ActivatePopUp("You made a mistake!\nTry again!"); 
        
        CurrentStep = TutorialStep.HighlightRecipeBook;
        highlighter.ChangeMaterial("recipeBook", true);
    }

    internal void ServedCustomer()
    {
        _customersServed++;

        if (_customersServed != _customersSpawned) return;

        if (CurrentStep == TutorialStep.Completed)
        {
            _tutorialPart++;
            NextPartOfTutorial();
        }
        else
        {
            queueManager.SpawnSpecificCustomer();
        }
    }

    private void ResetFlags()
    {
        _customersServed = 0;
        _customersSpawned = 0;
    }


    private void ResetAll()
    {
        CurrentStep = TutorialStep.HighlightRecipeBook;
        _tutorialPart = 1;
        _hasInteracted = false;
        ResetFlags();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!GameManager.Instance.IsInTutorialMode) return;

        if (!other.CompareTag("Customer")) return;

        switch (_tutorialPart)
        {
            case 1:
                tutorialUI.ActivatePopUp(partOneText);
                _popUpTimer.StartTimer();
                break;
            case 2:
                if (_hasShownPopUp) break;
                tutorialUI.ActivatePopUp(partTwoText);
                _popUpTimer.StartTimer();
                _hasShownPopUp = true;
                break;
            case 3:
                tutorialUI.ActivatePopUp(partThreeText);
                _popUpTimer.StartTimer();
                break;
        }
    }
}

public enum TutorialStep
{
    HighlightRecipeBook,
    PickUpMushroom,
    InsertIngredient,
    StirCauldron,
    PickUpPotionBottle,
    FillPotionBottle,
    ServePotion,
    Completed,
}