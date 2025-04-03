using UnityEngine;

public class HighlightableObject : MonoBehaviour
{
   [SerializeField] private Material highlightMaterial;
   [SerializeField] private ParticleSystem highlightParticles;
   
   private Material _originalMaterial;
   private Renderer _renderer;

   private void Awake()
   {
      _renderer = GetComponent<Renderer>();
      if(_renderer != null)
         _originalMaterial = _renderer.material;
   }

   internal void SetHighlight()
   {
      // Create a new array with space for the highlight material
      var newMaterials = new Material[_renderer.materials.Length + 1];

      // copy original materials
      for (var i = 0; i < _renderer.materials.Length; i++)
      {
         newMaterials[i] = _renderer.materials[i];
      }

      // Add the highlight material at the end
      newMaterials[newMaterials.Length - 1] = highlightMaterial;

      // Apply the new material array
      _renderer.materials = newMaterials;
      
      highlightParticles.Play();
   }

   internal void RevertHighlight()
   {
      if (_renderer == null) return;

      // Restore only the original materials, removing the highlight
      _renderer.materials = new Material[] { _originalMaterial };
      
      highlightParticles.Stop();
   }
}
