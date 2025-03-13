using System.Collections;
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

            // Log the values right before checking the condition
                Debug.Log($"Checking condition: {_customersServed} == {_customersSpawned}");
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
                if (InteractedWithBook) NextStep("Step 2: Pick Up Mushroom");
                break;
            case TutorialStep.PickUpMushroom:
                // Highlight crate
                if (PickedUpMushroom) NextStep("Step 3: Insert mushroom into cauldron");
                break;
            case TutorialStep.InsertIngredient:
                // highlight cauldron
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


    private IEnumerator SpawnTwoCustomers()
    {
        queueManager.SpawnSpecificCustomer();
        _customersSpawned++;
        yield return new WaitForSeconds(3);

        queueManager.SpawnSpecificCustomer();
        _customersSpawned++;
        yield return new WaitForSeconds(1);

        CurrentStep = TutorialStep.PickUpPotionBottle;
    }

    private IEnumerator StartDay()
    {
        yield return new WaitForSeconds(6);
        Actions.OnStartDay?.Invoke();
        enabled = false;
    }


    private void RestartTutorial()
    {
        if(_tutorialPartCount != 1) return;
        CurrentStep = TutorialStep.HighlightRecipeBook;
        Debug.Log("You made a mistake! Restarting tutorial.");
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
    }
}