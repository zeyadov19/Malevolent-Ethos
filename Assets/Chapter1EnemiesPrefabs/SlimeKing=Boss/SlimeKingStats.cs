using UnityEngine;
using UnityEngine.Events;

public class SlimeKingStats : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [Tooltip("Starting HP for Phase 1.")]
    public int maxHealth = 500;
    [HideInInspector] public int currentHealth;

    [Header("Phase & Rampage Thresholds")]
    public int rampage1Threshold = 400;   // Phase1 rampage #1
    public int rampage2Threshold = 300;   // Phase1 rampage #2
    public int phase2Threshold   = 250;   // switch to Phase2
    public int rampage3Threshold = 200;   // Phase2 rampage #1
    public int rampage4Threshold = 100;   // Phase2 rampage #2

    [Header("Events")]
    public UnityEvent OnRampage1;  
    public UnityEvent OnRampage2;  
    public UnityEvent OnPhase2;    
    public UnityEvent OnRampage3;  
    public UnityEvent OnRampage4;  
    public UnityEvent OnDeath;     

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        Debug.Log($"Slime King took {amount} damage. Current health: {currentHealth}");

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
}
