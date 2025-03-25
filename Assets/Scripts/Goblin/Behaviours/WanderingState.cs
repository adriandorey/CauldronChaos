using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class WanderingState : GoblinState
{
    private float _wanderRadius = 10f;
    private Vector3 _currentDestination;
    

    // Constructor calling the base constructor (GoblinState)
    public WanderingState(GoblinAI goblinAI) : base(goblinAI) { }

    public override void EnterState()
    {
        // Start the wandering coroutine
        Debug.Log("Entering WanderingState");
        PickNewDestination();
        _goblinAI.StartCoroutine(WanderingCoroutine());
    }

    private IEnumerator WanderingCoroutine()
    {
        // Simulate wandering time
        while (!_goblinAI.ReachedDestination())
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(_goblinAI.GetPauseTime()); 
        _goblinAI.ChangeState();
    }

    public override void ExitState() { }

    internal void PickNewDestination()
    {
        _currentDestination = RandomNavSphere(_goblinAI.transform.position, _wanderRadius, -1);
        _goblinAI.SetDestination(_currentDestination);
    }

    private Vector3 RandomNavSphere(Vector3 origin, float distance, int layermask)
    {
        var randomDirection = Random.insideUnitSphere * distance;
        randomDirection += origin;

        return NavMesh.SamplePosition(randomDirection, out var navHit, distance, layermask)
            ? navHit.position
            : origin; // If no valid point found, stay put
    }
}
