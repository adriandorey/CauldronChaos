using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CrateHolder : MonoBehaviour, IPickupable
{
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private AssetReference prefabReference;
    [SerializeField] private LayerMask newLayerMask;
    
    private AsyncOperationHandle<GameObject> _prefabLoadOpHandle;
    private PickupInteractionController _playerGivenTo;
    private GameObject _cachedPrefab;
    private GameObject _ingredientSpawned;
    
    private TutorialManager _tutorialManager;

    private void Start()
    {
        _tutorialManager = FindObjectOfType<TutorialManager>();
        
        // caches prefab
        _prefabLoadOpHandle = prefabReference.LoadAssetAsync<GameObject>();
        _prefabLoadOpHandle.Completed += handle =>
        {
            _cachedPrefab = handle.Result;
        };
    }


    private void OnDisable()
    {
        if (_prefabLoadOpHandle.IsValid())
            Addressables.Release(_prefabLoadOpHandle);
    }

    //Function that holds interact functionality for the ingredient crate - Interaction between player and crate
    public void OnPickup(PickupInteractionController controller)
    {
        _playerGivenTo = controller;

        // Checks tutorial steps if its in tutorial
        if (GameManager.Instance.IsInTutorial())
        {
            CheckTutorialStep();
        }
        
        transform.DOScale(1.2f, 0.08f).SetLoops(2, LoopType.Yoyo);
        GetObject();
       
        if (particles != null)
            particles.Play();
    }
   
    // not needed for crate but needed for interface
    public void OnDrop()
    {
        // Not needed for the crate
    }
    
    // not needed for crate but needed for interface
    public Sprite GetSprite()
    {
        // not needed for the crate
        return null;
    }

    // Function that handles the interaction between the goblin and the crate
    internal void GoblinInteraction(GameObject goblin)
    {
        // Bounces the crate when interacted with
        transform.DOScale(1.2f, 0.08f).SetLoops(2, LoopType.Yoyo);

        GetObject();
        
        if(particles != null)
            particles.Play();
        
        // Picks a random position for the item to "be thrown to"
        var forwardDirection = transform.forward;
        var randomSpread = Random.insideUnitCircle * 1.5f;
        
        // calculate final throw direction, forward + spread
        var throwTarget = goblin.transform.position + forwardDirection + new Vector3(randomSpread.x, 0, randomSpread.y);
        
        var ingredientSequence = DOTween.Sequence();

        ingredientSequence
            .Append(_ingredientSpawned.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutSine))
            .Join(_ingredientSpawned.transform.DOLocalJump(throwTarget, 1f, 1, 0.5f).SetEase(Ease.InOutSine));
    }

    // Function that loads a prefab from the resources folder
    private void GetObject()
    {
        if (_cachedPrefab != null)
        {
            _ingredientSpawned = Instantiate(_cachedPrefab);
            _ingredientSpawned.layer = LayerMaskToLayer(newLayerMask);

            // only gives item to the player not the goblin
            if (_playerGivenTo != null && _ingredientSpawned.TryGetComponent(out IPickupable pickupable))
            {
                pickupable.OnPickup(_playerGivenTo);
                _playerGivenTo = null;
            }
        }
        else
        {
            Debug.LogWarning("Ingredient prefab not yet loaded!");
        }
    }

    private void CheckTutorialStep()
    {
        switch (prefabReference.Asset.name)
        {
            case "Mushroom" when  
                _tutorialManager.CurrentStep < TutorialStep.PickUpMushroom:
                return;
            case "Mushroom":
                _tutorialManager.HandleTutorialStep(TutorialStep.PickUpMushroom);
                break;
            case "Bottle_Prefab" when 
                _tutorialManager.CurrentStep < TutorialStep.PickUpPotionBottle:
                return;
            case "Bottle_Prefab":
                _tutorialManager.HandleTutorialStep(TutorialStep.PickUpPotionBottle);
                break;
        }
    }
    
    int LayerMaskToLayer(LayerMask mask)
    {
        return Mathf.RoundToInt(Mathf.Log(mask.value, 2));
    }
}
