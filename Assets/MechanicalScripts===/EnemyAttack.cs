using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public int AttackDamage = 20; 


    public void FlipGameObject(bool facingRight)
    {
        // Flip the GameObject by changing its localScale.x
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1 : -1);
        transform.localScale = scale;
    }
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(AttackDamage);
            }
        }
    }
}
