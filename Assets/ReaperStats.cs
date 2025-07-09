using UnityEngine;
using UnityEngine.Events;
using System.Collections;

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

    // internal flags to ensure each only fires once
    bool phase1Fired = false;
    bool phase2Fired = false;
    bool wipeFired = false;
    bool phase3Fired = false;
    bool deathFired = false;

    void Awake()
    {
        currentHealth = maxHealth;
        reaperAI = GetComponent<ReaperAI>();
        sr = GetComponent<SpriteRenderer>();
        eb = GetComponent<Rigidbody2D>();
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
