using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class GolemBossAI : MonoBehaviour, IDamageable
{
    private enum State { Chase, MeleeAttack, BulletHell, Death }
    private State state = State.Chase;

    [Header("References")]
    [Tooltip("Player Transform")]
    public Transform player;

    [Header("Health")]
    public int maxHealth = 500;
    private int currentHealth;
    private float flashDuration = 1.5f;
    private float flashInterval = 0.05f;
    //private bool usedHell400 = false;
    //private bool usedHell300 = false;

    [Header("Chase & Melee")]
    public float chaseSpeed    = 3f;
    public float meleeRange    = 2f;
    public float meleeCooldown = 1f;
    public int   meleeDamage   = 30;
    private bool  canMelee     = true;

    [Header("Bullet Hell")]
    [Tooltip("Animation time for slam ground & stun")]
    public float detectDuration  = 0.5f;
    [Tooltip("Two waypoints for bullet hell run")]
    public Transform[] bulletWaypoints; // assign exactly 2
    [Tooltip("Run speed toward selected waypoint")]
    public float runSpeed        = 4f;
    [Tooltip("Time between bullets")]
    public float bulletInterval  = 1.5f;
    [Tooltip("Bullet prefab to fire")]
    public GameObject bulletPrefab;
    [Tooltip("Where bullets spawn from")]
    public Transform bulletSpawn;

    [Header("Death")]
    public float deathDelay = 1f;

    // internals
    private Rigidbody2D  rb;
    private Animator     anim;
    private SpriteRenderer sr;
    private Collider2D   col;

    void Awake()
    {
        rb  = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();
        col  = GetComponent<Collider2D>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;
    }

    void Update()
    {
        if (state == State.Death) return;

        float dist = Vector2.Distance(transform.position, player.position);
        switch (state)
        {
            case State.Chase:
                ChaseUpdate(dist);
                break;
            // MeleeAttack and BulletHell run their own coroutines
        }
    }

    private void ChaseUpdate(float dist)
    {
        anim.SetBool("isMoving", true);

        // melee attack
        if (dist <= meleeRange && canMelee)
        {
            StartCoroutine(MeleeCoroutine());
            return;
        }

        // chase player horizontally
        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        Vector2 vel = rb.linearVelocity;
        vel.x       = dir.x * chaseSpeed;
        rb.linearVelocity = vel;
        sr.flipX    = dir.x < 0;
    }

    private IEnumerator MeleeCoroutine()
    {
        state      = State.MeleeAttack;
        canMelee   = false;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        anim.SetTrigger("Melee");

        yield return new WaitForSeconds(meleeCooldown);

        // damage is handled via animation-triggered collider
        canMelee = true;
        state    = State.Chase;
    }

    private IEnumerator BulletHellSequence()
    {
        state = State.BulletHell;

        // 1) Slam ground & stun player
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        anim.SetTrigger("SlamGround");
        // disable player movement
        var playerStats = player.GetComponent<PlayerStats>();
        if (playerStats != null)
            playerStats.EnterStun();

        yield return new WaitForSeconds(detectDuration);
        gameObject.layer = LayerMask.NameToLayer("Untouchable");

        // 2) Run to the further-away waypoint
        if (bulletWaypoints != null && bulletWaypoints.Length == 2)
        {
            // pick waypoint farthest from player
            float d0 = Vector2.Distance(player.position, bulletWaypoints[0].position);
            float d1 = Vector2.Distance(player.position, bulletWaypoints[1].position);
            Transform targetWP = d0 > d1 ? bulletWaypoints[0] : bulletWaypoints[1];

            anim.SetBool("isMoving", true);
            while (Vector2.Distance(transform.position, targetWP.position) > 0.3f)
            {
                Vector2 dir = ((Vector2)targetWP.position - (Vector2)transform.position).normalized;
                rb.linearVelocity = new Vector2(dir.x * runSpeed, rb.linearVelocity.y);
                sr.flipX = dir.x < 0;
                yield return null;
            }
        }

        gameObject.layer = LayerMask.NameToLayer("Enemy");
        // reached waypoint – stop moving
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        anim.SetBool("isMoving", false);

        // face the player
        sr.flipX = (player.position.x < transform.position.x);
        if (playerStats != null)
            playerStats.ExitStun();

        // 3) Fire bullets every interval until boss is hit
        while (state == State.BulletHell)
        {
            yield return new WaitForSeconds(bulletInterval);
            if (state != State.BulletHell) break;

            anim.SetTrigger("FireBullet");
            if (bulletPrefab != null && bulletSpawn != null)
            {
                GameObject b = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
                if (sr.flipX)
                {
                    Vector3 s = b.transform.localScale;
                    s.x *= -1;
                    b.transform.localScale = s;
                }
            }
        }

        // 4) return to normal chase/melee
        state = State.Chase;
    }

    public void TakeDamage(int amount)
    {
        if (state == State.Death) return;

        currentHealth -= amount;
        StartCoroutine(DamageFlash());
        anim.SetTrigger("Hurt");

        // if hit during bullet hell, exit it immediately
        if (state == State.BulletHell)
            state = State.Chase;

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
            sr.color = Color.white;
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval * 2f;
        }
        sr.color = Color.white;
    }

    private IEnumerator DieRoutine()
    {
        state = State.Death;
        anim.SetTrigger("Death");
        yield return new WaitForSeconds(deathDelay);

        rb.simulated = false;
        col.enabled = false;
        Destroy(gameObject, 1f);
    }

    void OnDrawGizmosSelected()
    {
        // melee range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
        // bullet hell detect range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectDuration); // or use detectRange if you have one
    }
}
