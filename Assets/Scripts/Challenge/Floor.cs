using UnityEngine;

public class Floor : MonoBehaviour
{
    #region OnEnable / OnDisable / OnDestroy Events
    private void OnEnable()
    {
        Actions.OnApplyFloorMaterial += ApplyMaterial;
    }

    private void OnDisable()
    {
        Actions.OnApplyFloorMaterial -= ApplyMaterial;
    }

    private void OnDestroy()
    {
        Actions.OnApplyFloorMaterial -= ApplyMaterial;
    }
    #endregion

    /// <summary>
    /// Applies a texture to the floor renderer and sets a physics material to the collider 
    /// </summary>
    private void ApplyMaterial(PhysicMaterial material, Texture texture)
    {
        GetComponent<Collider>().material = material;
        GetComponent<Renderer>().material.mainTexture = texture;
    }
}
