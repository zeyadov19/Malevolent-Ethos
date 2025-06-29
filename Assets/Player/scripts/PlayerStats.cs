using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Animator))]
public class PlayerStats : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    [HideInInspector] public int currentHealth;

    [Header("Regeneration Settings")]
    public float regenInterval = 3f;
    public int regenAmount = 1;
    private float regenTimer;

    [Header("Invincibility Settings")]
    public float invincibilityDuration = 3f;
    public float flashInterval = 0.1f;

    [Header("Knockback Settings")]
    [Tooltip("Horizontal and vertical velocity applied on damage.")]
    public Vector2 knockbackVelocity = new Vector2(5f, 5f);
    [Tooltip("How long the player is locked out of movement.")]
    public float knockbackLockDuration = 0.2f;

    [Header("Death")]
    public bool isDead = false;

    private Animator       animator;
    private Rigidbody2D    rb;
    private SpriteRenderer sr;
    private PlayerMovement pm;
    private Color originalColor;
    private bool           isInvincible = false;

    void Awake()
    {
        rb            = GetComponent<Rigidbody2D>();
        sr            = GetComponent<SpriteRenderer>();
        pm = GetComponent<PlayerMovement>();
        animator      = GetComponent<Animator>();
        originalColor = sr.color;
    }

    void Start()
    {
        currentHealth = maxHealth;
        regenTimer    = regenInterval;
    }

    void Update()
    {
        if (isDead || isInvincible) return;

        regenTimer -= Time.deltaTime;
        if (regenTimer <= 0f)
        {
            RegenerateHealth();
            regenTimer = regenInterval;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage, current health: {currentHealth}");

        // ── FIXED KNOCKBACK DIRECTION ──
        // push *forward* in the direction the sprite is facing
        float dir = sr.flipX ? -1f : 1f;
        rb.linearVelocity = new Vector2(dir * knockbackVelocity.x,
                                  knockbackVelocity.y);

        // lock movement
        if (pm != null)
            pm.isKnockedBack = true;
        StartCoroutine(EndKnockback(pm, knockbackLockDuration));

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            animator.SetTrigger("Hurt");
            StartCoroutine(InvincibilityFlash());
        }
    }

    private IEnumerator EndKnockback(PlayerMovement pm, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (pm != null)
            pm.isKnockedBack = false;
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

        sr.color = originalColor;
        isInvincible = false;
    }

    void RegenerateHealth()
    {
        if (currentHealth < maxHealth)
            currentHealth += regenAmount;
    }

    void Die()
    {
        isDead = true;
        animator.SetBool("isDead", true);
        //pm.enabled = false; 
        
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Player healed {amount}, current health: {currentHealth}");
    }
}
