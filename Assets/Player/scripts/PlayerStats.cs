using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Animator))]
public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    [HideInInspector] public int currentHealth;

    [Header("Health Regeneration")]
    [Tooltip("Seconds between each health regen tick.")]
    public float healthRegenInterval = 3f;
    [Tooltip("How much health is restored each tick.")]
    public int healthRegenAmount = 1;
    private float healthRegenTimer;

    [Header("Stamina")]
    public int maxStamina = 20;
    [HideInInspector] public int currentStamina;

    [Header("Stamina Regeneration")]
    [Tooltip("Seconds between each stamina regen tick.")]
    public float staminaRegenInterval = 3f;
    [Tooltip("How much stamina is restored each tick.")]
    public int staminaRegenAmount = 2;
    private float staminaRegenTimer;

    [Header("Invincibility")]
    public float invincibilityDuration = 3f;
    public float flashInterval = 0.1f;

    [Header("Knockback Impulse")]
    [Tooltip("Default horizontal & vertical impulse when damaged without custom force.")]
    public Vector2 knockbackImpulse = new Vector2(5f, 5f);

    [Header("Death")]
    [Tooltip("Delay before disabling after death animation.")]
    public float deathDelay = 1f;
    [HideInInspector] public bool isDead = false;

    private Rigidbody2D    rb;
    private SpriteRenderer sr;
    private Animator       animator;
    private Color          originalColor;
    private bool           isInvincible = false;

    void Awake()
    {
        rb            = GetComponent<Rigidbody2D>();
        sr            = GetComponent<SpriteRenderer>();
        animator      = GetComponent<Animator>();
        originalColor = sr.color;
    }

    void Start()
    {
        currentHealth     = maxHealth;
        currentStamina    = maxStamina;
        healthRegenTimer  = healthRegenInterval;
        staminaRegenTimer = staminaRegenInterval;
    }

    void Update()
    {
        // Health Regen
        if (!isDead && currentHealth < maxHealth)
        {
            healthRegenTimer -= Time.deltaTime;
            if (healthRegenTimer <= 0f)
            {
                currentHealth = Mathf.Min(currentHealth + healthRegenAmount, maxHealth);
                healthRegenTimer = healthRegenInterval;
                Debug.Log($"Health regenerated to {currentHealth}/{maxHealth}");
            }
        }

        // Stamina Regen
        if (currentStamina < maxStamina)
        {
            staminaRegenTimer -= Time.deltaTime;
            if (staminaRegenTimer <= 0f)
            {
                currentStamina = Mathf.Min(currentStamina + staminaRegenAmount, maxStamina);
                staminaRegenTimer = staminaRegenInterval;
                Debug.Log($"Stamina regenerated to {currentStamina}/{maxStamina}");
            }
        }
    }

    /// <summary>
    /// Try to spend stamina. Returns true if there was enough.
    /// </summary>
    public bool SpendStamina(int amount)
    {
        if (currentStamina < amount) return false;
        currentStamina -= amount;
        Debug.Log($"Spent {amount} stamina, now {currentStamina}/{maxStamina}");
        staminaRegenTimer = staminaRegenInterval;
        return true;
    }

    /// <summary>
    /// Deal damage to the player. 
    /// If customKnockback is provided, use that impulse; otherwise use default.
    /// </summary>
    public void TakeDamage(int damage, Vector2? customKnockback = null)
    {
        if (isDead || isInvincible) 
            return;

        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage, health now {currentHealth}/{maxHealth}");

        // Reset vertical velocity
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        // Apply knockback
        if (customKnockback.HasValue)
        {
            rb.AddForce(customKnockback.Value, ForceMode2D.Impulse);
            var pm = GetComponent<PlayerMovement>();
            if (pm != null)
                pm.knockbackResidualX = customKnockback.Value.x;
        }
        else
        {
            rb.AddForce(new Vector2(0f, knockbackImpulse.y), ForceMode2D.Impulse);
            var pm = GetComponent<PlayerMovement>();
            if (pm != null)
            {
                float dir = sr.flipX ? -1f : 1f;
                pm.knockbackResidualX = dir * knockbackImpulse.x;
            }
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            StartCoroutine(Die());
        }
        else
        {
            animator.SetTrigger("Hurt");
            StartCoroutine(InvincibilityFlash());
        }
    }

    // Backward compatibility
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, null);
    }

    private IEnumerator InvincibilityFlash()
    {
        gameObject.layer = LayerMask.NameToLayer("Recovering");
        isInvincible = true;
        float timer = 0f;
        while (timer < invincibilityDuration)
        {
            sr.color = Color.gray;
            yield return new WaitForSeconds(flashInterval);
            sr.color = originalColor;
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval * 2f;
        }
        isInvincible = false;
        sr.color = originalColor;
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    private IEnumerator Die()
    {
        isDead = true;
        animator.SetBool("isDead", true);
        yield return new WaitForSeconds(deathDelay);

        // disable player
        var pm = GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        rb.simulated = false;

        // hand off to checkpoint manager
        CheckpointManager.Instance.HandlePlayerDeath(gameObject);
    }
}
