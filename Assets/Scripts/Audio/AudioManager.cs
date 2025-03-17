using UnityEngine;
using UnityEngine.Audio;

public enum MixerGroup
{
    Master,
    Music,
    SFX
}

public class AudioManager : MonoBehaviour
{
    [Header("Audio Managers")]
    public SFXManager sfxManager;
    public MusicManager musicManager;
    public EnvironmentSFXManager environmentManager;

    [Header("Audio Mixers")]
    [SerializeField] private AudioMixer masterMixer;

    [SerializeField] [Range(0.0001f, 1f)] private float masterMixerDefaultVolume = 0.5f;
    [SerializeField] [Range(0.0001f, 1f)] private float sfxMixerDefaultVolume = 0.5f;
    [SerializeField] [Range(0.0001f, 1f)] private float musicMixerDefaultVolume = 0.5f;

    private static AudioManager _instance;

    //function that checks if Instance exists and spawns one if it does not
    public static AudioManager instance
    {
        get
        {
            _instance = FindObjectOfType<AudioManager>(); // Try to find an existing AudioManager in the scene

            //check if Instance is null
            if (_instance == null)
            {
                // If no Instance exists, instantiate it
                _instance = Instantiate(Resources.Load("AudioManager") as GameObject).GetComponent<AudioManager>();
                _instance.name = "AudioManager";
            }

            return _instance;
        }
    }

    // Awake is called before the first frame update and before start
    void Awake()
    {
        //check if this is the active Instance
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            //remove copy
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //sets default mixer volumes
        SetVolume(MixerGroup.Master, masterMixerDefaultVolume);
        SetVolume(MixerGroup.Music, musicMixerDefaultVolume);
        SetVolume(MixerGroup.SFX, sfxMixerDefaultVolume);
    }

    //function that sets volume of mixers
    public void SetVolume(MixerGroup target, float volume)
    {
        switch (target)
        {
            case MixerGroup.Master:
                masterMixer.SetFloat("MasterVolume", ConvertToDB(volume)); //replace string with 
                break;

            case MixerGroup.Music:
                masterMixer.SetFloat("MusicVolume", ConvertToDB(volume));
                break;

            case MixerGroup.SFX:
                masterMixer.SetFloat("SFXVolume", ConvertToDB(volume));
                break;
        }
    }

    //function that returns the current volume of target mixer group
    public float GetVolume(MixerGroup target)
    {
        float value = 0f;

        switch (target)
        {
            case MixerGroup.Master:
                masterMixer.GetFloat("MasterVolume", out value);
                break;

            case MixerGroup.Music:
                masterMixer.GetFloat("MusicVolume", out value);
                break;

            case MixerGroup.SFX:
                masterMixer.GetFloat("SFXVolume", out value);
                break;
        }

        value = ConvertFromDB(value);
        return value;
    }

    //utility function for converting linear float to DB
    private float ConvertToDB(float value)
    {
        return Mathf.Log10(value) * 20f;
    }

    //utility function for converting DB to a linear float
    private float ConvertFromDB(float db)
    {
        return Mathf.Pow(10, (db / 20f));
    }
}