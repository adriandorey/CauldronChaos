using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class OrderCounter : MonoBehaviour
{
    [SerializeField] private QueueManager queueManager;

    private void OnDestroy()
    {
        transform.DOKill();
    }

    // When a potion hits the order counter, it will check to see if a customer needs it or not
    private void OnTriggerEnter(Collider other)
    {
        // Check to see if there's a potion output on the object
        if (other.TryGetComponent(out PotionOutput potion))
        {
            if (potion.GivenToCustomer) return;

            queueManager.CheckCustomerRecipes(potion);
            return;
        }

        // if there isn't check to see if there's a pickup object on it & check to see if it's being held
        if (!other.TryGetComponent(out PickupObject pickup)) return;
        if (pickup.isHeld) return;

        // This takes the starting position of the object, and then "bounces" the object back in a random direction
        var startPos = other.transform.position;
        var randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(0.5f, 2f));
        var endPos = startPos + randomDirection * 3f;
        other.transform.DOJump(endPos, 2, 1, 1);
    }
}