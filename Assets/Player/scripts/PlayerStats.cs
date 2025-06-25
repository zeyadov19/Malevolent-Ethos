using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Regeneration Settings")]
    public float regenInterval = 3f;
    public int regenAmount = 1; 
    private float regenTimer;


    [Header("Death")]
    public bool isDead = false;

    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        regenTimer = regenInterval;
    }

    void Update()
    {
        if (!isDead)
        {
            regenTimer -= Time.deltaTime;
            if (regenTimer <= 0f)
            {
                RegenerateHealth();
                regenTimer = regenInterval;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage, current health: {currentHealth}");
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void RegenerateHealth()
    {
        if (currentHealth < maxHealth)
        {
            currentHealth += regenAmount;
        }
    }

    void Die()
    {
        isDead = true;
        if (animator != null)
        {
            animator.SetBool("isDead", true);
        }
        // Add any additional death behavior here (disable movement, show UI, etc.)
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        Debug.Log($"Player healed {amount}, current health: {currentHealth}");
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
}
