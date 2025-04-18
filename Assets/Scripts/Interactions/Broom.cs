using UnityEngine;

public class Broom : MonoBehaviour, IInteractable
{
    [Header("Broom Variables")]
    [SerializeField] private LayerMask interactionLayers;//layers that interact with the broom effect
    [SerializeField] private Transform impactPoint; //point at which the explosion force of the broom will be applied
    [SerializeField] private Animator animator; //animator used to make the broom movement
    [SerializeField] private float moveForce; //force applied to physics objects near the impact point
    [SerializeField] private float forceRadius; //radius in which the force will be applied

    //Inherited method that allows the player to interact with the broom
    public void Interact()
    {
        animator.Play("BroomHit");
    }

    //Method that handles the functionality for the broom swing
    public void SwingFunc()
    {

        Collider[] colliders = Physics.OverlapSphere(impactPoint.position, forceRadius, interactionLayers); //get any colliders in the area
        //Debug.Log(colliders.Length);

        //loop through and apply force to nearby rigid bodies
        foreach (Collider collider in colliders)
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>(); //try to get rigid body off colliders
            if (rb != null) //check if rigid body was found
            {
                rb.AddExplosionForce(moveForce, impactPoint.position, forceRadius); //apply the explosion "cleaning" force
            }
        }
    }
}
