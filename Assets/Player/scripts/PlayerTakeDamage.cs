using UnityEngine;

public class PlayerTakeDamage : MonoBehaviour
{
    public int contactDamage = 100;
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        var ps = collision.gameObject.GetComponent<PlayerStats>();
        if (ps != null)
            ps.TakeDamage(contactDamage);
    }
}
