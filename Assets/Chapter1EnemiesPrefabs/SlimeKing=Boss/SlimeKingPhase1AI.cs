// SlimeKingPhase1AI.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class SlimeKingPhase1AI : MonoBehaviour
{
    [Header("References")]
    public SlimeKingStats stats;
    public Transform      player;

    [Header("Ground Check")]
    public Transform  groundCheck;
    public float      groundCheckRadius = 0.2f;
    public LayerMask  groundLayer;

    [Header("Chase & AttackA")]
    public float chaseSpeed      = 3f;
    public float attackARange    = 2.5f;
    public float attackACooldown = 1.5f;
    public int   attackADamage   = 20;

    [Header("Rampage Settings")]
    public float rageAnimationDuration   = 1f;
    public int   rampageJumpCount        = 3;
    public float rampageHorizontalForce = 10f;
    public float rampageVerticalForce   = 7f;
    public int   rampageContactDamage   = 25;

    private Rigidbody2D    rb;
    private Animator       anim;
    private SpriteRenderer sr;
    private bool           canAttack = true;

    private enum State { Chase, AttackA, Rampage }
    private State state = State.Chase;

    void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (stats == null)
            stats = GetComponent<SlimeKingStats>();
    }

    void OnEnable()
    {
        stats.OnRampage1.AddListener(StartRampage);
        stats.OnRampage2.AddListener(StartRampage);
    }

    void OnDisable()
    {
        stats.OnRampage1.RemoveListener(StartRampage);
        stats.OnRampage2.RemoveListener(StartRampage);
    }

    void Update()
    {
        if (state == State.Chase)
            DoChase();
    }

    private void DoChase()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackARange && canAttack)
        {
            StartCoroutine(PerformAttackA());
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
        state     = State.AttackA;
        canAttack = false;
        anim.SetTrigger("AttackA");
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(attackACooldown);

        if (Vector2.Distance(transform.position, player.position) <= attackARange)
            player.GetComponent<PlayerStats>()?.TakeDamage(attackADamage);

        canAttack = true;
        state     = State.Chase;
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
            // Wait until grounded before each jump
            Debug.Log($"Jump {i + 1}/{rampageJumpCount}");
            yield return new WaitForSeconds(3f);

            // Jump toward player
            Vector2 dir = (player.position - transform.position).normalized;
            rb.AddForce(new Vector2(dir.x * rampageHorizontalForce,rampageVerticalForce),ForceMode2D.Impulse);
        }

        state = State.Chase;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (state == State.Rampage && col.gameObject.CompareTag("Player"))
            col.gameObject.GetComponent<PlayerStats>()
               ?.TakeDamage(rampageContactDamage);
    }
}
