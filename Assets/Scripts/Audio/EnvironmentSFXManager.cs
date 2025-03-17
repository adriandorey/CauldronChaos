using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnvironmentSound
{
    icy,
    windy,
    regular
}

public class EnvironmentSFXManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float stingerMaxTime;
    [SerializeField] private float stingerMinTime;
    private float stingerTime;
    private EnvironmentSound targetAmbiance;
    private Coroutine stingersCoroutine = null;

    [Header("SFX Players")]
    [SerializeField] private AudioSFXPlayer ambiancePlayer;
    [SerializeField] private AudioSFXPlayer stingersPlayer;

    [Header("SFX Libraries")]
    [SerializeField] private AudioClip[] environmentAmbiance;
    [SerializeField] private SFXLibrary icyStingers;
    [SerializeField] private SFXLibrary windyStingers;
    [SerializeField] private SFXLibrary regularStingers;

    //Function that starts the environmental ambiance for the shop
    public void StartEnvironmentSFX(EnvironmentSound targetAmbiance)
    {
        //setting for later reference
        this.targetAmbiance = targetAmbiance;

        //play the correct environmental ambiance
        switch (targetAmbiance)
        {
            case EnvironmentSound.icy:
                ambiancePlayer.StartConstantSFX(environmentAmbiance[0]);
                stingersCoroutine = StartCoroutine(PlayStingersRandomly());
                break;

            case EnvironmentSound.windy:
                ambiancePlayer.StartConstantSFX(environmentAmbiance[1]);
                break;

            case EnvironmentSound.regular:
                //stingersCoroutine = StartCoroutine(PlayStingersRandomly());
                break;

            default:
                break;
        }  
    }

    //function that ends the environmental ambiance for the shop
    public void EndEnvironmentSFX()
    {
        ambiancePlayer.StopConstantSFX();

        if (stingersCoroutine != null)
        {
            StopCoroutine(stingersCoroutine);
            stingersCoroutine = null;
        }
    }

    //Function to play an environment stinger based on the currently selected environment
    public void PlayStinger()
    {
        SFXLibrary targetStingers = GetTargetStingers();
        stingersPlayer.PlayOneShot(targetStingers.PickAudioClip());
    }

    //Coroutine that plays stingers at random times
    private IEnumerator PlayStingersRandomly()
    {
        float timer = 0f;
        stingerTime = Random.Range(stingerMinTime, stingerMaxTime);

        //wait for timer to finish
        while (timer < stingerTime)
        {
            yield return null;
            timer += Time.deltaTime;
        }

        SFXLibrary targetStingers = GetTargetStingers();
        stingersPlayer.PlayOneShot(targetStingers.PickAudioClip());

        //recursively call next random stinger 
        stingersCoroutine = StartCoroutine(PlayStingersRandomly());
    }

    //Utility function that returns which SFX Library should be used based on currently selected ambiance
    private SFXLibrary GetTargetStingers()
    {
        SFXLibrary targetStingers;
        switch (targetAmbiance)
        {
            case EnvironmentSound.icy:
                targetStingers = icyStingers;
                break;

            case EnvironmentSound.windy:
                targetStingers = windyStingers;
                break;

            case EnvironmentSound.regular:
                targetStingers = regularStingers;
                break;

            default:
                targetStingers = null;
                break;
        }

        return targetStingers;
    }
}
