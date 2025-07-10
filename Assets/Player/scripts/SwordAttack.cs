using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackCooldown = 0.5f;
    private bool canAttack = true;

    [Header("Attack HitBox")]
    public Transform attackHitBox;

    [Header("Hitbox Positions")]
    public Vector2 rightPosition = new Vector2(0.12f, 0f);
    public Vector2 leftPosition = new Vector2(-0.12f, 0f);

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Handle input
        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            Attack();
            AudioManager.instance.PlayAt("PlayerAttack", gameObject);
        }

        // Handle hitbox flipping
        FlipHitbox();
    }

    void Attack()
    {
        animator.SetTrigger("Attack");
        canAttack = false;
        Invoke("ResetAttack", attackCooldown);
    }

    void ResetAttack()
    {
        canAttack = true;
    }

    void FlipHitbox()
    {
        if (spriteRenderer.flipX)
        {
            attackHitBox.localPosition = leftPosition;
        }
        else
        {
            attackHitBox.localPosition = rightPosition;
        }
    }
}
