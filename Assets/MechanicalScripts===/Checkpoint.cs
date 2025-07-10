using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Bonfire Animator (must have an 'Activate' trigger).")]
    public Animator anim;

    private bool active = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!active && other.CompareTag("Player"))
        {
            // 1) register both position & which Bonfire GO
            CheckpointManager.Instance
                .SetCheckpoint(transform.position, gameObject);

            // 2) visual + audio feedback
            if (anim != null) anim.SetTrigger("Activate");
            AudioManager.instance.PlayAt("BonFire", gameObject);

            active = true;
        }
    }
}
