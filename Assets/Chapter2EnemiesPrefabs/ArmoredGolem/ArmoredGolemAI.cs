using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class ArmoredGolemAI : MonoBehaviour, IDamageable
{
    private enum State { Patrol, Idle, ArmorUp, Chase, Attack, Death }

    [Header("References")]
    public Transform[] patrolPoints;
    public Transform   player;

    [Header("Stats")]
    public int maxHealth = 150;
    private int currentHealth;
    public float flashDuration = 0.5f;
    public float flashInterval = 0.05f;
    private Color originalColor;
    private bool isDead = false;

    [Header("Patrol")]
    public float patrolSpeed = 2f;

    [Header("Patrol Idle Timing")]
    public float patrolIdleIntervalMin = 3f;
    public float patrolIdleIntervalMax = 6f;
    public float idleDuration = 1.5f;
    private float nextIdleTime;
    private float idleTimer;

    [Header("Detect & Chase")]
    public float detectRange    = 7f;
    public float detectDuration = 0.5f;
    public float chaseSpeed     = 4f;
    public float chaseExitRange = 10f;

    [Header("Melee Attack")]
    public float meleeRange     = 2f;
    public float meleeCooldown  = 1f;
    public int   meleeDamage    = 20;
    private bool  canAttack     = true;

    [Header("Death")]
    public float deathDelay = 1f;

    // internals
    private State        state = State.Patrol;
    private Rigidbody2D  rb;
    private Animator     anim;
    private SpriteRenderer sr;
    private Collider2D   col;
    private int          currentPatrolIndex = 0;

    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
        col  = GetComponent<Collider2D>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        nextIdleTime = Random.Range(patrolIdleIntervalMin, patrolIdleIntervalMax);
    }

    void Update()
    {
        if (isDead) return;

        float dist = Vector2.Distance(transform.position, player.position);
        switch (state)
        {
            case State.Patrol:   PatrolUpdate(dist); break;
            case State.Idle:     IdleUpdate(dist);   break;
            case State.Chase:    ChaseUpdate(dist);  break;
        }
    }

    private void PatrolUpdate(float dist)
    {
        anim.SetBool("isMoving", true);

        Vector2 target = patrolPoints[currentPatrolIndex].position;
        Vector2 dir    = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity    = new Vector2(dir.x * patrolSpeed, rb.linearVelocity.y);
        sr.flipX       = dir.x < 0;

        if (Vector2.Distance(transform.position, target) < 0.1f)
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;

        nextIdleTime -= Time.deltaTime;
        if (nextIdleTime <= 0f)
        {
            EnterIdle();
            return;
        }

        if (dist <= detectRange)
            StartCoroutine(ArmorUpThenChase());
    }

    private void EnterIdle()
    {
        state = State.Idle;
        idleTimer = idleDuration;
        anim.SetTrigger("Idle");
        anim.SetBool("isMoving", false);
    }

    private void IdleUpdate(float dist)
    {
        if (dist <= detectRange)
        {
            StartCoroutine(ArmorUpThenChase());
            return;
        }

        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0f)
        {
            nextIdleTime = Random.Range(patrolIdleIntervalMin, patrolIdleIntervalMax);
            state = State.Patrol;
        }
    }

    private IEnumerator ArmorUpThenChase()
    {
        state = State.ArmorUp;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        anim.SetTrigger("ArmorUp");

        yield return new WaitForSeconds(detectDuration);

        state = State.Chase;
    }

    private void ChaseUpdate(float dist)
    {
        anim.SetBool("isMoving", true);

        if (dist > chaseExitRange)
        {
            anim.SetTrigger("ArmorDown");
            state = State.Patrol;
            return;
        }

        if (dist <= meleeRange && canAttack)
        {
            StartCoroutine(PerformMeleeAttack());
            return;
        }

        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        Vector2 vel = rb.linearVelocity;
        vel.x       = dir.x * chaseSpeed;
        rb.linearVelocity = vel;
        sr.flipX    = dir.x < 0;
    }

    private IEnumerator PerformMeleeAttack()
    {
        state = State.Attack;
        canAttack = false;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(meleeCooldown);

        if (Vector2.Distance(transform.position, player.position) <= meleeRange)
            player.GetComponent<PlayerStats>()?.TakeDamage(meleeDamage);

        canAttack = true;
        state = State.Chase;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        StartCoroutine(DamageFlash());
        anim.SetTrigger("Hurt");

        if (currentHealth <= 0)
            StartCoroutine(DieRoutine());
    }
    
    private IEnumerator DamageFlash()
    {
        float timer = 0f;
        while (timer < flashDuration)
        {
            sr.color = Color.gray;
            yield return new WaitForSeconds(flashInterval);
            sr.color = originalColor;
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval * 2f;
        }
        sr.color = originalColor;
    }

    private IEnumerator DieRoutine()
    {
        isDead = true;
        anim.SetTrigger("Death");
        yield return new WaitForSeconds(deathDelay);

        rb.simulated = false;
        col.enabled = false;
        Destroy(gameObject, 1f);
    }

    private void OnCollisionEnter2D(Collision2D colInfo)
    {
        if (state == State.Attack && colInfo.gameObject.CompareTag("Player"))
        {
            colInfo.gameObject.GetComponent<PlayerStats>()?
                .TakeDamage(meleeDamage);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, chaseExitRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
}
