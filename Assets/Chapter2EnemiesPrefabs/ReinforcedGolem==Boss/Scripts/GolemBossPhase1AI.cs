using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class GolemBossPhase1AI : MonoBehaviour
{
    public enum State { Chase, MeleeAttack,BulletAttack ,BulletHell, Death }
    public State state = State.Chase;

    [Header("References")]
    [Tooltip("Player Transform")]
    public Transform player;

    [Header("Chase & Melee")]
    public float chaseSpeed    = 3f;
    public float meleeRange    = 2f;
    public float meleeCooldown = 1f;
    public int   meleeDamage   = 30;
    private bool  canMelee     = true;

    [Header("Bullet Attack")]
    public float bulletAttackRange = 8f;
    public float bulletAttackCooldown = 5f;
    private bool canBulletAttack = true;

    [Header("Bullet Hell")]
    public float detectDuration  = 0.5f;
    public Transform[] bulletWaypoints; // assign exactly 2
    [Tooltip("Run speed toward selected waypoint")]
    public float runSpeed        = 4f;
    [Tooltip("Time between bullets")]
    public float bulletInterval  = 1.5f;
    [Tooltip("Bullet prefab to fire")]
    public GameObject bulletPrefab;
    [Tooltip("Where bullets spawn from")]
    public Transform bulletSpawn;
    
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
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;
    }

    void Update()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        switch (state)
        {
            case State.Chase:
                ChaseUpdate(dist);
                break;
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
        if (dist >= bulletAttackRange && canBulletAttack)
        {
            StartCoroutine(BulletAttack());
            //return;
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
        AudioManager.instance.PlayAt("GolemBossAttack", gameObject);

        yield return new WaitForSeconds(meleeCooldown);
        if (state == State.BulletHell)
        {
            canMelee = true;
        }
        else
        { 
            canMelee = true;
            state    = State.Chase;
        }
    }

    private IEnumerator BulletAttack()
    {
        state = State.BulletAttack;
        canBulletAttack = false;
        anim.SetBool("isMoving", false);
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        anim.SetTrigger("FireBullet");
        AudioManager.instance.PlayAt("GolemBullet", gameObject);
        yield return new WaitForSeconds(0.2f);

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
        yield return new WaitForSeconds(0.5f);
        state = State.Chase;

        yield return new WaitForSeconds(bulletAttackCooldown);
        if (state == State.BulletHell)
        {
            canBulletAttack = true;
        }
        else
        {
            canBulletAttack = true;
        }
    }
    
    public void StartBulletHell()
    {
        if (state == State.BulletHell) return;
        StartCoroutine(BulletHellSequence());
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
            while (Vector2.Distance(transform.position, targetWP.position) > 1f)
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
            AudioManager.instance.PlayAt("GolemBullet", gameObject);
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