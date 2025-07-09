using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Unity.VisualScripting;

public class ReaperStats : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 500;
    public float flashDuration = 0.5f;
    public float flashInterval = 0.05f;
    [HideInInspector] public int currentHealth;
    private ReaperAI reaperAI;
    private SpriteRenderer sr;
    private Rigidbody2D eb;
    private Animator anim;
    private BossArena bossArena;

    // internal flags to ensure each only fires once
    bool phase1Fired = false;
    bool phase2Fired = false;
    bool wipe1Fired = false;
    bool wipe2Fired = false;
    bool finalPhaseFired = false;

    void Awake()
    {
        currentHealth = maxHealth;
        reaperAI = GetComponent<ReaperAI>();
        sr = GetComponent<SpriteRenderer>();
        eb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        bossArena = FindFirstObjectByType<BossArena>();
    }


    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        
        StartCoroutine(DamageFlash());

        if (currentHealth <= 450 && !phase1Fired)
        {
            phase1Fired = true;
            reaperAI.OnSummonArmyPhase1();
        }
        if (currentHealth <= 350 && !phase2Fired)
        {
            phase2Fired = true;
            reaperAI.OnSummonArmyPhase2();
        }
        if (currentHealth <= 250 && !wipe1Fired)
        {
            wipe1Fired = true;
            reaperAI.OnWipePhase1();
        }
        if (currentHealth <= 150 && !wipe2Fired)
        {
            wipe2Fired = true;
            reaperAI.OnWipePhase2();
        }
        if (currentHealth <= 123 && !finalPhaseFired)
        {
            finalPhaseFired = true;
            reaperAI.OnFinalPhase();
        }
        {
            
        }
        if (currentHealth <= 0)
        {
            StartCoroutine(OnDeath());
        }
        

    }

    IEnumerator OnDeath()
    {
        bossArena.SwitchBack();
        yield return new WaitForSeconds(0.5f);
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
}
