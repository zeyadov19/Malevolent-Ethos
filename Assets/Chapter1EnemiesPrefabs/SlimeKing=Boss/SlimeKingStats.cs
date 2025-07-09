using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class SlimeKingStats : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [Tooltip("Starting HP for Phase 1.")]
    public int maxHealth = 500;
    [HideInInspector] public int currentHealth;

    [Header("Damage Flash")]
    private SpriteRenderer sr;
    private Color originalColor;
    public float flashDuration = 1.5f;
    public float flashInterval = 0.05f;

    [Header("Phase & Rampage Thresholds")]
    public int rampage1Threshold = 400;   // Phase1 rampage #1
    public int rampage1ThresholdPhase2 = 350; // Phase2 rampage #1
    public int rampage2Threshold = 300;   // Phase1 rampage #2
    public int phase2Threshold = 250;   // switch to Phase2
    public int rampage3Threshold = 200;   // Phase2 rampage #1
    public int rampage4Threshold = 100;   // Phase2 rampage #2

    [Header("Events")]
    public UnityEvent OnRampage1;
    public UnityEvent OnRampage2;
    public UnityEvent On1Rampage3;
    public UnityEvent OnPhase2;
    public UnityEvent OnRampage3;
    public UnityEvent OnRampage4;
    public UnityEvent OnDeath;
    private Animator anim;
    public GameObject wall;
    private ArenaCameraSwitcher arenaCameraSwitcher;

    void Awake()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
        anim = GetComponent<Animator>();
        arenaCameraSwitcher = FindFirstObjectByType<ArenaCameraSwitcher>();
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        AudioManager.instance.PlayAt("SlimeHurt", gameObject);
        StartCoroutine(DamageFlash());
        //Debug.Log($"Slime King took {amount} damage. Current health: {currentHealth}");

        if (currentHealth <= rampage1Threshold)
        {
            OnRampage1.Invoke();
            rampage1Threshold = int.MinValue;
        }
        if (currentHealth <= rampage2Threshold)
        {
            OnRampage2.Invoke();
            rampage2Threshold = int.MinValue;
        }
        if (currentHealth <= rampage1ThresholdPhase2)
        {
            On1Rampage3.Invoke();
            rampage1ThresholdPhase2 = int.MinValue;
        }
        if (currentHealth <= phase2Threshold)
        {
            OnPhase2.Invoke();
            phase2Threshold = int.MinValue;
        }
        if (currentHealth <= rampage3Threshold)
        {
            OnRampage3.Invoke();
            rampage3Threshold = int.MinValue;
        }
        if (currentHealth <= rampage4Threshold)
        {
            OnRampage4.Invoke();
            rampage4Threshold = int.MinValue;
        }
        if (currentHealth <= 0)
        {
            OnDeath.Invoke();
        }
    }

    private IEnumerator DamageFlash()
    {
        float timer = 0f;
        while (timer < flashDuration)
        {
            sr.color = Color.gray;
            yield return new WaitForSeconds(flashInterval);
            sr.color = originalColor;
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval * 2f;
        }
        sr.color = originalColor;
    }

    public void onDeath()
    {
        // Handle death logic here, e.g., play death animation, disable the boss, etc.
        AudioManager.instance.PlayAt("SlimeBossDeath", gameObject);
        anim.SetTrigger("Death");
        arenaCameraSwitcher.SwitchBack();
        wall.SetActive(false);
        Destroy(gameObject,2f);
        //Debug.Log("Slime King has been defeated!");
    }
}
