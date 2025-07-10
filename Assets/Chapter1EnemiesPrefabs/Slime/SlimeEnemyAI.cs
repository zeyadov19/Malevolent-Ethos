using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SlimeEnemyAI : MonoBehaviour, IDamageable
{
    private enum State { Patrol, Idle, Attack }

    [Header("References")]
    public Transform player;
    public PlayerStats ps;

    [Header("Stats")]
    public int maxHealth = 100;
    public float flashDuration = 0.5f;
    public float flashInterval = 0.05f;
    private Color originalColor;

    private int currentHealth;
    private bool isDead = false;
    public int contactDamage = 25;

    [Header("Stomp Settings")]
    public float stompBounceForce = 10f;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    [Tooltip("Horizontal move speed.")]
    public float moveSpeed = 2f;

    [Header("Patrol Idle Timing")]
    public float patrolIdleIntervalMin = 3f;
    public float patrolIdleIntervalMax = 6f;
    public float idleDuration = 1.5f;

    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public float attackDuration = 1f;


    private Animator       anim;
    private SpriteRenderer sr;
    private Rigidbody2D    rb;
    private Collider2D     collider2d;
    private State          state               = State.Patrol;
    private int            currentPatrolIndex  = 0;
    private float          stateTimer          = 0f;
    private float          nextPatrolIdleTime  = 0f;
    private float          moveDirection       = 0f;

    void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<Collider2D>();
        originalColor = sr.color;
        currentHealth = maxHealth;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        ps = player.GetComponent<PlayerStats>();
        //Debug.Log($"Found player: {player != null}, PlayerStats: {ps != null}");

        PickNextPatrolIdleTime();
        AudioManager.instance.PlayAt("SlimeWalk", gameObject);
    }

    void Update()
    {
        if (isDead || player == null || patrolPoints.Length < 2)
            return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Patrol: PatrolUpdate(distToPlayer); break;
            case State.Idle:   IdleUpdate(distToPlayer);   break;
            case State.Attack: AttackUpdate(distToPlayer); break;
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // Apply horizontal velocity; leave Y velocity for gravity
        Vector2 vel = rb.linearVelocity;
        vel.x = moveDirection * moveSpeed;
        rb.linearVelocity = vel;
    }

    #region State Logic

    private void PatrolUpdate(float dist)
    {
        anim.SetBool("isWalking", true);
        

        // Determine next patrol target and face it
        Vector2 target = patrolPoints[currentPatrolIndex].position;
        moveDirection = Mathf.Sign(target.x - transform.position.x);
        sr.flipX = (moveDirection < 0f);

        // Advance patrol if reached
        if (Mathf.Abs(transform.position.x - target.x) < 0.5f)
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;

        // Idle timer
        nextPatrolIdleTime -= Time.deltaTime;
        if (nextPatrolIdleTime <= 0f)
        {
            EnterIdle();
            return;
        }

        // Spot player -> Attack
        if (dist <= attackRange)
            EnterAttack();
    }

    private void IdleUpdate(float dist)
    {
        moveDirection = 0f;
        stateTimer -= Time.deltaTime;

        if (!isDead && dist <= attackRange)
        {
            EnterAttack();
            return;
        }

        if (stateTimer <= 0f)
        {
            EnterPatrol();
        }
            
    }

    private void AttackUpdate(float dist)
    {
        moveDirection = 0f;
        // Face player
        sr.flipX = (player.position.x < transform.position.x);

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
            EnterPatrol();
    }

    #endregion

    #region State Transitions

    private void EnterPatrol()
    {
        AudioManager.instance.PlayAt("SlimeWalk", gameObject);
        PickNextPatrolIdleTime();
        anim.SetBool("isWalking", true);
        state = State.Patrol;
    }

    private void EnterIdle()
    {
        AudioManager.instance.StopAt("SlimeWalk", gameObject);
        moveDirection = 0f;
        stateTimer = idleDuration;
        anim.SetTrigger("Idle");
        anim.SetBool("isWalking", false);
        state = State.Idle;
    }

    private void EnterAttack()
    {
        moveDirection = 0f;
        stateTimer = attackDuration;
        anim.SetTrigger("Attack");
        anim.SetBool("isWalking", false);
        AudioManager.instance.StopAt("SlimeWalk", gameObject);
        // Face player immediately
        sr.flipX = (player.position.x < transform.position.x);
        state = State.Attack;
    }

    private void PickNextPatrolIdleTime()
    {
        nextPatrolIdleTime = Random.Range(patrolIdleIntervalMin, patrolIdleIntervalMax);
    }

    #endregion

    #region Damage & Death

    /// <summary>
    /// Call this to damage the crab (e.g. from sword or stomp).
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        AudioManager.instance.StopAt("SlimeWalk", gameObject);
        AudioManager.instance.PlayAt("SlimeHurt", gameObject);
        StartCoroutine(DamageFlash());
        
        if (currentHealth > 0)
        {
            anim.SetTrigger("Hurt");
        }
        else
        {
            Die();
        }
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

    private void Die()
    {
        isDead = true;
        anim.SetTrigger("Death");
        rb.simulated = false;
        rb.linearVelocity = Vector2.zero;
        this.enabled = false;
    }

    #endregion

    #region Stomp Detection

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

        if (!collision.gameObject.CompareTag("Player"))
            return;

        // Player ran into us — deal contact damage
        if (ps != null)
        {
            ps.TakeDamage(contactDamage);
        }
        
        //     Rigidbody2D playerRb = collision.rigidbody;
        // if (playerRb != null)
        //     playerRb.linearVelocity = new Vector2(
        //         (collision.transform.position.x < transform.position.x ? -1 : 1) * stompBounceForce,
        //         playerRb.linearVelocity.y
        //     );

    }

    #endregion

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
