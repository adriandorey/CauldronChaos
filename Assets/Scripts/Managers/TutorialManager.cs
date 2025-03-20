using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    public static Renderer LastCauldronUsed;
    public static Renderer StirStickToHighlight;

    public static int TutorialPartCount;

    private int _customersSpawned;
    private int _customersServed;

    [Header("Pop Up")]
    [SerializeField] private GameObject tutorialPopup;
    [SerializeField] private TextMeshProUGUI tutorialText;

    [Header("Tutorial Text")]
    [TextArea]
    [SerializeField] private string partOneText;
    [TextArea]
    [SerializeField] private string partTwoText;
    [TextArea]
    [SerializeField] private string partThreeText;

    [Header("Highlight")]
    [SerializeField] private Material highlightMaterial;

    [SerializeField] private Renderer recipeBook;
    [SerializeField] private Renderer[] cauldrons;
    [SerializeField] private Renderer crate;
    [SerializeField] private Renderer[] potionBottles;
    [SerializeField] private Renderer servingCounter;
    
    private Material _previousMaterial;
    private Material _recipeBookMaterial;
    private Material _stirStickMaterial;
    private Material _potionBottleMaterial;
    private Material _servingCounterMaterial;
    private Material _cauldronMaterial;
    private Material _crateMaterial;
    
    private bool _isMaterialChanged = false;
    private Dictionary<Renderer, Material[]> _originalMaterials = new();
    
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
            if (TutorialPartCount == 1)
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
                // Debug.Log("Next part of tutorial should happen");
                ResetFlags();
                NextPartOfTutorial();
            }
        }
    }

    private void StartTutorial()
    {
        Debug.Log("Starting Tutorial");
        TutorialPartCount = 1;
        queueManager.SpawnSpecificCustomer();
        _customersSpawned++;
    }

    private void CheckStepOneCompletion()
    {
        if (ServedPotion && CurrentStep != TutorialStep.ServePotion)
        {
            queueManager.SpawnSpecificCustomer();
            _customersSpawned++;
            ResetFlags();
            return;
        }

        switch (CurrentStep)
        {
            case TutorialStep.HighlightRecipeBook:
                // Highlight book
                if(!_isMaterialChanged) ChangeMaterials(recipeBook);
                
                if (InteractedWithBook)
                {
                    RevertMaterial(recipeBook);
                    tutorialPopup.SetActive(false);
                    NextStep("Step 2: Pick Up Mushroom");
                }
                break;
            case TutorialStep.PickUpMushroom:
                // Highlight crate
                if(!_isMaterialChanged) ChangeMaterials(crate);
                
                if (PickedUpMushroom)
                {
                    RevertMaterial(crate);
                    NextStep("Step 3: Insert mushroom into cauldron");
                }
                break;
            case TutorialStep.InsertIngredient:
                // highlight a cauldron. This should also keep track of what cauldron the mushroom was put into
                if (!_isMaterialChanged)
                {
                    foreach (var cauldron in cauldrons)
                    {
                        ChangeMaterials(cauldron);
                    }
                }

                if (InsertCorrectIngredient)
                {
                    foreach (var cauldron in cauldrons)
                    {
                        RevertMaterial(cauldron);
                    }
                    NextStep("Step 4: Stir the cauldron clockwise");
                }
                break;
            case TutorialStep.StirCauldron:
                if (!_isMaterialChanged)
                {
                    // StirStickToHighlight.transform.DOLocalRotate(new Vector3(15, 0, 16), 1f, RotateMode.Fast)
                    //     .SetLoops(-1, LoopType.Yoyo);
                    ChangeMaterials(StirStickToHighlight);
                }
                
                if (StirredCauldronCorrectly)
                {
                    // StirStickToHighlight.transform.DOKill();
                    RevertMaterial(StirStickToHighlight);
                    NextStep("Step 5: Pick Up Potion Bottle");
                }
                break;
            case TutorialStep.PickUpPotionBottle:
                // highlight potion bottles
                if (!_isMaterialChanged)
                {
                    foreach (var potionBottle in potionBottles)
                    {
                        ChangeMaterials(potionBottle);
                    }
                }

                if (PickedUpPotionBottle)
                {
                    foreach (var potionBottle in potionBottles)
                    {
                        RevertMaterial(potionBottle);
                    }
                    NextStep("Step 6: Fill Potion Bottle");
                }
                break;
            case TutorialStep.FillPotionBottle:
                // highlight cauldron
                if(!_isMaterialChanged) ChangeMaterials(LastCauldronUsed);
                
                if (FilledPotionBottle)
                {
                    RevertMaterial(LastCauldronUsed);
                    NextStep("Step 7: Serve Potion Bottle");
                }
                break;
            case TutorialStep.ServePotion:
                // highlight serving counter
                if(!_isMaterialChanged) ChangeMaterials(servingCounter);
                
                if (ServedPotion)
                {
                    RevertMaterial(servingCounter);
                    NextPartOfTutorial();
                }
                break;
       }
    }

    private void ChangeMaterials(Renderer rend)
    {
        // Makes sure it's not changing the materials every frame and it only happens once
        _isMaterialChanged = true;
        
        // Store the original materials if we haven't already
        if (!_originalMaterials.ContainsKey(rend))
        {
            _originalMaterials[rend] = rend.materials; // Save original materials
        }

        // Create a new array with space for the highlight material
        Material[] newMaterials = new Material[rend.materials.Length + 1];

        // Copy original materials
        for (var i = 0; i < rend.materials.Length; i++)
        {
            newMaterials[i] = rend.materials[i];
        }

        // Add the highlight material at the end
        newMaterials[newMaterials.Length - 1] = highlightMaterial;

        // Apply the new material array
        rend.materials = newMaterials;
    }



    private void RevertMaterial(Renderer rend)
    {
        _isMaterialChanged = false;
        // Only restore if we actually stored the original materials
        if (!_originalMaterials.ContainsKey(rend)) return;
        
        rend.materials = _originalMaterials[rend]; // Restore original materials
        _originalMaterials.Remove(rend); // Clean up the stored reference
    }




    private void NextStep(string step)
    {
        CurrentStep++;
        Debug.Log(step);
    }

    private void NextPartOfTutorial()
    {
        ResetFlags();

        switch (TutorialPartCount)
        {
            // Spawn two customers to show that one cauldron can give multiple potions
            case 1:
                
                StartCoroutine(SpawnTwoCustomers());
                TutorialPartCount++;
                break;
            // This will be unguided the player needs to complete it themselves.
            case 2:
                queueManager.SpawnSpecificCustomer();
                tutorialPopup.SetActive(false);
                _customersSpawned++;
                TutorialPartCount++;
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
    }

    // Waits 5 seconds after the last tutorial part is completed, then opens the store and starts the timer.
    // Also disables the Tutorial Manager.
    private IEnumerator StartDay()
    {
        yield return new WaitForSeconds(5);
        Actions.OnStartDay?.Invoke();
        enabled = false;
    }

    private IEnumerator ShowTutorialText(string text)
    {
        tutorialPopup.SetActive(true);
        tutorialText.text = text;
        yield return new WaitForSeconds(3);
        tutorialPopup.SetActive(false);
    }


    private void RestartTutorial()
    {
        // if the tutorial is restarted, it should also restart the cauldron that's been used if there is one.
        if(TutorialPartCount != 1) return;
        
        tutorialPopup.SetActive(true);
        tutorialText.text = "You made a mistake!\nTry Again!";
        
        CurrentStep = TutorialStep.HighlightRecipeBook;
        ResetFlags();
    }

    internal void ServedCustomer()
    {
        _customersServed++;
    }

    private void ResetFlags()
    {
        ResetAllMaterials();
            
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

    private void ResetAllMaterials()
    {
        foreach (var cauldron in cauldrons)
        {
            RevertMaterial(cauldron);
        }

        foreach (var potionBottle in potionBottles)
        {
            RevertMaterial(potionBottle);
        }

        RevertMaterial(StirStickToHighlight);
        RevertMaterial(crate);
        RevertMaterial(servingCounter);
        RevertMaterial(recipeBook);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!GameManager.Instance.IsInTutorialMode) return;

        if (!other.CompareTag("Customer")) return;
        
        switch (TutorialPartCount)
        {
                case 1:
                    tutorialPopup.SetActive(true);
                    tutorialText.text = partOneText;
                    break;
                case 2:
                    tutorialPopup.SetActive(true);
                    StartCoroutine(ShowTutorialText(partTwoText));
                    tutorialText.text = partTwoText;
                    break;
                case 3:
                    tutorialPopup.SetActive(true);
                    StartCoroutine(ShowTutorialText(partThreeText));
                    break;
        }
    }
}