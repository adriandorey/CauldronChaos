using UnityEngine;

[CreateAssetMenu(fileName = "New Step", menuName = "Recipe/Recipe Step")]
public class RecipeStepSO : ScriptableObject
{
    public enum ActionType { Nothing, AddIngredient, Stir }
    public ActionType action;

    public string stepName;

    public Sprite stepSprite;

    public Color ingredientColour;
}
