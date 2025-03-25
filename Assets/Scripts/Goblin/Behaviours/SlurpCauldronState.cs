using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SlurpCauldronState : GoblinState
{
    public SlurpCauldronState(GoblinAI goblinAI) : base(goblinAI) { }

    public override void EnterState()
    {
        Debug.Log("Entering SlurpCauldron state");
        var cauldron = _goblinAI.PickACauldron();
        _goblinAI.StartCoroutine(SlurpCauldron(cauldron));
    }

    public override void ExitState()
    {
        _goblinAI.StopCoroutine(SlurpCauldron(null));
    }


    private IEnumerator SlurpCauldron(CauldronInteraction cauldron)
    {
        _goblinAI.SetDestination(cauldron.transform.position);

        while (!_goblinAI.ReachedDestination())
        {
            yield return null;
        } 
        
        _goblinAI.transform.rotation = Quaternion.LookRotation(cauldron.transform.position - _goblinAI.transform.position);
        
        AudioManager.instance.sfxManager.StopConstantSFX(); // stop movement sound

        cauldron.GetComponent<CauldronInteraction>().GoblinInteraction();
        _goblinAI.PlayGoblinSlurping();
        
        yield return new WaitForSeconds(_goblinAI.GetPauseTime()); 
        
        _goblinAI.ChangeState();
    }
}