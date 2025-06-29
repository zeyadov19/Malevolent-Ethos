using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Animator))]
public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    [HideInInspector]
    public int currentHealth;

    [Header("Invincibility")]
    public float invincibilityDuration = 3f;
    public float flashInterval = 0.1f;

    [Header("Knockback Impulse")]
    [Tooltip("Default horizontal & vertical impulse when damaged without custom force.")]
    public Vector2 knockbackImpulse = new Vector2(5f, 5f);

    [Header("Death")]
    [Tooltip("Delay before disabling after death animation.")]
    public float deathDelay = 1f;
    [HideInInspector]
    public bool isDead = false;

    private Rigidbody2D    rb;
    private SpriteRenderer sr;
    private Animator       animator;
    private Color          originalColor;
    private bool           isInvincible = false;

    void Awake()
    {
        rb        = GetComponent<Rigidbody2D>();
        sr        = GetComponent<SpriteRenderer>();
        animator  = GetComponent<Animator>();
        originalColor = sr.color;
    }

    void Start()
    {
        currentHealth = maxHealth;
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
        Debug.Log($"Player took {damage} damage, health now {currentHealth}");

        // Reset vertical velocity before applying impulse
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        // Apply knockback
        if (customKnockback.HasValue)
        {
            rb.AddForce(customKnockback.Value, ForceMode2D.Impulse);

            // Also feed any horizontal component into residualX
            var pm = GetComponent<PlayerMovement>();
            if (pm != null)
                pm.knockbackResidualX = customKnockback.Value.x;
        }
        else
        {
            // Default behavior: vertical impulse + horizontal residual via movement
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

    // Keep your old TakeDamage signature for backward compatibility:
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

        // disable player
        var pm = GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        rb.simulated = false;
    }
}
