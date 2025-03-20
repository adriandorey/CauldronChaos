using DG.Tweening;
using UnityEngine;

public class CounterTrigger : MonoBehaviour
{
    private PickupObject _pickup;
    [SerializeField] private float ejectPower = 2f;
    [SerializeField] private float jumpPower = 1f;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private bool isCorner;

    //Method called on collider entering the trigger volume
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Object added to counter");
        if(!other.gameObject.CompareTag("Ingredient")) return;
        
        if (_pickup == null && !isCorner)
        {
            _pickup = other.gameObject.GetComponent<PickupObject>();
            _pickup.transform.position = transform.position;
            return;
            
        }
            
        var ejectDirection = -transform.forward * ejectPower;
        other.transform.DOJump(transform.position + ejectDirection, jumpPower, 1, duration).SetEase(Ease.OutQuad);
    }

    //Method called on collider leaving the trigger volume
    private void OnTriggerExit(Collider other)
    {
        if(!other.gameObject.CompareTag("Ingredient")) return;
        if(other == _pickup.GetComponent<Collider>())
            _pickup = null;
    }
}
