using System.Collections;
using UnityEngine;

public class ScareCustomerState : GoblinState
{
    private GameObject _customerToScare;
    
    public ScareCustomerState(GoblinAI goblinAI) : base(goblinAI) { }

    public override void EnterState()
    {
        Debug.Log("Entering Scare state");
        _customerToScare = _goblinAI.GetCustomerToScare();
        
        if(_customerToScare != null)
            _goblinAI.StartCoroutine(ScareCustomer());
        else
        {
            _goblinAI.ChangeState();    
        }
    }

    public override void ExitState()
    {
        _goblinAI.StopCoroutine(ScareCustomer());
    }

    private IEnumerator ScareCustomer()
    {
        var lastKnownPosition = _customerToScare.transform.position;
        
        // Set goblins destination to the last known position of the customer
        _goblinAI.SetDestination(lastKnownPosition);

        while (!_goblinAI.ReachedDestination())
        {
            // Refresh customers list and check if the target is still valid
            if (_customerToScare == null || !_customerToScare.GetComponent<CustomerMovement>().HasJoinedQueue())
            {
                AudioManager.instance.sfxManager.StopConstantSFX(); // Stop movement sound
                _goblinAI.ChangeState();
                yield break;
            }
            
            // Update destination if the customer moved
            if (_customerToScare.transform.position != lastKnownPosition)
            {
                lastKnownPosition = _customerToScare.transform.position;
                _goblinAI.SetDestination(lastKnownPosition);
            }
            yield return null;
        }

        if (_customerToScare != null)
        {
            AudioManager.instance.sfxManager.StopConstantSFX();
            
            _goblinAI.ScareCustomer(_customerToScare);
            
            yield return new WaitForSeconds(2f); // Simulated scare time
        }
        
        yield return new WaitForSeconds(_goblinAI.GetPauseTime()); 
        _goblinAI.ChangeState();
    }
}