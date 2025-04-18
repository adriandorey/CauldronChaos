using UnityEngine;

public class Bucket : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Bucket interacted with");
    }
}
