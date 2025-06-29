// PlayerStats.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Animator))]
public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    [HideInInspector] public int currentHealth;

    [Header("Stamina")]
    public float maxStamina = 20;
    [HideInInspector] public float currentStamina;
    [Tooltip("Seconds between each stamina regen tick.")]
    public float staminaRegenInterval = 3f;
    [Tooltip("How much stamina is restored each tick.")]
    public float staminaRegenAmount = 2;
    private float staminaRegenTimer;

    [Header("Invincibility")]
    public float invincibilityDuration = 3f;
    public float flashInterval = 0.1f;

    [Header("Knockback Impulse")]
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
        currentHealth      = maxHealth;
        currentStamina     = maxStamina;
        staminaRegenTimer  = staminaRegenInterval;
    }

    void Update()
    {
        // Regenerate stamina
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
    public bool SpendStamina(float amount)
    {
        if (currentStamina < amount) return false;
        currentStamina -= amount;
        Debug.Log($"Spent {amount} stamina, now {currentStamina}/{maxStamina}");
        // reset regen timer so it waits full interval
        staminaRegenTimer = staminaRegenInterval;
        return true;
    }

    /// <summary>
    /// Deal damage to the player.
    /// </summary>
    public void TakeDamage(int damage, Vector2? customKnockback = null)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage, health now {currentHealth}");

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

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, null);
    }

    private IEnumerator InvincibilityFlash()
    {
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
    }

    private IEnumerator Die()
    {
        isDead = true;
        animator.SetBool("isDead", true);
        yield return new WaitForSeconds(deathDelay);
        var pm = GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        rb.simulated = false;
    }
}
