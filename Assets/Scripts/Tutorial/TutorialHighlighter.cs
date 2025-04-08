using UnityEngine;

public class TutorialHighlighter : MonoBehaviour
{
    [SerializeField] private HighlightableObject recipeBook;
    [SerializeField] private HighlightableObject crate;
    [SerializeField] private HighlightableObject servingCounter;
    [SerializeField] private HighlightableObject[] cauldrons;
    [SerializeField] private HighlightableObject[] potionShelves;
    
    internal HighlightableObject LastCauldron;
    internal HighlightableObject Stick;
    
    #region Enable / Disable / Destroy
    
    private void OnEnable()
    {
        Actions.LastCauldronUsed += SetLastUsed;
    }
    
    private void OnDisable()
    {
        Actions.LastCauldronUsed -= SetLastUsed;
    }
    
    private void OnDestroy()
    {
        Actions.LastCauldronUsed -= SetLastUsed;
    }
    
    #endregion

    #region Highlighter Methods
    private void HighlightCrate() => crate.SetHighlight();
    private void  HighlightServingCounter() => servingCounter.SetHighlight();
    private void HighlightRecipeBook() => recipeBook.SetHighlight();

    private void HighlightCauldrons()
    {
        foreach (var cauldron in cauldrons)
        {
            cauldron.SetHighlight();
        }
    }

    private void HighlightPotionShelves()
    {
        foreach (var potion in potionShelves)
        {
            potion.SetHighlight();
        }
    }
    
    private void HighlightLastCauldron() => LastCauldron.SetHighlight();
    private void HighlightStirStick() => Stick.SetHighlight();

    internal void HighlightMaterial(string objName)
    {
        switch (objName)
        {
            case "recipeBook": HighlightRecipeBook(); break;
            case "crate":  HighlightCrate(); break;
            case "cauldrons":  HighlightCauldrons(); break;
            case "potionFilling": HighlightLastCauldron(); break;
            case "stirStick": HighlightStirStick();  break;
            case "potionBottles":  HighlightPotionShelves(); break;
            case "servingCounter":   HighlightServingCounter(); break;
        }
    }
    #endregion
    
    #region Revert Highlight Methods
    private void RevertCrate() => crate.RevertHighlight();
    private void  RevertServingCounter() => servingCounter.RevertHighlight();
    private void RevertRecipeBook() => recipeBook.RevertHighlight();

    private void RevertCauldrons()
    {
        foreach (var cauldron in cauldrons)
        {
            cauldron.RevertHighlight();
        }
    }

    private void RevertPotionShelves()
    {
        foreach (var potion in potionShelves)
        {
            potion.RevertHighlight();
        }
    }

    private void RevertLastCauldron()
    {
        if(LastCauldron != null)
            LastCauldron.RevertHighlight();
    }

    private void RevertStirStick()
    {
        if(Stick != null)
            Stick.RevertHighlight();
    }


    internal void RevertMaterial(string objName)
    {
        switch (objName)
        {
            case "recipeBook":RevertRecipeBook(); break;
            case "crate":  RevertCrate(); break;
            case "cauldrons": RevertCauldrons(); break;
            case "potionFilling": RevertLastCauldron(); break;
            case "stirStick": RevertStirStick();  break;
            case "potionBottles":  RevertPotionShelves(); break;
            case "servingCounter":   RevertServingCounter(); break;
        }
    }

    internal void RevertAll()
    {
        RevertRecipeBook();
        RevertCrate();
        RevertStirStick();
        RevertPotionShelves();
        RevertStirStick();
        RevertServingCounter();
    }
    
    #endregion
    
    private void SetLastUsed(HighlightableObject cauldron, HighlightableObject stick)
    {
        LastCauldron = cauldron;
        Stick = stick;
    }
}