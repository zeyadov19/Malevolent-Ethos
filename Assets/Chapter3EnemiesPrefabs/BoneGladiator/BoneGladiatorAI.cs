using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(Collider2D))]
public class BoneGladiatorAI : MonoBehaviour, IDamageable
{
    private enum State { Patrol, Idle, Chase, Attack, Block, Death }
    private State state = State.Patrol;

    [Header("References")]
    public Transform player;
    public Transform[] patrolPoints;

    [Header("Detection & Movement")]
    public float detectRange = 5f;
    private float lostTime = 0f;
    public float chaseLostTime = 3f;
    public float patrolSpeed = 2f;
    public float chaseSpeed  = 4f;

    [Header("Patrol Idle")]
    public float idleIntervalMin = 3f;
    public float idleIntervalMax = 6f;
    public float idleDuration    = 1.5f;

    [Header("Melee Attack")]
    public float meleeRange    = 2f;
    public float meleeCooldown = 1f;
    public int   meleeDamage   = 20;

    [Header("Block")]
    public int   blockThreshold = 25;
    public float blockDuration  = 10f;

    [Header("Health & Death")]
    public int   maxHealth  = 100;
    public float flashDuration = 0.5f;
    public float flashInterval = 0.05f;
    public float deathDelay = 1f;

    Animator      anim;
    Rigidbody2D   rb;
    Collider2D    col;
    SpriteRenderer sr;

    int    currentHealth;
    bool   canMelee    = true;
    int    patrolIndex = 0;
    float  nextIdleTime;
    float  idleTimer;
    Coroutine blockRoutine;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb   = GetComponent<Rigidbody2D>();
        col  = GetComponent<Collider2D>();
        sr   = GetComponent<SpriteRenderer>();
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
            case State.Idle: IdleUpdate(dist); break;
            case State.Chase: ChaseUpdate(dist); break;
        }
        //Debug.Log($"State: {state}");
    }

    // ————————————— PATROL —————————————
    void PatrolUpdate(float dist)
    {
        // spot player?
        if (dist <= detectRange)
        {
            EnterChase();
            return;
        }

        // maybe go idle?
        nextIdleTime -= Time.deltaTime;
        if (nextIdleTime <= 0f)
        {
            EnterIdle();
            return;
        }

        // move toward current patrol point
        if (patrolPoints.Length == 0) return;
        Transform target = patrolPoints[patrolIndex];
        float dirX = Mathf.Sign(target.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dirX * patrolSpeed, rb.linearVelocity.y);

        // face direction
        sr.flipX = (dirX < 0f);
        anim.SetBool("isMoving", true);

        // reached point?
        if (Mathf.Abs(transform.position.x - target.position.x) < 0.1f)
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    // ————————————— IDLE —————————————
    void IdleUpdate(float dist)
    {
        // spot player?
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

    // ————————————— CHASE —————————————
    void ChaseUpdate(float dist)
    {
        // lost sight?
        if (dist > detectRange)
        {
            lostTime += Time.deltaTime;
            if (lostTime >= chaseLostTime)
            {
                lostTime = 0f;
                EnterIdle();
                return;
            }
        }

        // melee attack if in range
        if (dist <= meleeRange && canMelee)
        {
            StartCoroutine(Melee());
            return;
        }

        // chase player horizontally
        float dirX = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dirX * chaseSpeed, rb.linearVelocity.y);
        sr.flipX = (dirX < 0f);
        anim.SetBool("isMoving", true);
    }

    IEnumerator Melee()
    {
        state = State.Attack;
        anim.SetBool("isMoving", false);
        canMelee = false;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(meleeCooldown);
        if (state == State.Block)
            canMelee = true;
        else
        {
            canMelee = true;
            state = State.Chase;
        }
    }

    void EnterBlock()
    {
        if (blockRoutine != null)
            StopCoroutine(blockRoutine);
        blockRoutine = StartCoroutine(BlockRoutine());
    }

    IEnumerator BlockRoutine()
    {
        state = State.Block;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("Block");

        float t = blockDuration;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            // face player
            float dx = player.position.x - transform.position.x;
            sr.flipX = (dx < 0f);
            yield return null;
        }

        // recover & resume chase
        currentHealth = maxHealth;
        EnterChase();
    }

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

    public void TakeDamage(int amount)
    {
        if (state == State.Block) return;

        currentHealth -= amount;
        StartCoroutine(DamageFlash());
        anim.SetTrigger("Hurt");
        if (currentHealth <= 0)
            EnterDeath();
        else if (currentHealth <= blockThreshold)
            EnterBlock();
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

    public void TakeDash()
    {
        if (state == State.Block)
            EnterDeath();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("ShadowDash"))
            TakeDash();
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
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null) continue;
                Gizmos.DrawWireSphere(patrolPoints[i].position, 0.2f);
                Transform nxt = patrolPoints[(i + 1) % patrolPoints.Length];
                if (nxt != null)
                    Gizmos.DrawLine(patrolPoints[i].position, nxt.position);
            }
        }
    }
}
