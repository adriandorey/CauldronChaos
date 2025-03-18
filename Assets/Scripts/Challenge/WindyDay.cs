using UnityEngine;

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
        foreach (var window in windows)
        {
            window.SetActive(true);
        }
    }

    private void ChangeWindDirection()
    {
        // picks a random direction for the wind to change to
        windDirect = (WindDirection)Random.Range(0, 4);
        
        // Depending on which direction, it will change the transform of the force
        switch (windDirect)
        {
            case WindDirection.GoingLeft: _direction = -transform.right; break;
            case WindDirection.GoingRight: _direction = transform.right; break;
            case WindDirection.TowardsScreen: _direction = -transform.forward; break;
            case WindDirection.AwayFromScreen: _direction = transform.forward; break;
        }
        Debug.Log("Wind Direction Changed to: " + windDirect);

        _windDirectionChange.ResetTimer();
    }

    // Is called to add the wind resistance (force) to the rigidbody of the item/character
    internal Vector3 AddWindResistance()
    {
        return _direction * _currentStrength;
    }
}
