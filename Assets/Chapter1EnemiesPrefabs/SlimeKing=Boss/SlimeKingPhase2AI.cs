using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class SlimeKingPhase2AI : MonoBehaviour
{
    [Header("References")]
    public SlimeKingStats stats;
    public Transform      player;

    [Header("Ground Check")]
    public Transform  groundCheck;
    public float      groundCheckRadius = 0.2f;
    public LayerMask  groundLayer;

    [Header("Chase")]
    public float chaseSpeed = 3f;

    [Header("AttackA")]
    public float attackARange    = 2.5f;
    public float attackACooldown = 1.5f;
    public int   attackADamage   = 20;

    [Header("AttackB")]
    public float attackBRange    = 3.5f;
    public float attackBCooldown = 3f;
    public int   attackBDamage   = 30;

    [Header("Rampage Settings")]
    public float rageAnimationDuration   = 1f;
    public int   rampageJumpCount        = 5;
    public float timeBetweenJumps        = 1f;
    public float rampageHorizontalForce = 12f;
    public float rampageVerticalForce   = 8f;
    public int   rampageContactDamage   = 35;

    private Rigidbody2D    rb;
    private Animator       anim;
    private SpriteRenderer sr;
    private bool           canAttackA = true;
    private bool           canAttackB = true;

    private enum State { Chase, AttackA, AttackB, Rampage }
    private State state = State.Chase;

    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr  = GetComponent<SpriteRenderer>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (stats == null)
            stats = GetComponent<SlimeKingStats>();
    }

    void OnEnable()
    {
        stats.OnRampage3.AddListener(StartRampage);
        stats.OnRampage4.AddListener(StartRampage);
    }

    void OnDisable()
    {
        stats.OnRampage3.RemoveListener(StartRampage);
        stats.OnRampage4.RemoveListener(StartRampage);
    }

    void Update()
    {
        if (state == State.Chase)
            DoChase();
    }

    private void DoChase()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackARange && canAttackA)
        {
            StartCoroutine(PerformAttackA());
            return;
        }
        else if (dist <= attackBRange && canAttackB)
        {
            StartCoroutine(PerformAttackB());
            return;
        }

        Vector2 dir = (player.position - transform.position).normalized;
        Vector2 vel = rb.linearVelocity;
        vel.x = dir.x * chaseSpeed;
        rb.linearVelocity = vel;
        anim.SetBool("isMoving", true);
        sr.flipX = dir.x < 0;
    }

    private IEnumerator PerformAttackA()
    {
        state = State.AttackA;
        canAttackA = false;
        anim.SetTrigger("AttackA");
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(attackACooldown);

        if (Vector2.Distance(transform.position, player.position) <= attackARange)
            player.GetComponent<PlayerStats>()?.TakeDamage(attackADamage);

        canAttackA = true;
        state = State.Chase;
    }

    private IEnumerator PerformAttackB()
    {
        state = State.AttackB;
        canAttackB = false;
        anim.SetTrigger("AttackB");
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(attackBCooldown);

        if (Vector2.Distance(transform.position, player.position) <= attackBRange)
            player.GetComponent<PlayerStats>()?.TakeDamage(attackBDamage);

        canAttackB = true;
        state = State.Chase;
    }

    public void StartRampage()
    {
        if (state != State.Rampage)
            StartCoroutine(RampageRoutine());
    }

    private IEnumerator RampageRoutine()
    {
        state = State.Rampage;
        anim.SetTrigger("Rage");
        yield return new WaitForSeconds(rageAnimationDuration);

        for (int i = 0; i < rampageJumpCount; i++)
        {
            if (i > 0)
                yield return new WaitUntil(() =>
                    Physics2D.OverlapCircle(groundCheck.position,
                                            groundCheckRadius,
                                            groundLayer)
                );

            Vector2 dir = (player.position - transform.position).normalized;
            rb.AddForce(
                new Vector2(dir.x * rampageHorizontalForce,
                            rampageVerticalForce),
                ForceMode2D.Impulse
            );
        }

        state = State.Chase;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (state == State.Rampage && col.gameObject.CompareTag("Player"))
        {
            col.gameObject.GetComponent<PlayerStats>()?
                .TakeDamage(rampageContactDamage);
        }
    }
}