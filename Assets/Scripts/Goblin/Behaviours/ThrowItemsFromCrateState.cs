using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowItemsFromCrateState : GoblinState
{
    public ThrowItemsFromCrateState(GoblinAI goblinAI) : base(goblinAI) { }

    public override void EnterState()
    {
        Debug.Log("Entering Throw Items from crate state");
        _goblinAI.StartCoroutine(ThrowItems());
    }

    public override void ExitState()
    {
        _goblinAI.StopCoroutine(ThrowItems());
    }


    private IEnumerator ThrowItems()
    {
        var crate = _goblinAI.GetCrate();
        _goblinAI.SetDestination(crate.transform.position);

        var amount = Random.Range(1, 4);

        while (!_goblinAI.ReachedDestination())
        {
            yield return null;
        }

        _goblinAI.PlayRummage();
        
        for (var i = 0; i < amount; i++)
        {
            crate.GoblinInteraction(_goblinAI.Hands().gameObject);
        }
        
        yield return new WaitForSeconds(_goblinAI.GetPauseTime()); 
        
        _goblinAI.ChangeState();
    }
}