using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(Collider2D))]
public class BonePianistAIonetimeuse : MonoBehaviour, IDamageable
{
    private enum State { Idle, Attack, Death }
    private State state = State.Idle;

    [Header("References")]
    [Tooltip("Where skulls will be spawned from")]
    public Transform summonPoint;
    [Tooltip("Prefab of the flying skull to summon")]
    public GameObject skullPrefab;
    [Tooltip("Player Transform")]
    public Transform player;

    [Header("Detection")]
    [Tooltip("Distance at which the pianist begins/stops summoning")]
    public float detectRange = 10f;

    [Header("Summon Settings")]
    [Tooltip("Seconds between skull spawns")]
    public float spawnInterval = 5f;

    [Header("Health & Death")]
    public int   maxHealth  = 100;
    public float flashDuration = 0.5f;
    public float flashInterval = 0.05f;
    public float deathDelay = 1f;

    Animator     anim;
    Collider2D   col;
    Rigidbody2D  rb;
    SpriteRenderer sr;
    int          currentHealth;
    Coroutine    summonRoutine;

    void Awake()
    {
        anim = GetComponent<Animator>();
        col  = GetComponent<Collider2D>();
        rb   = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Start()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;

        currentHealth = maxHealth;
        //anim.SetTrigger("Idle");
    }

    void Update()
    {
        if (state != State.Idle && state != State.Attack) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (state == State.Idle && dist <= detectRange)
            EnterAttack();
        else if (state == State.Attack && dist > detectRange)
            EnterIdle();
    }

    void EnterIdle()
    {
        state = State.Idle;
        //anim.SetTrigger("Idle");
        if (summonRoutine != null)
        {
            StopCoroutine(summonRoutine);
            summonRoutine = null;
        }
    }

    void EnterAttack()
    {
        state = State.Attack;
        if (summonRoutine != null)
            StopCoroutine(summonRoutine);
        summonRoutine = StartCoroutine(SummonSkulls());
    }

    IEnumerator SummonSkulls()
    {
        while (state == State.Attack)
        {
            if (skullPrefab != null && summonPoint != null)
                Instantiate(skullPrefab, summonPoint.position, summonPoint.rotation);
            anim.SetTrigger("Attack");
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void TakeDamage(int amount)
    {
        if (state == State.Death) return;

        currentHealth -= amount;
        AudioManager.instance.PlayAt("PianistHurt", gameObject);
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
        if (summonRoutine != null)
            StopCoroutine(summonRoutine);

        anim.SetTrigger("Death");
        AudioManager.instance.Stop("BGM");
        col.enabled = false;
        rb.simulated = false;
        Destroy(gameObject, deathDelay);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
