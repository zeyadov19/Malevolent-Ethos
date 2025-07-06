using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GolemBullets : MonoBehaviour
{
    public int damage = 10;
    public float speed = 8f;
    public float lifetime = 5f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        float dir = Mathf.Sign(transform.localScale.x);
        rb.linearVelocity = new Vector2(dir * speed, 0f);

        // Auto-destroy after lifetime to clean up
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // e.g. damage player
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerStats>()?.TakeDamage(damage); // or pass custom knockback
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
