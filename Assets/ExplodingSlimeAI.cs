using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ExplodingSlimeAI : MonoBehaviour, IDamageable
{
    private enum State { Patrol, Idle, Chase, ExplodeCountdown }

    [Header("References")]
    [Tooltip("Player transform (will auto-find tagged 'Player' if left null).")]
    public Transform player;

    [Header("Health")]
    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    [Header("Patrol Settings")]
    [Tooltip("Waypoints for patrolling.")]
    public Transform[] patrolPoints;
    [Tooltip("Horizontal speed when patrolling.")]
    public float patrolSpeed = 2f;

    [Header("Chase Settings")]
    [Tooltip("Horizontal speed when chasing or during countdown.")]
    public float chaseSpeed = 5f;

    [Header("Idle Settings")]
    [Tooltip("Min/Max time between patrol idles.")]
    public float patrolIdleIntervalMin = 3f;
    public float patrolIdleIntervalMax = 6f;
    [Tooltip("Fixed duration of each idle stop.")]
    public float idleDuration = 1.5f;

    [Header("Chase / Explosion Ranges")]
    [Tooltip("Distance at which slime starts chasing.")]
    public float chaseRange = 6f;
    [Tooltip("Distance at which slime begins explode countdown.")]
    public float explodeRange = 1.5f;

    [Header("Explosion Settings")]
    [Tooltip("Time (sec) from countdown start to explode.")]
    public float explodeCountdown = 3f;
    [Tooltip("Flash interval (sec) during countdown.")]
    public float flashInterval = 0.2f;
    [Tooltip("Final explosion radius.")]
    public float explosionRadius = 4f;
    [Tooltip("Damage dealt by explosion.")]
    public int explodeDamage = 50;
    [Tooltip("Horizontal force applied to player on explode.")]
    public float explosionKnockback = 8f;
    [Tooltip("Vertical force applied to player on explode.")]
    public float explosionUpwardKnockback = 5f;

    [Header("Death")]
    [Tooltip("Seconds to wait before destroying after explode or taking fatal damage.")]
    public float deathDelay = 0.5f;

    // Internal
    private Animator       anim;
    private SpriteRenderer sr;
    private Rigidbody2D    rb;
    private Collider2D     col;
    private State          state = State.Patrol;
    private int            patrolIndex = 0;
    private float          stateTimer = 0f;
    private float          nextIdleTime = 0f;
    private float          moveDirection = 0f;
    private Color          originalColor;

    void Start()
    {
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();
        rb   = GetComponent<Rigidbody2D>();
        col  = GetComponent<Collider2D>();
        originalColor = sr.color;

        currentHealth = maxHealth;
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        PickNextIdleTime();
    }

    void Update()
    {
        if (isDead || player == null || patrolPoints.Length < 2)
            return;

        float dist = Vector2.Distance(transform.position, player.position);
        switch (state)
        {
            case State.Patrol:          PatrolUpdate(dist);         break;
            case State.Idle:            IdleUpdate(dist);           break;
            case State.Chase:           ChaseUpdate(dist);          break;
            case State.ExplodeCountdown: ExplodeCountdownUpdate(dist); break;
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // Choose speed based on state
        float speed = patrolSpeed;
        if (state == State.Chase || state == State.ExplodeCountdown)
            speed = chaseSpeed;

        Vector2 v = rb.linearVelocity;
        v.x = moveDirection * speed;
        rb.linearVelocity = v;
    }

    #region State Logic

    private void PatrolUpdate(float dist)
    {
        anim.SetBool("isWalking", true);
        Vector2 target = patrolPoints[patrolIndex].position;
        moveDirection = Mathf.Sign(target.x - transform.position.x);
        sr.flipX = (moveDirection < 0);

        if (Mathf.Abs(transform.position.x - target.x) < 0.1f)
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;

        nextIdleTime -= Time.deltaTime;
        if (nextIdleTime <= 0f)
        {
            EnterIdle();
            return;
        }

        if (dist <= chaseRange)
            EnterChase();
    }

    private void IdleUpdate(float dist)
    {
        anim.SetBool("isWalking", false);
        moveDirection = 0f;

        if (dist <= chaseRange)
        {
            EnterChase();
            return;
        }

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
            EnterPatrol();
    }

    private void ChaseUpdate(float dist)
    {
        anim.SetBool("isWalking", true);
        moveDirection = Mathf.Sign(player.position.x - transform.position.x);
        sr.flipX = (moveDirection < 0);

        if (dist <= explodeRange)
            EnterExplodeCountdown();
    }

    private void ExplodeCountdownUpdate(float dist)
    {
        anim.SetBool("isWalking", true);
        moveDirection = Mathf.Sign(player.position.x - transform.position.x);
        sr.flipX = (moveDirection < 0);

        float cycle = Mathf.PingPong(Time.time, flashInterval * 2f);
        sr.color = (cycle < flashInterval) ? Color.gray : originalColor;

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
            Explode();
    }

    #endregion

    #region State Transitions

    private void EnterPatrol()
    {
        PickNextIdleTime();
        anim.SetBool("isWalking", true);
        state = State.Patrol;
    }

    private void EnterIdle()
    {
        stateTimer = idleDuration;
        anim.SetTrigger("Idle");
        anim.SetBool("isWalking", false);
        state = State.Idle;
    }

    private void EnterChase()
    {
        anim.SetBool("isWalking", true);
        state = State.Chase;
    }

    private void EnterExplodeCountdown()
    {
        stateTimer = explodeCountdown;
        anim.SetTrigger("Attack");
        state = State.ExplodeCountdown;
    }

    private void PickNextIdleTime()
    {
        nextIdleTime = Random.Range(patrolIdleIntervalMin, patrolIdleIntervalMax);
    }

    #endregion

    #region Explosion & Death

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            Vector2 dir = (hit.transform.position - transform.position).normalized;
            Vector2 force = new Vector2(dir.x * explosionKnockback,
                                        explosionUpwardKnockback);

            var ps = hit.GetComponent<PlayerStats>();
            if (ps != null)
                ps.TakeDamage(explodeDamage, force);
        }

        anim.SetTrigger("Death");
        isDead = true;
        col.enabled = false;
        rb.simulated = false;
        Destroy(gameObject, deathDelay);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        anim.SetTrigger("Hurt");
        if (currentHealth <= 0)
            Explode();  // die by exploding immediately
    }

    #endregion

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explodeRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
