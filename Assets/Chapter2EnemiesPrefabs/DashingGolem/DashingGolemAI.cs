using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class DashingGolemAI : MonoBehaviour, IDamageable
{
    private enum State { Patrol, Idle, Detect, Chase, Dash, Death }


    [Header("References")]
    public Transform[] patrolPoints;
    public Transform player;

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
    public float detectRange = 7f;
    public float detectDuration = 0.5f;
    public float chaseSpeed = 4f;
    public float chaseExitRange = 10f;

    [Header("Dash Attack")]
    public float dashRange = 3f;
    public float dashDistance = 7f;
    public float dashSpeed = 12f;
    public float dashStopSmoothTime = 0.2f;
    public int dashDamage = 25;
    public float dashChargeTime = 0.5f;

    [Header("Death")]
    public float deathDelay = 1f;

    // internals
    private State state = State.Patrol;
    private float dashCooldownTimer = 2f;
    private float abandonTimer = 5f;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Collider2D col;

    private int currentPatrolIndex = 0;
    private int originalLayer, untouchableLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
        col = GetComponent<Collider2D>();

        originalLayer = gameObject.layer;
        untouchableLayer = LayerMask.NameToLayer("Untouchable");
    }

    void Start()
    {
        currentHealth = maxHealth;
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // existing patrol init...
        nextIdleTime = Random.Range(patrolIdleIntervalMin, patrolIdleIntervalMax);
    }

    void Update()
    {
        if (isDead) return;

        //Debug.Log($"Current State: {state}");

        float dist = Vector2.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Patrol:
                PatrolUpdate(dist);
                break;
            case State.Idle:
                IdleUpdate(dist);
                break;
            case State.Chase:
                ChaseUpdate(dist);
                break;
        }
        //Debug.Log($"State: {state}");
    }

    private void PatrolUpdate(float dist)
    {
        anim.SetBool("isMoving", true);
        
        // move toward current patrol point
        Vector2 target = patrolPoints[currentPatrolIndex].position;
        Vector2 dir    = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity    = new Vector2(dir.x * patrolSpeed, rb.linearVelocity.y);
        sr.flipX = dir.x < 0;

        if (Vector2.Distance(transform.position, target) < 1f)
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;

        // countdown to Idle
        nextIdleTime -= Time.deltaTime;
        if (nextIdleTime <= 0f)
        {
            EnterIdle();
            return;
        }

        if (dist <= detectRange)
            StartCoroutine(DetectThenChase());
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
        // interrupt idle to detect/chase
        if (dist <= detectRange)
        {
            StartCoroutine(DetectThenChase());
            return;
        }

        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0f)
        {
            // pick next patrol‐idle interval and go back to Patrol
            nextIdleTime = Random.Range(patrolIdleIntervalMin, patrolIdleIntervalMax);
            state = State.Patrol;
        }
    }

    private IEnumerator DetectThenChase()
    {
        state = State.Detect;
        // stop moving
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        anim.SetBool("isMoving", false);
        anim.SetTrigger("Detect");

        yield return new WaitForSeconds(detectDuration);

        state = State.Chase;
    }

    private void ChaseUpdate(float dist)
    {
        anim.SetBool("isMoving", true);

        // give up chase
        if (dist > detectRange)
        {
            abandonTimer -= Time.deltaTime;
            if (abandonTimer <= 0f)
            {
                abandonTimer = 5f;
                state = State.Patrol;
                anim.SetTrigger("isMoving");
                return;
            }
        }
        else
        {
            abandonTimer = 5f; // reset timer if still chasing
        }

        // update dash cooldown timer
        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        // dash attack if close and not on cooldown
        if (dist <= dashRange && dashCooldownTimer <= 0f)
        {
            StartCoroutine(DashRoutine());
            dashCooldownTimer = 2f;
            return;
        }


        // otherwise chase horizontally
        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        Vector2 vel = rb.linearVelocity;
        vel.x = dir.x * chaseSpeed;
        rb.linearVelocity = vel;
        sr.flipX = dir.x < 0;
        if (dist <= dashRange)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private IEnumerator DashRoutine()
    {
        state = State.Dash;

        // 1) Stop any chase motion immediately
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // 2) Charge up
        //anim.SetTrigger("Charge");
        yield return new WaitForSeconds(dashChargeTime);

        // 3) Perform the dash
        //    (make yourself untouchable during the dash)
        gameObject.layer = untouchableLayer;
        anim.SetTrigger("Attack");
        AudioManager.instance.PlayAt("GolemPunch", gameObject);

        Vector2 dashDir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        Vector2 start = transform.position;
        float dashTimer = 0f;
        while (Vector2.Distance(start, transform.position) < dashDistance && dashTimer < 2f)
        {
            rb.linearVelocity = dashDir * dashSpeed;
            dashTimer += Time.deltaTime;
            yield return null;
        }
        // 4) Smoothly stop
        float t = 0f;
        float initialX = rb.linearVelocity.x;
        while (t < dashStopSmoothTime)
        {
            float newX = Mathf.Lerp(initialX, 0f, t / dashStopSmoothTime);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
            t += Time.deltaTime;
            yield return null;
        }
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // 5) Revert layer and finish
        gameObject.layer = originalLayer;
        state = State.Chase;
    }


    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        StartCoroutine(DamageFlash());
        //anim.SetTrigger("Hurt");
        AudioManager.instance.PlayAt("GolemHurt", gameObject);

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
        state = State.Death;
        anim.SetTrigger("Death");

        yield return new WaitForSeconds(deathDelay);

        // disable physics & collider
        rb.simulated = false;
        col.enabled = false;
        // optionally destroy after a bit
        AudioManager.instance.PlayAt("GolemDeath", gameObject);
        Destroy(gameObject, 1f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (state == State.Dash && collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerStats>()?
                .TakeDamage(dashDamage);
        }
    }
    
    void OnDrawGizmosSelected()
{
    // draw patrol point connections
    if (patrolPoints != null && patrolPoints.Length > 0)
    {
        Gizmos.color = Color.cyan;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] != null)
            {
                Gizmos.DrawWireSphere(patrolPoints[i].position, 0.2f);
                // draw line to next (wrap around)
                Transform next = patrolPoints[(i + 1) % patrolPoints.Length];
                if (next != null)
                    Gizmos.DrawLine(patrolPoints[i].position, next.position);
            }
        }
    }

    // draw detect range
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, detectRange);

    // draw chase exit range
    Gizmos.color = Color.magenta;
    Gizmos.DrawWireSphere(transform.position, chaseExitRange);

    // draw dash range
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, dashRange);

    // optionally draw dash distance area
    Gizmos.color = new Color(1, 0, 0, 0.2f);
    Gizmos.DrawWireSphere(transform.position, dashDistance);
}

}
