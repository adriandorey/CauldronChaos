using System.Collections.Generic;
using UnityEngine;

public class SlimeTrail : MonoBehaviour
{
    [Header("Slime Settings")]
    [SerializeField] private GameObject slimePrefab;
    [SerializeField] private float distanceBetweenPoints = 0.5f; // Distance between the last and current slime 
    [SerializeField] private int maxTrailDistance = 20; // Max amount of slime allowed

    private Vector3 _startPosition; // saves starting position of player slime spawn
    private Vector3 _lastSpawnedPosition; // last spawned slime position
    private Queue<GameObject> _slimeTrail = new(); // queue for all slime objects
    private bool _trailActive; // starts / ends the slime trail

    private void Start()
    {
        _startPosition = transform.position;
    }
    
    #region OnEnable / OnDisable / OnDestroy Events
    private void OnEnable()
    {
        Actions.OnStartSlime += StartSlimeTrail;
        Actions.OnResetValues += ResetTrail;
    }

    private void OnDisable()
    {
        Actions.OnStartSlime -= StartSlimeTrail;
        Actions.OnResetValues -= ResetTrail;
    }

    private void OnDestroy()
    {
        Actions.OnStartSlime -= StartSlimeTrail;
        Actions.OnResetValues -= ResetTrail;
    }
    #endregion

    private void Update()
    {
        if (!_trailActive) return;
        
        // makes it so slime doesn't show until the player actually moves.
        if(Vector3.Distance(_startPosition,  transform.position) < 0.2f) return;
        _startPosition = new Vector3(40, 40, 40); // change starting position to a random spot so the above line doesn't occur after the first run.
        
        // Once the last spawned position is far enough away from the last position, it will spawn a new slime prefab.
        if(Vector3.Distance(_lastSpawnedPosition, transform.position) >= distanceBetweenPoints)
        {
            var angle = Random.Range(0, 360);
            var rotation = Quaternion.Euler(0, angle, 0);
            
            
            var slime = Instantiate(slimePrefab, transform.position, rotation);
            _slimeTrail.Enqueue(slime);
            _lastSpawnedPosition = transform.position;
        }

        // Remove the oldest slime if it exceeds the max amount
        if (_slimeTrail.Count > maxTrailDistance)
        {
            Destroy(_slimeTrail.Dequeue());
        }
    }

    // Starts the slime trail challenge
    private void StartSlimeTrail()
    {
        _trailActive = true;
        _startPosition = transform.position;
    }

    // Removes all slime from the scene
    private void ResetTrail()
    {
        _trailActive = false;
        foreach (var slime in _slimeTrail)
        {
            Destroy(slime);
        }
        _slimeTrail.Clear();
    }
}
