using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(SpriteRenderer))]
public class SlimeKingPhase1AI : MonoBehaviour
{
    [Header("References")]
    public SlimeKingStats stats;
    public Transform      player;

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
    public float hoverDuration = 1f;
    public float slamForce = 15f;
    public float postSlamDelay = 0.5f;


    private Rigidbody2D rb;
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

    void Start()
    {
        StartCoroutine(PlayEntrance());
    }

    private IEnumerator PlayEntrance()
    {
        yield return new WaitForSeconds(0.5f);
        AudioManager.instance.PlayAt("SlimeBossStart", gameObject);
        yield return new WaitForSeconds(0.2f);
        AudioManager.instance.PlayAt("SlimeBossWalk", gameObject);
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
        {
            DoChase();
        }
        
        //Debug.Log($"Current State: {state}");
    }

    private void DoChase()
    {
        if (state == State.Rampage)
            return;

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
        anim.SetBool("isChasing", true);
        sr.flipX = dir.x < 0;
        if (state == State.Chase)
            return;
    }

    private IEnumerator PerformAttackA()
    {
        state = State.AttackA;
        canAttack = false;
        anim.SetBool("isChasing",false);
        AudioManager.instance.StopAt("SlimeBossWalk",gameObject);
        anim.SetTrigger("AttackA");
        anim.SetBool("AttackAA", true);
        AudioManager.instance.PlayAt("SlimeKingAttack",gameObject);
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(1f); // wait for the attack animation to start
        anim.SetBool("AttackAA", false);
        yield return new WaitForSeconds(attackACooldown);

        // if (Vector2.Distance(transform.position, player.position) <= attackARange)
        //     player.GetComponent<PlayerStats>()?.TakeDamage(attackADamage);
        if (state == State.Rampage)
        {
            canAttack = true;
        }
        else
        {
            canAttack = true;
            AudioManager.instance.PlayAt("SlimeBossWalk",gameObject);
            state = State.Chase;
        }

    }

    public void StartRampage()
    {
        if (state != State.Rampage)
            StartCoroutine(RampageRoutine());
    }

    private IEnumerator RampageRoutine()
    {
        state = State.Rampage;
        anim.SetBool("isChasing", false);
        AudioManager.instance.StopAt("SlimeBossWalk",gameObject);
        anim.SetBool("AttackAA", false);
        anim.SetTrigger("Idle");
        gameObject.layer = LayerMask.NameToLayer("SlamAttack");

        // stop any chase motion
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        anim.SetTrigger("Rage");
        AudioManager.instance.PlayAt("SlimeKingAttackLoop",gameObject);
        yield return new WaitForSeconds(rageAnimationDuration);
        AudioManager.instance.StopAt("SlimeKingAttackLoop",gameObject);

        for (int i = 0; i < rampageJumpCount; i++)
        {
            // 1) Leap toward the player
            anim.SetTrigger("Jump");
            AudioManager.instance.PlayAt("SlimeKingJump",gameObject);
            yield return new WaitForSeconds(0.3f);

            Vector2 dir = (player.position - transform.position).normalized;
            rb.AddForce(new Vector2(dir.x * rampageHorizontalForce, rampageVerticalForce), ForceMode2D.Impulse);

            // 2) Wait until roughly above the player or 1 second passes
            float timer = 0f;
            while (Mathf.Abs(transform.position.x - player.position.x) >= 0.4f && timer < 0.9f)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            // anim.SetTrigger("Hover");

            // 3) Zero horizontal speed to hover in place
            Vector2 v = rb.linearVelocity;
            rb.linearVelocity = new Vector2(0f, v.y);
            // 4) Hover
            yield return new WaitForSeconds(hoverDuration);

            // 5) Slam down
            rb.AddForce(Vector2.down * slamForce, ForceMode2D.Impulse);
            anim.SetTrigger("Slam");
            AudioManager.instance.PlayAt("SlimeKingSlam",gameObject);

            yield return new WaitForSeconds(0.5f);
            // 6) Do attack B
            //anim.SetTrigger("AttackB");
            anim.SetTrigger("Idle");

            // 7) Pause before next jump
            yield return new WaitForSeconds(postSlamDelay);
            //Debug.Log($"Rampage jump {i + 1} completed.");
        }

        state = State.Chase;
        AudioManager.instance.PlayAt("SlimeBossWalk",gameObject);
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        //Debug.Log("Rampage finished, returning to chase state.");
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (state == State.Rampage && col.gameObject.CompareTag("Player"))
            col.gameObject.GetComponent<PlayerStats>()
               ?.TakeDamage(rampageContactDamage);
    }
}
