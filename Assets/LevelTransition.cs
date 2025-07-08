using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        AudioManager.instance.Stop("LavaBGM");
    SceneManager.LoadScene("Loading Scene");
    }
}
