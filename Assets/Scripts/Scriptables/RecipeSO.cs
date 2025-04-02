using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Recipe/Recipe")]
public class RecipeSO : ScriptableObject
{
    [Header("Recipe Name")]
    public string recipeName;
    public Sprite recipeNameIcon;

    [Header("Weight")]
    [Tooltip("1 is rare, 10 is more common")]
    [Range(1,10)] public int weight;
    public int sellAmount;

    [Header("Ingredient Steps / Input")]
    public RecipeStepSO[] steps;

    [Header("Potion / Output")]
    public Sprite potionIcon;
    public Color potionColor;
}
