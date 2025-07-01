using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Bonfire Animator (must have an 'Activate' trigger).")]
    public Animator anim;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Bonfire Trigger hit by: {other.gameObject.name} (tag={other.gameObject.tag})");
        if (other.CompareTag("Player"))
        {
            CheckpointManager.Instance.SetCheckpoint(transform.position);
            Debug.Log("Checkpoint set at: " + transform.position);
            if (anim != null)
                anim.SetTrigger("Activate");
        }
    }
}
