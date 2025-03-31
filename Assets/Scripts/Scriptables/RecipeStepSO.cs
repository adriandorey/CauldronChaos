using UnityEngine;

[CreateAssetMenu(fileName = "New Step", menuName = "Recipe/Recipe Step")]
public class RecipeStepSO : ScriptableObject
{
    public enum ActionType { Nothing, AddIngredient, Stir }
    public ActionType action;

    public string stepName;

    // Ingredient
    public enum Ingredient { Nothing, Bottle, Mushroom, Eye_of_Basilisk, Rabbit_Foot, Troll_Bone , Mandrake_Root };
    public Ingredient ingredient;
   
    public Sprite stepSprite;

    public Color ingredientColour;
}
