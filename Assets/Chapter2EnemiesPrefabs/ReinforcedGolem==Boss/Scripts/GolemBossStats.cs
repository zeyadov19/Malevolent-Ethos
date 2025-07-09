using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class GolemBossStats : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 500;
    [HideInInspector] public int currentHealth;
    public float flashDuration = 1.5f;
    public float flashInterval = 0.05f;
    public float deathDelay = 1f;

    [Header("References")]
    public Animator anim;
    public SpriteRenderer sr;
    public GolemBossPhase1AI phase1AI;
    public GolemBossPhase2AI phase2AI;
    private bool usedHell400 = false;
    private bool usedHell300 = false;
    private bool usedHell200 = false;
    private bool usedHell100 = false;




    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        //rb = GetComponent<Rigidbody2D>();
        //col = GetComponent<Collider2D>();
        phase1AI = GetComponent<GolemBossPhase1AI>();
        phase2AI = GetComponent<GolemBossPhase2AI>();
    }

    void Update()
    {
        if (currentHealth <= 400 && !usedHell400)
        {
            usedHell400 = true;
            phase1AI.StartBulletHell();
            return;
        }
        if (currentHealth <= 300 && !usedHell300)
        {
            usedHell300 = true;
            phase1AI.StartBulletHell();
            return;
        }
        if (currentHealth <= 250 && phase1AI.enabled)
        {
            phase1AI.enabled = false;
            phase2AI.enabled = true;
        }
        if (currentHealth <= 200 && !usedHell200)
        {
            usedHell200 = true;
            phase2AI.StartBulletHell();
            return;
        }
        if (currentHealth <= 100 && !usedHell100)
        {
            usedHell100 = true;
            phase2AI.StartBulletHell();
            return;
        }
    }

    public void TakeDamage(int amount)
    {
        if (phase1AI.state == GolemBossPhase1AI.State.Death) return;

        currentHealth -= amount;
        StartCoroutine(DamageFlash());
        AudioManager.instance.PlayAt("GolemHurt", gameObject);
        anim.SetTrigger("Hurt");

        // if hit during bullet hell, exit it immediately
        if (phase1AI.state == GolemBossPhase1AI.State.BulletHell)
            phase1AI.state = GolemBossPhase1AI.State.Chase;

        if (phase2AI.state == GolemBossPhase2AI.State.BulletHell)
            phase2AI.state = GolemBossPhase2AI.State.Chase;


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
        anim.SetTrigger("Death");
        yield return new WaitForSeconds(deathDelay);

        //rb.simulated = false;
        //col.enabled = false;
        Destroy(gameObject, 1f);
    }
}
