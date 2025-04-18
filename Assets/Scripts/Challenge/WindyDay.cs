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
    [SerializeField] private WindSo windSo;
    private float _currentStrength;
    private Vector3 _direction;
    [SerializeField] private GameObject[] windows;
    private float _nextWindChange;

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
    private Coroutine _windRoutine;
  

    #region OnEnable / OnDisable / OnDestroy Events
    private void OnEnable()
    {
        Actions.OnStartWindy += ActivateWind;
        Actions.OnStopWindy += StopWind;
    }

    private void OnDisable()
    {
        Actions.OnStartWindy -= ActivateWind;
        Actions.OnStopWindy -= StopWind;
    }

    private void OnDestroy()
    {
        Actions.OnStartWindy -= ActivateWind;
        Actions.OnStopWindy -= StopWind;
    }
    #endregion

   
    // Starts the wind challenge. 
    private void ActivateWind()
    {
        Debug.Log("Starting windy day");
        _currentStrength = windSo.windStrength;
        
        // Removes all the windows from their pane        
        foreach(var window in windows)
        {
            window.SetActive(false);
        }
        // Starts the wind changing direction
        
        windLeft.wind.Stop();
        windLeft.loop.Stop();
        windRight.wind.Stop();
        windRight.loop.Stop();
        windUp.wind.Stop();
        windUp.loop.Stop();
        windDown.wind.Stop();
        windDown.loop.Stop();
        
        ChangeDirection();
    }

    // Stops the wind from changing direction
    private void StopWind()
    {
        Debug.Log("Stopping windy day");
        _currentStrength = 0;
      
        
        if(_windRoutine != null)
            StopCoroutine(_windRoutine);
        
        foreach (var window in windows)
        {
            window.SetActive(true);
        }
    }

    private void ScheduleWindChange()
    {
        _nextWindChange = Random.Range(windSo.windMinChangeTime, windSo.windMaxChangeTime);
        Invoke(nameof(ChangeDirection),  _nextWindChange);
    }

    private void ChangeDirection()
    {
        // picks a random direction for the wind to change to
        windDirect = Direction();
        
        if(_windRoutine != null)
            StopCoroutine(_windRoutine);
        
        // Depending on which direction, it will change the transform of the force
        switch (windDirect)
        {
            case WindDirection.GoingLeft: 
                _direction = -transform.right;
                _windRoutine = StartCoroutine(WindDelay(windLeft));
                break;
            case WindDirection.GoingRight: 
                _direction = transform.right;
                _windRoutine = StartCoroutine(WindDelay(windRight));
                break;
            case WindDirection.TowardsScreen: 
                _direction = -transform.forward;
                _windRoutine = StartCoroutine(WindDelay(windDown));
                break;
            case WindDirection.AwayFromScreen: 
                _direction = transform.forward;
                _windRoutine = StartCoroutine(WindDelay(windUp));
                break;
        }
        Debug.Log("Wind Direction Changed to: " + windDirect);
    }

    private WindDirection Direction()
    {
        // picks a random direction for the wind to change to
        return (WindDirection)Random.Range(0, 4);
    }

    // Is called to add the wind resistance (force) to the rigidbody of the item/character
    internal Vector3 AddWindResistance()
    {
        return _direction * _currentStrength;
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
        
        ScheduleWindChange();
    }
}

[Serializable]
public class Wind
{
    public ParticleSystem loop;
    public ParticleSystem wind;
}