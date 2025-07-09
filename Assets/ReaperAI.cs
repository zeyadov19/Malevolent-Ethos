using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class ReaperAI : MonoBehaviour
{
    private enum State { Chase, Attack, SummonPhase1, SummonPhase2 }
    private State state = State.Chase;

    [Header("Stats & Movement")]
    public ReaperStats  stats;
    public Transform    player;
    public float        chaseSpeed    = 5f;
    public float        meleeRange    = 2f;
    public float        meleeCooldown = 1f;
    public int          meleeDamage   = 25;
    private bool        canMelee = true;

    [Header("Teleport & Anim")]
    public Transform    teleportPoint;
    public float        vanishDuration = 1f;
    public float        appearDuration = 1f;

    [Header("Phases armies")]
    public GameObject Army1;
    public GameObject Army2;

    [Header("Phase 2: Skull Waves")]
    public Transform[]  skullSpawnPoints;   // length 2
    public GameObject   skullPrefab;
    public float        skullSpawnInterval = 5f;

    Animator     anim;
    Rigidbody2D  rb;
    SpriteRenderer sr;

    void Awake()
    {
        if (stats == null)
            stats = GetComponent<ReaperStats>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
        
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

    }

    void Update()
    {
        if (state != State.Chase) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // melee attack
        if (dist <= meleeRange && canMelee)
        {
            StartCoroutine(MeleeAttack());
            return;
        }

        // chase
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(dir.x * chaseSpeed, dir.y * chaseSpeed);
        anim.SetBool("isMoving", true);
        GetComponent<SpriteRenderer>().flipX = dir.x < 0f;
    }

    IEnumerator MeleeAttack()
    {
        state = State.Attack;
        canMelee = false;
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isMoving", false);
        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(meleeCooldown);

        if (state != State.Attack)
        {
            canMelee = true;
        }
        else
        { 
            canMelee = true;
            state = State.Chase;
        }
        
    }

    public void OnSummonArmyPhase1()
    {
        StartCoroutine(SummonArmyPhase1());
    }

    IEnumerator SummonArmyPhase1()
    {
        state = State.SummonPhase2;
        // Fade out sprite
        float fadeTime = vanishDuration;
        float elapsed = 0f;
        Color originalColor = Color.white;
        while (elapsed < fadeTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        yield return new WaitForSeconds(vanishDuration);

        transform.position = teleportPoint.position;
        // Fade in sprite
        elapsed = 0f;
        while (elapsed < appearDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsed / appearDuration);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        yield return new WaitForSeconds(appearDuration);

        anim.SetBool("Summoning", true);
        Army1.SetActive(true);

        yield return new WaitForSeconds(1f);
        while (true)
        {
            if (GameObject.FindGameObjectsWithTag("Summons").Length == 0)
                break;
        }

        anim.SetBool("Summoning", false);
        yield return new WaitForSeconds(0.5f);
        state = State.Chase;
    }
    
    public void OnSummonArmyPhase2()
    {
        StartCoroutine(SummonArmyPhase2());
    }

    IEnumerator SummonArmyPhase2()
    {
        state = State.SummonPhase2;
        // Fade out sprite
        float fadeTime = vanishDuration;
        float elapsed = 0f;
        Color originalColor = Color.white;
        while (elapsed < fadeTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        yield return new WaitForSeconds(vanishDuration);

        transform.position = teleportPoint.position;
        // Fade in sprite
        elapsed = 0f;
        while (elapsed < appearDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsed / appearDuration);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        yield return new WaitForSeconds(appearDuration);

        anim.SetBool("Summoning", true);

        while (true)
        {
            // spawn two skulls
            foreach (var pt in skullSpawnPoints)
            {
                Instantiate(skullPrefab, pt.position, pt.rotation);
            }

            yield return new WaitForSeconds(skullSpawnInterval);

            // if player killed them all before the next wave, end phase
            if (GameObject.FindGameObjectsWithTag("Summons").Length == 0)
                break;
        }

        anim.SetBool("Summoning", false);
        yield return new WaitForSeconds(0.5f);
        state = State.Chase;
    }
}
