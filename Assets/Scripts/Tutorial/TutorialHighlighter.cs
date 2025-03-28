using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHighlighter : MonoBehaviour
{
   [Header("Highlight")]
    [SerializeField] private Material highlightMaterial;

    [Header("Renderers")]
    [SerializeField] private Renderer recipeBook;
    [SerializeField] private Renderer crate;
    [SerializeField] private Renderer[] cauldrons;
    [SerializeField] private Renderer[] potionBottles;
    [SerializeField] private Renderer servingCounter;
    internal Renderer StirStick;
    internal Renderer LastCauldron;

    private Dictionary<Renderer, Material> _originalMaterials = new();

    private bool _isMaterialChanged;

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

    internal void ChangeMaterial(string rendererName, bool isAddingHighlight)
    {
        switch (rendererName)
        {
            case "recipeBook":
                if(isAddingHighlight) SingleHighlight(recipeBook);
                else RevertSingleHighlight(recipeBook);
                break;
            case "crate":
                if(isAddingHighlight) SingleHighlight(crate);
                else RevertSingleHighlight(crate);
                break;
            case "cauldrons":
                if(isAddingHighlight) MultipleHighlights(cauldrons);
                else RevertMultiHighlights(cauldrons);
                break;
            case "potionFilling":
                if(isAddingHighlight) SingleHighlight(LastCauldron);
                else RevertSingleHighlight(LastCauldron);
                break;
            case "stirStick":
                if(isAddingHighlight) SingleHighlight(StirStick);
                else RevertSingleHighlight(StirStick);
                break;
            case "potionBottles":
                if(isAddingHighlight) MultipleHighlights(potionBottles);
                else RevertMultiHighlights(potionBottles);
                break;
            case "servingCounter":
                if(isAddingHighlight) SingleHighlight(servingCounter);
                else RevertSingleHighlight(servingCounter);
                break;
        }
    }

    #region Highlight
    private void MultipleHighlights(Renderer[] meshRend)
    {
        foreach (var rend in  meshRend)
        {
            SingleHighlight(rend);
        }
    }

    private void SingleHighlight(Renderer rend)
    {
        // store original materials if we haven't already
        if (!_originalMaterials.ContainsKey(rend))
        {
            _originalMaterials[rend] = rend.material; // save original materials
        }
        
        // Create a new array with space for the highlight material
        var newMaterials = new Material[rend.materials.Length + 1];
        
        // copy original materials
        for (var i = 0; i < rend.materials.Length; i++)
        {
            newMaterials[i] = rend.materials[i];
        }
        
        // Add the highlight material at the end
        newMaterials[newMaterials.Length - 1] = highlightMaterial;
        
        // Apply the new material array
        rend.materials = newMaterials;
    }
    #endregion

    #region Remove Highlight
    private void RevertSingleHighlight(Renderer rend)
    {
        // only restore if we have the original materials.
        if(!_originalMaterials.ContainsKey(rend)) return;
        
        rend.materials = new[] { _originalMaterials[rend] }; // restore original materials
        _originalMaterials.Remove(rend); // clean up the stored reference
    }

    private void RevertMultiHighlights(Renderer[] meshRend)
    {
        foreach (var rend in meshRend)
        {
            RevertSingleHighlight(rend);
        }
    }
    #endregion

    private void SetLastUsed(Renderer cauldron, Renderer stirStick)
    {
        LastCauldron = cauldron;
        StirStick = stirStick;
    }
}
