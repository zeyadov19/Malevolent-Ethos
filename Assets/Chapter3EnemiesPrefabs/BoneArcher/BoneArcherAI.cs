using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BoneArcherAI : MonoBehaviour, IDamageable
{
    private enum State { Patrol, Idle, Chase, Attack, Death }
    private State state = State.Patrol;

    [Header("References")]
    public Transform player;
    public Transform[] patrolPoints;
    public GameObject  arrowPrefab;
    public Transform   arrowSpawn;

    [Header("Movement")]
    public float patrolSpeed     = 2f;
    public float chaseSpeed      = 4f;
    public float detectRange     = 12f;

    [Header("Patrol Idle")]
    public float idleIntervalMin = 2f;
    public float idleIntervalMax = 5f;
    public float idleDuration    = 1f;

    [Header("Attack")]
    public float attackRange     = 10f;
    public float attackCooldown = 2f;
    public float arrowSpeed      = 12f;

    [Header("Health & Death")]
    public int maxHealth = 50;
    public float flashDuration = 0.5f;
    public float flashInterval = 0.05f;
    public float deathDelay = 1f;

    // internals
    Animator      anim;
    Rigidbody2D   rb;
    SpriteRenderer sr;
    Collider2D    col;

    int    currentHealth;
    bool   canAttack     = true;
    int    patrolIndex   = 0;
    float  nextIdleTime;
    float  idleTimer;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb   = GetComponent<Rigidbody2D>();
        sr   = GetComponent<SpriteRenderer>();
        col  = GetComponent<Collider2D>();
    }

    void Start()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;

        currentHealth = maxHealth;
        ScheduleNextIdle();
        anim.SetBool("isMoving", true);
    }

    void Update()
    {
        if (state == State.Death) return;

        float dist = Vector2.Distance(transform.position, player.position);
        switch (state)
        {
            case State.Patrol: PatrolUpdate(dist); break;
            case State.Idle:   IdleUpdate(dist);   break;
            case State.Chase:  ChaseUpdate(dist);  break;
        }
    }

    // — PATROL —
    void PatrolUpdate(float dist)
    {
        if (dist <= detectRange)
        {
            EnterChase();
            return;
        }

        nextIdleTime -= Time.deltaTime;
        if (nextIdleTime <= 0f)
        {
            EnterIdle();
            return;
        }

        if (patrolPoints.Length == 0) return;
        Transform target = patrolPoints[patrolIndex];
        float dirX = Mathf.Sign(target.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dirX * patrolSpeed, rb.linearVelocity.y);

        sr.flipX = (dirX < 0f);
        anim.SetBool("isMoving", true);

        if (Mathf.Abs(transform.position.x - target.position.x) < 0.1f)
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    // — IDLE —
    void IdleUpdate(float dist)
    {
        if (dist <= detectRange)
        {
            EnterChase();
            return;
        }

        idleTimer -= Time.deltaTime;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        anim.SetBool("isMoving", false);

        if (idleTimer <= 0f)
            EnterPatrol();
    }

    // — CHASE & ATTACK —
    void ChaseUpdate(float dist)
    {
        if (dist > detectRange)
        {
            EnterPatrol();
            return;
        }

        // Attack
        if (dist <= attackRange && canAttack)
        {
            StartCoroutine(AttackRoutine());
            return;
        }

        float dirX = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dirX * chaseSpeed, rb.linearVelocity.y);
        sr.flipX = dirX < 0f;
        anim.SetBool("isMoving", true);
    }

    IEnumerator AttackRoutine()
    {
        state = State.Attack;
        canAttack = false;
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isMoving", false);
        anim.SetTrigger("Attack");

        // Face the player before shooting
        float dirX = player.position.x - transform.position.x;
        sr.flipX = dirX < 0f;

        // instantiate arrow
        yield return new WaitForSeconds(0.5f); // small sync delay if needed
        if (arrowPrefab && arrowSpawn)
        {
            Vector2 dir = (player.position - arrowSpawn.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            var arrow = Instantiate(arrowPrefab, arrowSpawn.position, Quaternion.AngleAxis(angle, Vector3.forward));
            var arb = arrow.GetComponent<Rigidbody2D>();
            if (arb != null) arb.linearVelocity = dir * arrowSpeed;
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
        EnterChase();
    }

    // — STATE TRANSITIONS —
    void EnterPatrol()
    {
        state = State.Patrol;
        ScheduleNextIdle();
        anim.SetBool("isMoving", true);
    }

    void EnterIdle()
    {
        state = State.Idle;
        idleTimer = idleDuration;
        anim.SetTrigger("Idle");
    }

    void EnterChase()
    {
        state = State.Chase;
        anim.SetBool("isMoving", true);
    }

    void ScheduleNextIdle()
    {
        nextIdleTime = Random.Range(idleIntervalMin, idleIntervalMax);
    }

    // — DAMAGE & DEATH —
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        StartCoroutine(DamageFlash());
        anim.SetTrigger("Hurt");
        if (currentHealth <= 0)
            EnterDeath();
    }

    private IEnumerator DamageFlash()
    {
        float timer = 0f;
        while (timer < flashDuration)
        {
            sr.color = Color.gray;
            yield return new WaitForSeconds(flashInterval);
            sr.color = Color.white;
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval * 2f;
        }
        sr.color = Color.white;
    }

    void EnterDeath()
    {
        state = State.Death;
        anim.SetTrigger("Death");
        rb.simulated = false;
        col.enabled = false;
        Destroy(gameObject, deathDelay);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
