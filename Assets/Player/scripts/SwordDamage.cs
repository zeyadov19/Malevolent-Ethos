using UnityEngine;

public class SwordDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 25;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Look for an IDamageable component
        var target = collision.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(damage);
        }
    }
}
