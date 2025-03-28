using UnityEngine;
using System;

public static class Actions
{
    // Interaction Actions
    /// <summary> Used to toggle recipe book on and off </summary>
    public static Action OnToggleRecipeBook;

    // Shop Actions
    #region Shop Actions

    /// <summary> Starts Shop Day, this is meant to start anything that has to run when the store opens.  </summary>
    public static Action OnStartDay;

    /// <summary> Ends the shop day. Anything that needs to stop running when this happens will happen.  </summary>
    public static Action OnEndDay;

    /// <summary> When a customer is served this will trigger any other script that needs to know about it </summary>
    public static Action<int> OnCustomerServed;

    /// <summary> Sets the day for everything that needs to know what the day is </summary>
    public static Action<int> OnSetDay;

    #endregion

    // Game Manager Actions
    /// <summary> Changes the state of the game manager </summary>
    public static Action<string> OnStateChange;

    /// <summary> Changes the Ui based on the state </summary>
    public static Action<GameState> OnChangeUi; // used for UI changes

    // Gameplay Actions
    #region Gameplay Actions

    /// <summary> Updates the Level Select buttons</summary>
    public static Action UpdateLevelButtons;

    /// <summary> Sets the unlocked days</summary>
    public static Action<int> OnSetUnlockedDays;

    /// <summary> Sets the score based on the day</summary>
    public static Action<int[]> OnSetScore;

    /// <summary> This is used to reset any values that need to be reset at the end</summary>
    public static Action OnResetValues;

    #endregion

    // Save Manager Actions
    #region Save Manager Actions

    /// <summary> Tells listeners if a save exists or not </summary>
    public static Action<bool> OnSaveExist;

    /// <summary> This is used to save the day, score and if the next day should be unlocked </summary>
    public static Action<int, int, bool> OnSaveDay;

    /// <summary> This tells any listener that the save should be deleted. </summary>
    public static Action OnDeleteSaveFile;

    #endregion

    // Actions for First Select / Above Player UI
    #region First Select Actions

    /// <summary> Selects the Recipe Button </summary>
    public static Action<GameObject> OnSelectRecipeButton;

    /// <summary> Sets UI location for the first selection </summary>
    public static Action<Page> OnSetUiLocation;

    public static Action OnShowInteraction;
    public static Action OnShowStir;
    public static Action OnShowPickup;
    public static Action OnHideUI;

    #endregion

    // Actions for Menu
    #region Menu Actions

    /// <summary> Used to open settings </summary>
    public static Action OnOpenSettingsAction;

    /// <summary> Used to activate how to play based on if it's a loading screen or not </summary>
    public static Action<bool> OnActivateHowToPlay;

    /// <summary> Deactivates how to play </summary>
    public static Action OnDeactivateHowToPlay;

    /// <summary> This is used for the camera dolly to say where they've reached</summary>
    public static Action<string> ReachedWaypoint;

    #endregion

    // Challenge Actions
    #region Challenge Actions

    /// <summary> Used to start a challenge based on the day </summary>
    public static Action<int> OnStartChallenge;

    /// <summary> Used for challenge one to apply floor physics material and texture </summary>
    public static Action<PhysicMaterial, Texture> OnApplyFloorMaterial;

    /// <summary> Used for challenge 1 to tell any listener that it's an ice day or not</summary>
    public static Action<bool> OnIceDay;

    /// <summary> Used for challenge 2, tells the cauldrons to start moving</summary>
    public static Action OnStartCauldron;

    /// <summary> Used for challenge 2, tells the cauldrons to stop moving </summary>
    public static Action OnEndCauldron;

    /// <summary> Used to top the goblin cage or reset it </summary>
    public static Action<bool> OnMoveCage;

    /// <summary> Used to start the goblin, telling it if it's the first day or not </summary>
    public static Action<bool> OnStartGoblin;

    /// <summary> Tells the goblin to stop </summary>
    public static Action OnEndGoblin;

    /// <summary> Starts the windy day </summary>
    public static Action OnStartWindy;

    /// <summary> Stops the windy day </summary>
    public static Action OnStopWindy;

    /// <summary> Starts the slime </summary>
    public static Action OnStartSlime;

    #endregion
    
    // Tutorial Actions

    #region Tutorial Actions
    public static Action OnStartTutorialDay;
    public static Action OnBookInteracted;
    public static Action OnMushroomPickedUp;
    public static Action OnIngredientInserted;
    public static Action <Renderer> OnCauldronStirred;
    public static Action OnPotionBottlePickedUp;
    public static Action OnPotionServed;
    public static Action <Renderer> OnPotionFilled;
    public static Action OnMadeIncorrectMove;

    // This will be used to set the last cauldron used and the stir stick that belongs to it
    public static Action<Renderer, Renderer> LastCauldronUsed; 
    #endregion
}