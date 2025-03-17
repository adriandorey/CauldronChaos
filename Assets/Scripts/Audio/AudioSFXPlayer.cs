using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SFX_Type
{
    PlayerSounds,
    ShopSounds,
    StationSounds,
    ItemInteraction,
    UISounds,
    GoblinSounds,
    ConstantSounds,
    EnvironmentAmbiance,
    EnvironmentStingers
}

[System.Serializable]
public class AudioSFXPlayer : MonoBehaviour
{
    [SerializeField] public SFX_Type sfx_Type;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public float pitchVariance;
    [SerializeField] private float cooldownTime;
    [SerializeField] private float fadeTime;
    private bool played;  

    //Function to call audio SFX that does not inturrupt playing clip (can overlap)
    public void PlayOneShot(AudioClip audioClip)
    {
        // Debug.Log("CallingPlayOneshot");
        if (!played)
        {
            // Debug.Log(audioSource.volume);
            audioSource.pitch = 1 + Random.Range(-pitchVariance, pitchVariance);
            audioSource.PlayOneShot(audioClip);
            played = true;

            StartCoroutine(AudioCooldown());
        }
    }

    //Function to call audio SFX that inturrupts playing clip
    public void Play(AudioClip audioClip)
    {
        audioSource.pitch = 1 + Random.Range(-pitchVariance, pitchVariance);
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    //Method that starts a constant SFX
    public void StartConstantSFX(AudioClip audioClip)
    {
        //set new clip if starting a new sound
        audioSource.clip = audioClip;
        StartCoroutine(fadeSound(true));
    }

    //Method that stops a constant SFX
    public void StopConstantSFX()
    {
        StartCoroutine(fadeSound(false));
    }

    //Coroutine to put audio on cooldown
    private IEnumerator AudioCooldown()
    {
        yield return new WaitForSecondsRealtime(cooldownTime);
        played = false;
    }

    //Coroutine that handles fading in or out a SFX
    private IEnumerator fadeSound(bool fadeIn)
    {
        float startingValue;
        float endingValue;
        float timer = 0f;

        //determine start numbers
        if (fadeIn)
        {
            startingValue = 0;
            endingValue = 1;

            //initiate SFX start
            audioSource.Play();
        }
        else if (audioSource.isPlaying)
        {
            startingValue= 1;
            endingValue= 0;
        }
        //called to fade when already off
        else
        {
            timer = fadeTime;
            startingValue = 0;
            endingValue = 0f;
        }

        //loop through and fade audio source volume
        while (timer < fadeTime)
        {
            //lerp to correct value and increment timer
            audioSource.volume = Mathf.Lerp(startingValue, endingValue, timer/fadeTime);
            timer += Time.deltaTime;
            yield return null;
        }

        //snap to final value
        audioSource.volume = endingValue;

        if (!fadeIn)
        {
            audioSource.Stop();
        }
    }


}
