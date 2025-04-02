using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipeBookNavigation : MonoBehaviour
{
    private int _pageNumber;
    private int _totalRecipes;

    internal void Initialize(int totalRecipes)
    {
        _totalRecipes = totalRecipes;
        _pageNumber = 0;
    }

    /// <summary> Gets current page number </summary>
    internal int GetCurrentPage() => _pageNumber;
    
    internal bool CanGoForward() => (_pageNumber + 1) * 2 < _totalRecipes;

    internal bool CanGoBack() => _pageNumber > 0;

    internal void FlipForward() => _pageNumber++;

    internal void FlipBack() => _pageNumber--;
}