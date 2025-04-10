using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public enum WindDirection
{
    GoingLeft,
    GoingRight,
    TowardsScreen,
    AwayFromScreen
}

public class WindyDay : MonoBehaviour
{
    public WindDirection windDirect;

    [Header("Wind Settings")]
    [SerializeField] private float strength = 5;
    private float _currentStrength;
    private Vector3 _direction;
    private CustomTimer _windDirectionChange;
    private readonly float _windChangeTime = 30f;
    [SerializeField] private GameObject[] windows;

    [Header("Wind Particles")]
    [SerializeField] private GameObject windLeftParticles;
    [SerializeField] private GameObject windRightParticles;
    [SerializeField] private GameObject windUpParticles;
    [SerializeField] private GameObject windDownParticles;

    [SerializeField] private Wind windLeft;
    [SerializeField] private Wind windRight;
    [SerializeField] private Wind windUp;
    [SerializeField] private Wind windDown;
    
    private Wind _currentWind;
    private Coroutine _windDirectionCoroutine;


    private void Awake()
    {
        _windDirectionChange = new CustomTimer(_windChangeTime, false);
    }

    #region OnEnable / OnDisable / OnDestroy Events
    private void OnEnable()
    {
        Actions.OnStartWindy += StartWind;
        Actions.OnStopWindy += StopWind;
    }

    private void OnDisable()
    {
        Actions.OnStartWindy -= StartWind;
        Actions.OnStopWindy -= StopWind;
    }

    private void OnDestroy()
    {
        Actions.OnStartWindy -= StartWind;
        Actions.OnStopWindy -= StopWind;
    }
    #endregion

    private void Update()
    {
        // if the timer is completed the wind will change direction.
        if(_windDirectionChange.UpdateTimer())
        {
            ChangeWindDirection();
        }
    }

    // Starts the wind challenge. 
    private void StartWind()
    {
        Debug.Log("Starting windy day");
        _currentStrength = strength;
        
        // Removes all the windows from their pane        
        foreach(var window in windows)
        {
            window.SetActive(false);
        }
        // Starts the wind changing direction
        ChangeWindDirection();
    }

    // Stops the wind from changing direction
    private void StopWind()
    {
        Debug.Log("Stopping windy day");
        _currentStrength = 0;
        _windDirectionChange.StopTimer();
        
        if(_windDirectionCoroutine != null)
            StopCoroutine(_windDirectionCoroutine);
        foreach (var window in windows)
        {
            window.SetActive(true);
        }
    }

    private void ChangeWindDirection()
    {
        // picks a random direction for the wind to change to
        windDirect = (WindDirection)Random.Range(0, 4);
        
        if(_windDirectionCoroutine != null)
            StopCoroutine(_windDirectionCoroutine);
        
        // Depending on which direction, it will change the transform of the force
        switch (windDirect)
        {
            case WindDirection.GoingLeft: 
                _direction = -transform.right;
                _windDirectionCoroutine = StartCoroutine(WindDelay(windLeft));
                // SetParticleSystem(windLeftParticles);
                break;
            case WindDirection.GoingRight: 
                _direction = transform.right;
                _windDirectionCoroutine = StartCoroutine(WindDelay(windRight));
                // SetParticleSystem(windRightParticles);
                break;
            case WindDirection.TowardsScreen: 
                _direction = -transform.forward;
                _windDirectionCoroutine = StartCoroutine(WindDelay(windDown));
                // SetParticleSystem(windDownParticles);
                break;
            case WindDirection.AwayFromScreen: 
                _direction = transform.forward;
                _windDirectionCoroutine = StartCoroutine(WindDelay(windUp));
                // SetParticleSystem(windUpParticles);
                break;
        }
        Debug.Log("Wind Direction Changed to: " + windDirect);

        _windDirectionChange.ResetTimer();
    }

    // Is called to add the wind resistance (force) to the rigidbody of the item/character
    internal Vector3 AddWindResistance()
    {
        return _direction * _currentStrength;
    }

    private void SetParticleSystem(GameObject newSystem)
    {
        windUpParticles.SetActive(false);
        windDownParticles.SetActive(false);
        windLeftParticles.SetActive(false);
        windRightParticles.SetActive(false);

        newSystem.SetActive(true);
    }

    private IEnumerator WindDelay(Wind direction)
    {
        if (_currentWind != null)
        {
            _currentWind.loop.Stop();
            _currentWind.wind.Stop();
        }
        
        yield return new WaitForSeconds(0.2f);
        
        direction.loop.Play();
        direction.wind.Play();
        
        _currentWind = direction;
    }
}

[Serializable]
public class Wind
{
    public ParticleSystem loop;
    public ParticleSystem wind;
}