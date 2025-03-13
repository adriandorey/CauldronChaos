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


        transform.DOScale(1.2f, 0.08f).SetLoops(2, LoopType.Yoyo);

        var newIngredient = Instantiate(ingredientPrefab, playerPickup.GetHolderLocation()); //spawning new ingredient
        playerPickup.SetHeldObject(newIngredient.GetComponent<PickupObject>()); //adding manually to player's held slot

        if (crateType == CrateType.Mushroom && GameManager.Instance.IsInTutorialMode && TutorialManager.CurrentStep == TutorialStep.PickUpMushroom)
            TutorialManager.PickedUpMushroom = true;
        
        if(crateType == CrateType.Bottle &&  GameManager.Instance.IsInTutorialMode && TutorialManager.CurrentStep == TutorialStep.PickUpPotionBottle)
            TutorialManager.PickedUpPotionBottle = true;

    }

    // Function that handles the interaction between the goblin and the crate
    internal void GoblinInteraction(Transform goblin)
    {
        transform.DOScale(1.2f, 0.08f).SetLoops(2, LoopType.Yoyo);

        var ingredient = Instantiate(ingredientPrefab, goblin.position, Quaternion.identity);
        ingredient.transform.localScale = Vector3.zero;
        Vector3 randomPosition = new(Random.Range(-1, 1), 0, Random.Range(-1, 1));

        ingredient.transform.DOScale(new Vector3(1f, 1f, 1f), 1f); //DOTween animation for scaling the ingredient
        ingredient.transform.DOJump(goblin.position + randomPosition, 1, 1, 1); //DOTween animation for jumping the ingredient
    }

    // Function that loads a prefab from the resources folder
    private GameObject LoadPrefab(string path)
    {
        var prefab = Resources.Load<GameObject>(path);

        if (prefab != null) return prefab;
        
        Debug.LogError("Prefab not found at path: " + path);
        return null;
    }
   
}
