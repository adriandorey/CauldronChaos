using DG.Tweening;
using UnityEngine;

public class CrateHolder : Interactable
{
    public GameObject ingredientPrefab;
    public enum CrateType { Bottle, Mushroom, RabbitFoot, EyeOfBasilisk, Mandrake, TrollBone };
    public CrateType crateType;
    private Vector3 _originalScale;


    public void Start()
    {
        _originalScale = transform.localScale;

        // If no ingredient prefab is assigned, load the default prefab based on the crate type
        if (ingredientPrefab != null) return;
        
        switch (crateType)
        {
            case CrateType.Bottle: ingredientPrefab = LoadPrefab("Ingredient_Prefabs/Bottle_Prefab"); break;
            case CrateType.Mushroom: ingredientPrefab = LoadPrefab("Ingredient_Prefabs/Mushroom"); break;
            case CrateType.RabbitFoot: ingredientPrefab = LoadPrefab("Ingredient_Prefabs/Rabbit_Foot_Prefab"); break;
            case CrateType.EyeOfBasilisk: ingredientPrefab = LoadPrefab("Ingredient_Prefabs/Eye_of_Basilisk_Prefab"); break;
            case CrateType.Mandrake: ingredientPrefab = LoadPrefab("Ingredient_Prefabs/Mandrake"); break;
            case CrateType.TrollBone: ingredientPrefab = LoadPrefab("Ingredient_Prefabs/Troll_Bone"); break;
        }
    }

    //Unimplemented regular interact function
    public override void Interact()
    {
        throw new System.NotImplementedException();
    }

    //Function that holds interact functionality for the ingedient crate - Interaction between player and crate
    public override void Interact(PickupBehaviour playerPickup)
    {
        //Debug.Log("Create Interact called");

        //Exit function  if no ingredient is selected
        if (ingredientPrefab == null)
        {
            Debug.LogError("No ingredient prefab assigned to " + gameObject.name);
            return;
        }

        if(GameManager.Instance.IsInTutorialMode)
        {
            switch (crateType)
            {
                case CrateType.Mushroom when 
                    TutorialManager.CurrentStep < TutorialStep.PickUpMushroom:
                    return;
                case CrateType.Mushroom:
                    TutorialManager.PickedUpMushroom = true;
                    break;
                case CrateType.Bottle when 
                    TutorialManager.CurrentStep < TutorialStep.PickUpPotionBottle:
                    return;
                case CrateType.Bottle:
                    TutorialManager.PickedUpPotionBottle = true;
                    break;
            }
        }

        transform.DOScale(1.2f, 0.08f).SetLoops(2, LoopType.Yoyo);

        var newIngredient = Instantiate(ingredientPrefab, playerPickup.GetHolderLocation()); //spawning new ingredient
        playerPickup.SetHeldObject(newIngredient.GetComponent<PickupObject>()); //adding manually to player's held slot
    }

    // Function that handles the interaction between the goblin and the crate
    internal void GoblinInteraction(Transform goblin)
    {
        // Bounces the crate when interacted with
        transform.DOScale(1.2f, 0.08f).SetLoops(2, LoopType.Yoyo);

        // Instantiate ingredient & makes it small
        var ingredient = Instantiate(ingredientPrefab, goblin.position, Quaternion.identity);
        ingredient.transform.localScale = Vector3.zero;
        
        // Picks a random position for the item to "be thrown to"
        var forwardDirection = transform.forward;
        var randomSpread = Random.insideUnitCircle * 1.5f;
        
        // calculate final throw direction, forward + spread
        var throwTarget = goblin.position + forwardDirection * 2 + new Vector3(randomSpread.x, 0, randomSpread.y);
        
        var ingredientSequence = DOTween.Sequence();

        ingredientSequence
            .Append(ingredient.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutSine))
            .Join(ingredient.transform.DOLocalJump(throwTarget, 1f, 1, 0.5f).SetEase(Ease.InOutSine));
    }

    // Function that loads a prefab from the resources folder
    private GameObject LoadPrefab(string path)
    {
        var prefab = Resources.Load<GameObject>(path);

        if (prefab != null) return prefab;
        
        Debug.LogError("Prefab not found at path: " + path);
        return null;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 2f); // Draw forward direction
    }

   
}
