using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookInteraction : Interactable
{
    //Function that broadcasts the action to toggle the recipe book being on
    public override void Interact()
    {
        //Debug.Log("BookInteraction");
        if (GameManager.Instance.IsInTutorialMode)
            TutorialManager.InteractedWithBook = true;

        Actions.OnToggleRecipeBook?.Invoke();
    }

    //Unimplemented crate interact method
    public override void Interact(PickupBehaviour pickup)
    {
        throw new System.NotImplementedException();
    }
}
