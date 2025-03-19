using UnityEditor;

[CustomEditor(typeof(RecipeStepSO))]
public class RecipeStepEditor : Editor
{
    SerializedProperty action;
    SerializedProperty ingredient; // this will need to be renamed
    SerializedProperty nameOfStep;
    SerializedProperty stepSprite;

    private void OnEnable()
    {
        // Cache serialized properties
        nameOfStep = serializedObject.FindProperty("stepName");
        action = serializedObject.FindProperty("action");
        ingredient = serializedObject.FindProperty("ingredient");
        stepSprite = serializedObject.FindProperty("stepSprite");
    }

    public override void OnInspectorGUI()
    {
        // Update serialized object
        serializedObject.Update();

        // Show step ingredient and allow selection
        EditorGUILayout.PropertyField(action);
        EditorGUILayout.PropertyField(nameOfStep);

        // Conditionally show other fields based on step ingredient
        if (action.enumValueIndex == (int)RecipeStepSO.ActionType.AddIngredient)
        {
            // Show ingredient ingredient
            EditorGUILayout.PropertyField(ingredient);
            EditorGUILayout.PropertyField(stepSprite);
        }
        else if (action.enumValueIndex == (int)RecipeStepSO.ActionType.Stir)
        {
            // Show stir amount
            EditorGUILayout.PropertyField(stepSprite);
        }

        // Apply changes to the serialized object
        serializedObject.ApplyModifiedProperties();
    }
}