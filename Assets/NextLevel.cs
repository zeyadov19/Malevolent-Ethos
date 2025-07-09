using UnityEngine;
using UnityEngine.SceneManagement;
public class NextLevel : MonoBehaviour
{

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneTracker.GoToLoading();
        }
    }    
}
