using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.UI;

public class RecipeBookUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RecipeManager recipeManager;
    [SerializeField] private RecipeBookNavigation navigator;
    [SerializeField] private RecipeBookRenderer bookUi;

    [Header("Book")]
    [SerializeField] private GameObject recipeBook;
    [SerializeField] private GameObject closeButton;
    
    [Header("Recipe Book SFX")]
    [SerializeField] private SFXLibrary pageFlipSounds;
    private List<RecipeSO> _availableRecipes;

    private void Start()
    {
        _availableRecipes = recipeManager.GetAvailableRecipes();
        navigator.Initialize(_availableRecipes.Count);
    }
    
    private void Update()
    {
        if (!recipeBook.activeSelf) return;
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleRecipeBook();
        }
    }

    #region OnEnable / OnDisable / OnDestroy Events

    private void OnEnable()
    {
        InputManager.NextPageAction += NextPageAction;
        InputManager.PreviousPageAction += PreviousPageAction;
        Actions.OnToggleRecipeBook += ToggleRecipeBook;
    }

    private void OnDisable()
    {
        InputManager.NextPageAction -= NextPageAction;
        InputManager.PreviousPageAction -= PreviousPageAction;
        Actions.OnToggleRecipeBook -= ToggleRecipeBook;
    }

    private void OnDestroy()
    {
        InputManager.NextPageAction -= NextPageAction;
        InputManager.PreviousPageAction -= PreviousPageAction;
        Actions.OnToggleRecipeBook -= ToggleRecipeBook;
    }

    #endregion

    #region Book Navigation

    private void NextPageAction(InputAction.CallbackContext context)
    {
        if(!context.performed) return;
        NextPage();     
    }

    private void PreviousPageAction(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        PreviousPage();
    }

    public void NextPage()
    {
        if(!navigator.CanGoForward()) return;

        navigator.FlipForward();
        PlayFlipSfx();
        bookUi.RenderPage(navigator.GetCurrentPage(), _availableRecipes);
    }


    public void PreviousPage()
    {
        if (!navigator.CanGoBack()) return;
        
        navigator.FlipBack();
        PlayFlipSfx();
        bookUi.RenderPage(navigator.GetCurrentPage(), _availableRecipes);
    }
    

    #endregion

    private void ToggleRecipeBook()
    {
        if (recipeBook.activeSelf) DeactivateRecipeBook();
        else ActivateRecipeBook();
    }

    private void ActivateRecipeBook()
    {
        Time.timeScale = 0;
        recipeBook.SetActive(true);
        bookUi.RenderPage(navigator.GetCurrentPage(), _availableRecipes);
        Actions.OnSetUiLocation?.Invoke(Page.RecipeBook);
        Actions.OnSelectRecipeButton?.Invoke(closeButton);
        InputManager.OnRecipeBookInputs?.Invoke();
    }

    private void DeactivateRecipeBook()
    {
        Time.timeScale = 1;
        recipeBook.SetActive(false);
        Actions.OnSetUiLocation?.Invoke(Page.Gameplay);
        InputManager.OnGameplayInputs?.Invoke();
    }

    private void PlayFlipSfx()
    {
        AudioManager.instance.sfxManager.PlayMenuSFX(pageFlipSounds.PickAudioClip());
    }
}

[Serializable]
public class RecipeUI
{
    public GameObject recipeObject;
    public TextMeshProUGUI recipeName;
    public Image potionIcon;
    public TextMeshProUGUI potionPrice;
    public RecipeVisuals[] stepVisual;
}

[Serializable]
public struct RecipeVisuals
{
    public GameObject mainObject;
    public TextMeshProUGUI stirButtonText;
    public Image stirIcon;
}

[Serializable]
public struct BookNavigation
{
    public GameObject button;
    public Image icon;
    public TextMeshProUGUI text;
}