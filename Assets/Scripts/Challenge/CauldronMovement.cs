using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CauldronMovement : MonoBehaviour
{
    // Agent Variables
    [Header("Movement Settings")]
    [SerializeField] private CauldronMovementSo movementSo;
    
    // Model Transform for movement
    [Header("Other Settings")]
    [SerializeField] private Transform cauldronModel;
    [SerializeField] private GameObject otherCauldron; // saves a reference to the other cauldron in the scene

    private NavMeshAgent _agent;
    private bool _isMoving;
    private float _nextMoveDelay;
    private Coroutine _movement;
    private Vector3 _startPosition;
    private Vector3 _currentDestination;


    private void Start()
    {
        _startPosition = cauldronModel.transform.localPosition;
        _agent = GetComponent<NavMeshAgent>();
    }

    // These are used to call the functions for turning the cauldrons on and off. On Destroy is only as a back-up just in case on disable doesn't do what it should.
    #region OnEnable / OnDisable / OnDestroy Events
    private void OnEnable()
    {
        Actions.OnStartCauldron += ScheduleNextMove;
        Actions.OnEndCauldron += DisableMovement;
    }

    private void OnDisable()
    {
        Actions.OnStartCauldron -= ScheduleNextMove;
        Actions.OnEndCauldron -= DisableMovement;
    }

    private void OnDestroy()
    {
        Actions.OnStartCauldron -= ScheduleNextMove;
        Actions.OnEndCauldron -= DisableMovement;
    }

    #endregion

    // Start the movement of the cauldron. Picks a random time between two values then starts a timer.
    private void ScheduleNextMove()
    {
       _nextMoveDelay = Random.Range(movementSo.minMovementTime, movementSo.maxMovementTime);
       Invoke(nameof(AttemptMove), _nextMoveDelay);
    }

    private void AttemptMove()
    {
        if (!TryGetValidDestination(out _currentDestination))
        {
            ScheduleNextMove(); // Try again later if no valid point
            return;
        }
        
        _movement = StartCoroutine(MoveCauldronRoutine(_currentDestination));
    }

    // Coroutine to move the cauldron
    private IEnumerator MoveCauldronRoutine(Vector3 destination)
    {
        // Animate Lift
        yield return cauldronModel.DOLocalMoveY(cauldronModel.localPosition.y + movementSo.liftAmount, 0.5f)
            .SetEase(Ease.OutSine)
            .WaitForCompletion();
        
        // Move to Destination
        _agent.SetDestination(destination);
        while (!_agent.pathPending && _agent.remainingDistance > _agent.stoppingDistance)
        {
            yield return null;
        }        
        
        // Animate Settle
        yield return cauldronModel.DOLocalMove(_startPosition, 0.5f)
            .SetEase(Ease.InSine)
            .WaitForCompletion();
        
        ScheduleNextMove();
    }

    private bool TryGetValidDestination(out Vector3 result)
    {
        for (var i = 0; i < 10; i++)
        {
            var randomPoint = transform.position + Random.insideUnitSphere * movementSo.wanderRadius;

            if (!NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1f, NavMesh.AllAreas)) continue;
            
            if (!(Vector3.Distance(hit.position, otherCauldron.transform.position) >=
                  movementSo.cauldronMinDistance)) continue;
            
            result = hit.position;
            return true;
        }
        result = Vector3.zero;
        return false;
    }
    
    private void DisableMovement()
    {
        if(_agent == null) return;
        
        CancelInvoke();
        if(_movement != null)
            StopCoroutine(_movement);
    }
}
