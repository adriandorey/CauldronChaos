using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

public class CrateHolder : Interactable
{
    public enum CrateType { Bottle, Mushroom, RabbitFoot, EyeOfBasilisk, Mandrake, TrollBone };
    public CrateType crateType;
    [SerializeField] private ParticleSystem particles;
    private TutorialManager _tutorialManager;

    [SerializeField] private AssetReference prefabReference;
    private AsyncOperationHandle<GameObject> _prefabLoadOpHandle;
    private PickupBehaviour _parent;
    
    public void Start()
    {
        _tutorialManager = FindObjectOfType<TutorialManager>();
    }

    private void OnDisable()
    {
        if(_prefabLoadOpHandle.IsValid())
            _prefabLoadOpHandle.Completed -= OnPrefabLoadComplete;

    }

    //Unimplemented regular interact function
    public override void Interact()
    {
        throw new NotImplementedException();
    }

    //Function that holds interact functionality for the ingredient crate - Interaction between player and crate
    public override void Interact(PickupBehaviour playerPickup)
    {
        //Debug.Log("Create Interact called");
        _parent = playerPickup;
        //Exit function  if no ingredient is selected

        if(GameManager.Instance.IsInTutorial())
        {
            CheckTutorialSteps();
        }

        LoadPrefab();
        transform.DOScale(1.2f, 0.08f).SetLoops(2, LoopType.Yoyo);
       
        if (particles != null)
            particles.Play();
        
    }

    // Function that handles the interaction between the goblin and the crate
    internal void GoblinInteraction(Transform goblin)
    {
        // Bounces the crate when interacted with
        transform.DOScale(1.2f, 0.08f).SetLoops(2, LoopType.Yoyo);

        // Instantiate ingredient & makes it small
        // var ingredient = Instantiate(_ingredientPrefab, goblin.position, Quaternion.identity);
        // ingredient.transform.localScale = Vector3.zero;

        if(particles != null)
            particles.Play();
        
        // Picks a random position for the item to "be thrown to"
        var forwardDirection = transform.forward;
        var randomSpread = Random.insideUnitCircle * 1.5f;
        
        // calculate final throw direction, forward + spread
        var throwTarget = goblin.position + forwardDirection + new Vector3(randomSpread.x, 0, randomSpread.y);
        
        var ingredientSequence = DOTween.Sequence();

        // ingredientSequence
        //     .Append(ingredient.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutSine))
        //     .Join(ingredient.transform.DOLocalJump(throwTarget, 1f, 1, 0.5f).SetEase(Ease.InOutSine));
    }

    // Function that loads a prefab from the resources folder
    private void LoadPrefab()
    {
        if (!prefabReference.RuntimeKeyIsValid())
        {
            Debug.LogError("Prefab reference is invalid");
            return;
        }
        
        _prefabLoadOpHandle = prefabReference.LoadAssetAsync<GameObject>();
        _prefabLoadOpHandle.Completed += OnPrefabLoadComplete;
    }


    private void OnPrefabLoadComplete(AsyncOperationHandle<GameObject> asyncOperationHandle)
    {
        if (asyncOperationHandle.Status != AsyncOperationStatus.Succeeded) return;
        
        var newIngredient = Instantiate(asyncOperationHandle.Result, _parent.GetHolderLocation());
        _parent.SetHeldObject(newIngredient.GetComponent<PickupObject>()); //adding manually to player's held slot
        
        Addressables.Release(asyncOperationHandle);
    }
    
    private void CheckTutorialSteps()
    {
        if (_tutorialManager.CurrentStep != TutorialStep.Completed)
        {
            switch (crateType)
            {
                case CrateType.Mushroom
                    when _tutorialManager.CurrentStep < TutorialStep.PickUpMushroom:
                    return;
                case CrateType.Mushroom:
                    _tutorialManager.HandleTutorialStep(TutorialStep.PickUpMushroom);
                    // Actions.OnMushroomPickedUp?.Invoke(); 
                    break;
                case CrateType.Bottle
                    when _tutorialManager.CurrentStep < TutorialStep.PickUpPotionBottle:
                    return;
                case CrateType.Bottle:
                    _tutorialManager.HandleTutorialStep(TutorialStep.PickUpPotionBottle);
                    // Actions.OnPotionBottlePickedUp?.Invoke(); 
                    break;
            }
        }
    }
}
