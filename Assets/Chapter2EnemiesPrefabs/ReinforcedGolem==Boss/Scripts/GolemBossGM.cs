// // GolemBossGameManager.cs
// using UnityEngine;

// public class GolemBossGM : MonoBehaviour
// {
//     [Header("Boss Components")]
//     //public GolemBossStats        stats;
//     //public GolemBossPhase1AI     phase1AI;
//     //public GolemBossPhase2AI     phase2AI;

//     void Awake()
//     {
//         // Phase2AI should start disabled
//         if (phase2AI != null) phase2AI.enabled = false;

//         // when HP ≤250, swap scripts
//         stats.OnPhase2.AddListener(SwitchToPhase2);
//     }

//     void OnDestroy()
//     {
//         stats.OnPhase2.RemoveListener(SwitchToPhase2);
//     }

//     void SwitchToPhase2()
//     {
//         if (phase1AI != null) phase1AI.enabled = false;
//         if (phase2AI != null) phase2AI.enabled = true;
//     }
// }
