// GolemBossStats.cs
using UnityEngine;
using UnityEngine.Events;

public class GolemBossStats : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 500;
    [HideInInspector] public int currentHealth;

    [Header("Phase Events")]
    public UnityEvent OnBulletHell1;  // fires at <= 400HP
    public UnityEvent OnBulletHell2;  // fires at <= 300HP
    public UnityEvent OnDeath;        // fires at 0HP

    private bool hasHell1Fired = false;
    private bool hasHell2Fired = false;

    void Awake()
    {
        currentHealth = maxHealth;
        // ensure events are non-null
        OnBulletHell1 = OnBulletHell1 ?? new UnityEvent();
        OnBulletHell2 = OnBulletHell2 ?? new UnityEvent();
        OnDeath      = OnDeath      ?? new UnityEvent();
    }

    /// <summary>
    /// IDamageable API
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;

        if (!hasHell1Fired && currentHealth <= 400)
        {
            hasHell1Fired = true;
            OnBulletHell1.Invoke();
        }

        if (!hasHell2Fired && currentHealth <= 300)
        {
            hasHell2Fired = true;
            OnBulletHell2.Invoke();
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath.Invoke();
        }
    }
}
