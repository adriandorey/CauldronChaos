using System;
using System.Collections;
using TMPro;
using UnityEngine;

public enum TutorialStep
{
    HighlightRecipeBook,
    PickUpMushroom,
    InsertIngredient,
    StirCauldron,
    PickUpPotionBottle,
    FillPotionBottle,
    ServePotion,
    Completed
}


public class TutorialManager : MonoBehaviour
{
    [SerializeField] private QueueManager queueManager;

    public static TutorialStep CurrentStep = TutorialStep.HighlightRecipeBook;
    private bool _stepCompleted;

    public static bool InteractedWithBook;
    public static bool PickedUpMushroom;
    public static bool InsertCorrectIngredient;
    public static bool StirredCauldronCorrectly;
    public static bool PickedUpPotionBottle;
    public static bool FilledPotionBottle;
    public static bool ServedPotion;
    public static bool MadeIncorrectMove;

    private int _tutorialPartCount = 1;
    private int _customersSpawned;
    private int _customersServed;
    
    [SerializeField] private GameObject tutorialPopup;
    [SerializeField] private TextMeshProUGUI tutorialText;

    [Header("Tutorial Text")]
    [TextArea]
    [SerializeField] private string partOneText;
    [TextArea]
    [SerializeField] private string partTwoText;


    private void Start()
    {
        tutorialPopup.SetActive(false);
    }

    #region Enable / Disable / Destroy

    private void OnEnable()
    {
        Actions.OnTutorialDay += StartTutorial;
        Actions.OnResetValues += ResetFlags;
    }

    private void OnDisable()
    {
        Actions.OnTutorialDay -= StartTutorial;
        Actions.OnResetValues -= ResetFlags;
    }

    private void OnDestroy()
    {
        Actions.OnTutorialDay -= StartTutorial;
        Actions.OnResetValues -= ResetFlags;
    }

    #endregion


    private void Update()
    {
        if (GameManager.Instance.IsInTutorialMode)
        {
            if (_tutorialPartCount == 1)
            {
                // if at any point they make an incorrect move, the tutorial will start over.
                if (MadeIncorrectMove)
                {
                    RestartTutorial();
                    return;
                }
                
                CheckStepOneCompletion();
                return;
            }

            // Check if the customers served is equal to customers spawned
            if (_customersServed == _customersSpawned)
            {
                Debug.Log("Next part of tutorial should happen");
                ResetFlags();
                NextPartOfTutorial();
            }
        }
    }

    private void StartTutorial()
    {
        Debug.Log("Starting Tutorial");
        queueManager.SpawnSpecificCustomer();

        StartCoroutine(WaitForCustomer(partOneText));
    }

    private void CheckStepOneCompletion()
    {
        if (ServedPotion && CurrentStep != TutorialStep.ServePotion)
        {
            queueManager.SpawnSpecificCustomer();
            ResetFlags();
            return;
        }
        switch (CurrentStep)
        {
            case TutorialStep.HighlightRecipeBook:
                // Highlight book
                if (InteractedWithBook)
                {
                    tutorialPopup.SetActive(false);
                    NextStep("Step 2: Pick Up Mushroom");
                }
                break;
            case TutorialStep.PickUpMushroom:
                // Highlight crate
                if (PickedUpMushroom) NextStep("Step 3: Insert mushroom into cauldron");
                break;
            case TutorialStep.InsertIngredient:
                // highlight a cauldron. This should also keep track of what cauldron the mushroom was put into
                if (InsertCorrectIngredient) NextStep("Step 4: Stir the cauldron clockwise");
                break;
            case TutorialStep.StirCauldron:
                // do something to show it needs to be stirred
                if (StirredCauldronCorrectly) NextStep("Step 5: Pick Up Potion Bottle");
                break;
            case TutorialStep.PickUpPotionBottle:
                // highlight potion bottles
                if (PickedUpPotionBottle) NextStep("Step 6: Fill Potion Bottle");
                break;
            case TutorialStep.FillPotionBottle:
                // highlight cauldron
                if (FilledPotionBottle) NextStep("Step 7: Serve Potion Bottle");
                break;
            case TutorialStep.ServePotion:
                // highlight serving counter
                if (ServedPotion) NextPartOfTutorial();
                break;
       }
    }



    private void NextStep(string step)
    {
        CurrentStep++;
        Debug.Log(step);
    }

    private void NextPartOfTutorial()
    {
        ResetFlags();

        switch (_tutorialPartCount)
        {
            // Spawn two customers to show that one cauldron can give multiple potions
            case 1:
                
                StartCoroutine(SpawnTwoCustomers());
                _tutorialPartCount++;
                break;
            // This will be unguided the player needs to complete it themselves. - We need something to tell them this?
            case 2:
                queueManager.SpawnSpecificCustomer();
                tutorialPopup.SetActive(false);
                _customersSpawned++;
                _tutorialPartCount++;
                break;
            // Completing the previous ones will start the day.
            case 3:
                Debug.Log("Tutorial Completed!");
                CurrentStep = TutorialStep.Completed;
                
                StartCoroutine(StartDay());
                GameManager.Instance.IsInTutorialMode = false;
                break;
        }
    }

    // This is used to wait for the customer to get in queue then to show the pop up.
    private IEnumerator WaitForCustomer(string insertText)
    {
        yield return new WaitForSeconds(5f);
        tutorialPopup.SetActive(true);
        
        tutorialText.text = insertText;
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

        CurrentStep = TutorialStep.PickUpPotionBottle;
        tutorialText.text =  partTwoText;
        tutorialPopup.SetActive(true);
    }

    // Waits 5 seconds after the last tutorial part is completed, then opens the store and starts the timer.
    // Also disables the Tutorial Manager.
    private IEnumerator StartDay()
    {
        yield return new WaitForSeconds(5);
        Actions.OnStartDay?.Invoke();
        enabled = false;
    }


    private void RestartTutorial()
    {
        // if the tutorial is restarted, it should also restart the cauldron that's been used if there is one.
        if(_tutorialPartCount != 1) return;
        StartCoroutine(WaitForCustomer("You made a mistake!\nTry Again!"));
        CurrentStep = TutorialStep.HighlightRecipeBook;
        ResetFlags();
    }

    internal void ServedCustomer()
    {
        _customersServed++;
    }

    private void ResetFlags()
    {
        _customersServed = 0;
        _customersSpawned = 0;
        InteractedWithBook = false;
        PickedUpMushroom = false;
        InsertCorrectIngredient = false;
        StirredCauldronCorrectly = false;
        PickedUpPotionBottle = false;
        FilledPotionBottle = false;
        ServedPotion = false;
        MadeIncorrectMove = false;
        tutorialPopup.SetActive(false);
    }
}