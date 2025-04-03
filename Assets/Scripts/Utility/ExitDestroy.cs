using UnityEngine;

public class ExitDestroy : MonoBehaviour
{
   private void OnTriggerEnter(Collider other)
   {
      Destroy(other.gameObject);
   }
}
