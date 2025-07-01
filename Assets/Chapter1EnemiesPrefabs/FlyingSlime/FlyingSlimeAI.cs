using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class FlyingSlimeAI : MonoBehaviour, IDamageable
{
    private enum State { Patrol, Idle, Chase }

    [Header("References")]
    [Tooltip("The box collider defining the patrol area (assign in Inspector).")]
    public BoxCollider2D patrolArea;
    [Tooltip("Player (will auto-find tag 'Player' if left null).")]
    public Transform player;

    [Header("Health")]
    public int maxHealth = 50;
    private int currentHealth;
    private bool isDead = false;

    [Header("Patrol")]
    [Tooltip("Speed while patrolling.")]
    public float patrolSpeed = 2f;
    [Tooltip("Time between stops (min/max).")]
    public float stopIntervalMin = 2f;
    public float stopIntervalMax = 5f;
    [Tooltip("Duration of each idle stop.")]
    public float idleDuration = 1.5f;

    [Header("Chase")]
    [Tooltip("Detection radius for chasing.")]
    public float chaseRange = 5f;
    [Tooltip("Distance beyond which the slime gives up chase.")]
    public float loseRange = 10f;
    [Tooltip("Speed while chasing.")]
    public float chaseSpeed = 4f;

    [Header("Damage")]
    [Tooltip("Damage dealt on contact.")]
    public int contactDamage = 15;

    // internal
    private Animator       anim;
    private SpriteRenderer sr;
    private Rigidbody2D    rb;
    private Collider2D     col;
    private State          state = State.Patrol;
    private Vector2        patrolTarget;
    private float          stopTimer;
    private float          idleTimer;
    private Color          originalColor;

    void Start()
    {
        anim          = GetComponent<Animator>();
        sr            = GetComponent<SpriteRenderer>();
        rb            = GetComponent<Rigidbody2D>();
        col           = GetComponent<Collider2D>();
        originalColor = sr.color;

        currentHealth = maxHealth;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        PickNewPatrolTarget();
        PickNextStopTime();
    }

    void Update()
    {
        if (isDead || player == null || patrolArea == null)
            return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        switch (state)
        {
            case State.Patrol:
                PatrolUpdate(distToPlayer);
                break;
            case State.Idle:
                IdleUpdate(distToPlayer);
                break;
            case State.Chase:
                ChaseUpdate(distToPlayer);
                break;
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        float speed = (state == State.Chase) ? chaseSpeed : patrolSpeed;
        if (state == State.Patrol || state == State.Chase)
        {
            Vector2 dir = (state == State.Chase)
                ? ((Vector2)player.position - rb.position).normalized
                : ((Vector2)patrolTarget - rb.position).normalized;

            sr.flipX = dir.x < 0;
            rb.linearVelocity = dir * speed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    #region State Logic

    private void PatrolUpdate(float dist)
    {
        anim.SetBool("isMoving", true);

        if (Vector2.Distance(rb.position, patrolTarget) < 0.1f)
            PickNewPatrolTarget();

        stopTimer -= Time.deltaTime;
        if (stopTimer <= 0f)
        {
            EnterIdle();
            return;
        }

        if (dist <= chaseRange)
            EnterChase();
    }

    private void IdleUpdate(float dist)
    {
        anim.SetBool("isMoving", false);

        if (dist <= chaseRange)
        {
            EnterChase();
            return;
        }

        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0f)
            EnterPatrol();
    }

    private void ChaseUpdate(float dist)
    {
        anim.SetBool("isMoving", true);

        // Give up chase if player is too far
        if (dist > loseRange)
        {
            EnterPatrol();
            return;
        }
    }

    #endregion

    #region Transitions

    private void EnterPatrol()
    {
        PickNewPatrolTarget();
        PickNextStopTime();
        state = State.Patrol;
    }

    private void EnterIdle()
    {
        idleTimer = idleDuration;
        anim.SetTrigger("Idle");
        state = State.Idle;
    }

    private void EnterChase()
    {
        state = State.Chase;
    }

    private void PickNextStopTime()
    {
        stopTimer = Random.Range(stopIntervalMin, stopIntervalMax);
    }

    private void PickNewPatrolTarget()
    {
        Bounds b = patrolArea.bounds;
        float x = Random.Range(b.min.x, b.max.x);
        float y = Random.Range(b.min.y, b.max.y);
        patrolTarget = new Vector2(x, y);
    }

    #endregion

    #region Damage & Death

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        var ps = collision.gameObject.GetComponent<PlayerStats>();
        if (ps != null)
            ps.TakeDamage(contactDamage);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        anim.SetTrigger("Hurt");
        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        anim.SetTrigger("Death");
        rb.simulated = false;
        col.enabled  = false;
        Destroy(gameObject, 1f);
    }

    #endregion

    void OnDrawGizmosSelected()
    {
        if (patrolArea != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(patrolArea.bounds.center, patrolArea.bounds.size);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, loseRange);
    }
}
