using UnityEngine;

public class DestroyOnEnter : MonoBehaviour
{
    public GameObject pianist;
   void OnTriggerEnter2D(Collider2D other)
   {
      if (other.CompareTag("Player"))
        {
            
            pianist.SetActive(false);
            
        }
   }
}
