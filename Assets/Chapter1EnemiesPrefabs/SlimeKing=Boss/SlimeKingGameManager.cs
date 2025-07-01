using UnityEngine;

public class SlimeKingGameManager : MonoBehaviour
{
    [Tooltip("Attach your SlimeKingStats here")]
    public SlimeKingStats stats;
    [Tooltip("Phase1 AI component")]
    public SlimeKingPhase1AI phase1;
    [Tooltip("Phase2 AI component")]
    public SlimeKingPhase2AI phase2;

    void Awake()
    {
        // Start in Phase1 only
        phase1.enabled = true;
        phase2.enabled = false;

        // Subscribe to thresholds
        stats.OnRampage1.AddListener(phase1.StartRampage);
        stats.OnRampage2.AddListener(phase1.StartRampage);
        stats.OnPhase2.AddListener(EnterPhase2);
        stats.OnRampage3.AddListener(phase2.StartRampage);
        stats.OnRampage4.AddListener(phase2.StartRampage);
        stats.OnDeath.AddListener(HandleDeath);
    }

    public void EnterPhase2()
    {
        phase1.enabled = false;
        phase2.enabled = true;
    }

    public void HandleDeath()
    {
        phase1.enabled = false;
        phase2.enabled = false;
        // optional: play a final death sequence, drop loot, etc.
    }
}
