using UnityEngine;

public class BookInteraction : MonoBehaviour,  IInteractable
{
    private TutorialManager _tutorialManager;

    private void Start()
    {
        _tutorialManager = FindObjectOfType<TutorialManager>();
    }

    //Function that broadcasts the action to toggle the recipe book being on
    public void Interact()
    {
        //Debug.Log("BookInteraction");
        Actions.OnToggleRecipeBook?.Invoke();

        if (GameManager.Instance.IsInTutorial() && _tutorialManager.CurrentStep != TutorialStep.Completed)
        {
            _tutorialManager.HandleBookInteraction();
        }
    }
}
