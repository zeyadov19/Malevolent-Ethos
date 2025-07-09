using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class ReaperAI : MonoBehaviour
{
    private enum State { Chase, Attack, SummonPhase }
    private State state = State.Chase;

    [Header("Stats & Movement")]
    public ReaperStats stats;
    public Transform player;
    public PlayerStats ps;
    public float chaseSpeed = 5f;
    public float meleeRange = 2f;
    public float meleeCooldown = 1f;
    public int meleeDamage = 25;
    private bool canMelee = true;

    [Header("Teleport & Anim")]
    public Transform teleportPoint;
    public float vanishDuration = 1f;
    public float appearDuration = 1f;

    [Header("Phases armies")]
    public GameObject Army1;
    public GameObject Army2;
    public GameObject Army3;
    public GameObject Army4;
    public GameObject ArmyFinal;

    [Header("Phase 2: Skull Waves")]
    public Transform[] skullSpawnPoints;
    public GameObject skullPrefab;
    public float skullSpawnInterval = 5f;

    [Header("Wipe Phase")]
    private float wipeTimer = 30f;
    private bool wipeTimerActive = false;
    public TextMeshProUGUI wipeTimerText;
    public GameObject WipeTimerUI;

    [Header("Escape")]
    public Transform[] EscapePoints;
    private bool FinalPhaseActive = false;
    private bool ArmySummoned = false;



    Animator anim;
    Rigidbody2D rb;
    SpriteRenderer sr;
    Collider2D coll;
    BoxCollider2D boxColl;

    void Awake()
    {
        if (stats == null)
            stats = GetComponent<ReaperStats>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
        boxColl = GetComponent<BoxCollider2D>();
        ps = player.GetComponent<PlayerStats>();

    }

    void Start()
    {
        AudioManager.instance.PlayAt("ReaperStart",gameObject);
    }

    void Update()
    {
        if (wipeTimerActive)
        {
            wipeTimerText.text = "Wipe in: " + Mathf.CeilToInt(wipeTimer).ToString();
            WipeTimerUI.SetActive(true);
            wipeTimer -= Time.deltaTime;
            if (wipeTimer <= 0f)
            {
                ps.TakeDamage(200);
                Debug.Log("Wipe Phase 1 triggered, player took 200 damage");
                wipeTimer = 30f; // reset timer
                wipeTimerActive = false; // stop the timer
                WipeTimerUI.SetActive(false); // hide the UI
            }
        }

        if (FinalPhaseActive)
        {
            StartCoroutine(FinalPhase());
        }

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

        Vector2 boxOffset = boxColl.offset;
        boxOffset.x = Mathf.Abs(boxOffset.x) * (sr.flipX ? -1 : 1);
        boxColl.offset = boxOffset;

    }

    IEnumerator MeleeAttack()
    {
        state = State.Attack;
        canMelee = false;
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isMoving", false);
        anim.SetTrigger("Attack");
        AudioManager.instance.PlayAt("ReaperAttack", gameObject);

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
        rb.linearVelocity = Vector2.zero;
        coll.enabled = false;
    }

    IEnumerator SummonArmyPhase1()
    {
        state = State.SummonPhase;
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
            yield return new WaitForSeconds(1f);
            if (GameObject.FindGameObjectsWithTag("Summons").Length == 0)
                break;
        }

        anim.SetBool("Summoning", false);
        yield return new WaitForSeconds(0.5f);
        coll.enabled = true;
        state = State.Chase;
    }

    public void OnSummonArmyPhase2()
    {
        StartCoroutine(SummonArmyPhase2());
        rb.linearVelocity = Vector2.zero;
        coll.enabled = false;
    }

    IEnumerator SummonArmyPhase2()
    {
        state = State.SummonPhase;
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
        Army2.SetActive(true);

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
        coll.enabled = true;
        state = State.Chase;
    }

    public void OnWipePhase1()
    {
        StartCoroutine(WipePhase1());
        rb.linearVelocity = Vector2.zero;
        coll.enabled = false;
    }

    IEnumerator WipePhase1()
    {
        state = State.SummonPhase;
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
        Army3.SetActive(true);
        wipeTimerActive = true;

        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if (GameObject.FindGameObjectsWithTag("Summons").Length == 0)
                break;
        }
        wipeTimerActive = false;
        WipeTimerUI.SetActive(false);
        wipeTimer = 30f;
        anim.SetBool("Summoning", false);

        yield return new WaitForSeconds(1f);

        coll.enabled = true;
        state = State.Chase;
    }

    public void OnWipePhase2()
    {
        StartCoroutine(WipePhase2());
        rb.linearVelocity = Vector2.zero;
        coll.enabled = false;
    }

    IEnumerator WipePhase2()
    {
        state = State.SummonPhase;
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
        Army4.SetActive(true);
        wipeTimerActive = true;

        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if (GameObject.FindGameObjectsWithTag("Summons").Length == 0)
                break;
        }
        wipeTimerActive = false;
        WipeTimerUI.SetActive(false);
        wipeTimer = 30f;
        anim.SetBool("Summoning", false);

        yield return new WaitForSeconds(1f);

        coll.enabled = true;
        state = State.Chase;
    }
    
    public void OnFinalPhase()
    {
        FinalPhaseActive = true;
    }

    IEnumerator FinalPhase()
    {
        if (!ArmySummoned)
            anim.SetBool("Summoning", true);
         
        // Find the furthest escape point from the player
        Transform furthestPoint = null;
        float maxDist = float.MinValue;
        foreach (var point in EscapePoints)
        {
            float dist = Vector2.Distance(player.position, point.position);
            if (dist > maxDist)
            {
                maxDist = dist;
                furthestPoint = point;
            }
        }

        if (furthestPoint != null)
        {
            // Stop moving if reached the escape point
            float distanceToPoint = Vector2.Distance(transform.position, furthestPoint.position);
            if (distanceToPoint <= 0.1f)
            {
                rb.linearVelocity = Vector2.zero;
                anim.SetBool("isMoving", false);
                // Optionally, set state or trigger next phase here
            }
            else
            {
                state = State.SummonPhase;
                // Move towards the furthest escape point normally
                Vector2 direction = (furthestPoint.position - transform.position).normalized;
                rb.linearVelocity = direction * chaseSpeed;
                anim.SetBool("isMoving", true);
                sr.flipX = direction.x < 0f;
            }
            yield return new WaitForSeconds(0.5f);
            ArmyFinal.SetActive(true);
            anim.SetBool("Summoning", false);
            ArmySummoned = true;

        }
    }
}
