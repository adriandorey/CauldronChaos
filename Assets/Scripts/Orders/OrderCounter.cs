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

    // When a pickup hits the order counter, it will check to see if a customer needs it or not
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Ingredient")) return;

        // Check to see if there's a pickup output on the object
        if (other.TryGetComponent(out PickupItem pickup))
        {
            if ((pickup.IsHeld || pickup.GivenToCustomer ) && pickup.type != ItemType.Potion) return;

            queueManager.CheckCustomerRecipes(pickup);
            return;
        }

        // This takes the starting position of the object, and then "bounces" the object back in a random direction
        var startPos = other.transform.position;
        var randomDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(0.5f, 2f));
        var endPos = startPos + randomDirection * 3f;
        other.transform.DOJump(endPos, 2, 1, 1);
    }
}