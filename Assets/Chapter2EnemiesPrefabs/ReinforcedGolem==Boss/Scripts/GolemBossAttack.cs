using UnityEngine;

public class GolemBossAttack : MonoBehaviour
{
    public int AttackDamage = 20;
    private Animator anim; 
    
    private void Start()
    {
        anim = GetComponent<Animator>();
    }
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(AttackDamage);
                anim.SetTrigger("AAAttack");

            }
        }
    }
}
