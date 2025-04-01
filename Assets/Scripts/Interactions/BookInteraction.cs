using System;

public class BookInteraction : Interactable
{
    private TutorialManager _tutorialManager;

    private void Start()
    {
        _tutorialManager = FindObjectOfType<TutorialManager>();
    }

    //Function that broadcasts the action to toggle the recipe book being on
    public override void Interact()
    {
        //Debug.Log("BookInteraction");
        Actions.OnToggleRecipeBook?.Invoke();

        if (GameManager.Instance.IsInTutorial() && _tutorialManager.CurrentStep != TutorialStep.Completed)
        {
            _tutorialManager.HandleBookInteraction();
        }

    }

    //Unimplemented crate interact method
    public override void Interact(PickupBehaviour pickup)
    {
        throw new System.NotImplementedException();
    }
}
