using UnityEngine;

public class LavaTrigger : MonoBehaviour
{
    public RisingLava lava; // Assign in Inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            AudioManager.instance.Stop("BGM");
            AudioManager.instance.Play("LavaBGM");
            lava.StartLava();
            gameObject.SetActive(false); // Optional: disable the trigger after use
        }
    }
}
