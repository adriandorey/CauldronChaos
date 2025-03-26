using System.Collections.Generic;
using UnityEngine;
using System;

public class ChallengeManager : MonoBehaviour
{
    [Header("Floor Materials")]
    [SerializeField] private PhysicMaterial slipperyMaterial;
    [SerializeField] private PhysicMaterial defaultMaterial;
    [SerializeField] private Texture icyTexture;
    [SerializeField] private Texture defaultTexture;

    private Dictionary<int, Action> _challengeActions;

    private void Awake()
    {
        _challengeActions = new Dictionary<int, Action>
        {
            {0, () => Debug.Log("No Challenges")},
            {1, () =>
            {
                Actions.OnIceDay?.Invoke(true);
                Actions.OnApplyFloorMaterial?.Invoke(slipperyMaterial, icyTexture);
            }},
            {2, () => Actions.OnStartCauldron?.Invoke()},
            { 3, () => { 
                Actions.OnStartGoblin?.Invoke(true);
                Actions.OnMoveCage?.Invoke(true);
            }},
            { 4, () => Actions.OnStartWindy?.Invoke() },
            { 5, () => Actions.OnStartSlime?.Invoke() }
        };
    }

    #region OnEnable / OnDisable / OnDestroy Events
    private void OnEnable()
    {
        Actions.OnStartChallenge += StartChallenge;
        Actions.OnResetValues += ResetChallenges;
    }

    private void OnDisable()
    {
        Actions.OnStartChallenge -= StartChallenge;
        Actions.OnResetValues -= ResetChallenges;
    }

    private void OnDestroy()
    {
        Actions.OnStartChallenge -= StartChallenge;
        Actions.OnResetValues -= ResetChallenges;
    }
    #endregion

    // Start the challenge based on the challenge type
    private void StartChallenge(int challenge)
    {
        // Is a backup to make sure that all challenges have been reset before starting a new one
        ResetChallenges();

        if (_challengeActions.TryGetValue(challenge, out var action))
        {
            // Debug.Log("Challenge Started: " + challenge);

            //play the environmental audio for correct challenge
            switch (challenge)
            {
                case 1:
                    AudioManager.instance.environmentManager.StartEnvironmentSFX(EnvironmentSound.icy);
                    break;

                case 2:
                    AudioManager.instance.environmentManager.StartEnvironmentSFX(EnvironmentSound.regular);
                    break;

                case 3:
                    AudioManager.instance.environmentManager.StartEnvironmentSFX(EnvironmentSound.regular);
                    break;

                case 4:
                    AudioManager.instance.environmentManager.StartEnvironmentSFX(EnvironmentSound.windy);
                    break;

                case 5:
                    AudioManager.instance.environmentManager.StartEnvironmentSFX(EnvironmentSound.regular);
                    break;
            }
            
            action.Invoke();
        }
        else
        {
            Debug.LogWarning($"Challenge {challenge} not found, resetting challenges.");
            ResetChallenges();
        }
    }

    // Reset all challenges
    private void ResetChallenges()
    {
        Actions.OnApplyFloorMaterial?.Invoke(defaultMaterial, defaultTexture);
        Actions.OnEndCauldron?.Invoke();
        Actions.OnIceDay?.Invoke(false);
        Actions.OnEndGoblin?.Invoke();
        Actions.OnStopWindy?.Invoke();
        Actions.OnMoveCage?.Invoke(false);
    }
}