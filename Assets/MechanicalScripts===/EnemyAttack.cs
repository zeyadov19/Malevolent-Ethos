using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public int AttackDamage = 20; 

    
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
