using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(Collider2D))]
public class BoneGladiatorAI : MonoBehaviour, IDamageable
{
    private enum State { Chase, Patrol, Idle, Attack, Block }
    private State state = State.Chase;

    [Header("References")]
    public Transform player;
    //public Collider2D wakeTrigger;
    public Transform[] patrolPoints;
    public float detectRange = 5f;

    [Header("Timings")]
    public float leaveDelay      = 5f;     // after losing player
    public float patrolDuration  = 20f;    // total patrol before sleep
    public float idleIntervalMin = 3f;
    public float idleIntervalMax = 6f;
    public float idleDuration    = 1.5f;
    public float blockDuration   = 10f;

    [Header("Movement")]
    public float patrolSpeed = 2f;
    public float chaseSpeed  = 4f;

    [Header("Attack")]
    public float meleeRange    = 2f;
    public float meleeCooldown = 1f;
    public int   meleeDamage   = 20;

    [Header("Health")]
    public int blockThreshold = 25;
    public int maxHealth      = 100;
    public float flashDuration = 1.5f;
    public float flashInterval = 0.05f;

    // internals
    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private int currentHealth;
    private int         patrolIndex;
    private float       leaveTimer;
    //private float       patrolTimer;
    private float       nextIdleTime;
    private float       idleTimer;
    private bool        canMelee = true;
    private Coroutine   blockRoutine;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb   = GetComponent<Rigidbody2D>();
        sr   = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    void Start()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;
    }

    void Update()
    {
        switch (state)
        {
            case State.Chase:
                ChaseUpdate();
                break;
            case State.Patrol:
                PatrolUpdate();
                break;
            case State.Idle:
                IdleUpdate();
                break;
        }
    }

    private void ChaseUpdate()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        // Attack if in melee range
        if (dist <= meleeRange && canMelee)
        {
            StartCoroutine(MeleeAttack());
            return;
        }

        // Lost sight?
        if (dist > detectRange)
        {
            leaveTimer = leaveDelay;
            state = State.Patrol;
            anim.SetBool("isMoving", true);
            return;
        }

        // Move toward player
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * chaseSpeed, rb.linearVelocity.y);
        anim.SetBool("isMoving", true);
        transform.localScale = new Vector3(dir.x < 0 ? -1 : 1, 1, 1);
    }

    private IEnumerator MeleeAttack()
    {
        state = State.Attack;
        canMelee = false;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(meleeCooldown);
        canMelee = true;
        state = State.Chase;
    }

    private void PatrolUpdate()
    {
        // patrolTimer += Time.deltaTime;
        // leaveTimer -= Time.deltaTime;

        // Lost player too long? continue patrolling inside box
        if (leaveTimer <= 0f)
        {
            // Move between patrol points
            Vector2 target = patrolPoints[patrolIndex].position;
            Vector2 dir    = (target - (Vector2)transform.position).normalized;
            rb.linearVelocity    = new Vector2(dir.x * patrolSpeed, rb.linearVelocity.y);
            anim.SetBool("isMoving", true);
            transform.localScale = new Vector3(dir.x < 0 ? -1 : 1, 1, 1);

            if (Vector2.Distance(transform.position, target) < 0.1f)
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            }

            // occasional idle
            nextIdleTime -= Time.deltaTime;
            if (nextIdleTime <= 0f)
                EnterIdle();
        }
        else
        {
            EnterChase();
        }
    }

    private void IdleUpdate()
    {
        idleTimer -= Time.deltaTime;
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isMoving", false);

        if (idleTimer <= 0f)
        {
            state = State.Patrol;
            nextIdleTime = Random.Range(idleIntervalMin, idleIntervalMax);
        }
    }

    private void EnterChase()
    {
        state = State.Chase;
        anim.SetBool("isMoving", true);
    }

    private void EnterIdle()
    {
        state = State.Idle;
        idleTimer = idleDuration;
        anim.SetTrigger("Idle");
    }

    private void EnterBlock()
    {
        if (blockRoutine != null) StopCoroutine(blockRoutine);
        blockRoutine = StartCoroutine(BlockRoutine());
    }

    private IEnumerator BlockRoutine()
    {
        state = State.Block;
        rb.linearVelocity = Vector2.zero;
        anim.Play("isBlocking");
        float timer = blockDuration;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            // face player
            float dx = player.position.x - transform.position.x;
            transform.localScale = new Vector3(dx < 0 ? -1 : 1, 1, 1);
            yield return null;
        }
        // recover HP
        currentHealth = maxHealth;
        EnterChase();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ShadowDash"))
        {
            TakeDash();
        }
    }

    public void TakeDamage(int amount)
    {
        if (state == State.Block)
        {

            return;
        }
        currentHealth -= amount;
        StartCoroutine(DamageFlash());
        anim.SetTrigger("Hurt");
        if (currentHealth <= blockThreshold)
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

    // Call this from your ShadowDash collision code instead:
    public void TakeDash()
    {
        if (state == State.Block)
        {
            // die immediately
            anim.SetTrigger("Death");
            // Add any additional death logic here if needed
        }
    }
}
