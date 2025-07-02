// ArenaCameraSwitcher.cs
using UnityEngine;
using Unity.Cinemachine;

public class ArenaCameraSwitcher : MonoBehaviour
{
    [Header("Virtual Cameras")]
    public CinemachineCamera mainCam;
    public CinemachineCamera bossCam;

    [Header("Boss Reference")]
    public GameObject SlimeKing;
    public SlimeKingStats bossStats;
    public GameObject BossUI;

    bool hasSwitched = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasSwitched && other.CompareTag("Player"))
        {
            SlimeKing.SetActive(true);
            BossUI.SetActive(true);

            bossCam.Priority = mainCam.Priority + 1;
            hasSwitched = true;

            bossStats.OnDeath.AddListener(SwitchBack);
        }
    }

    void SwitchBack()
    {
        // restore priorities
        bossCam.Priority = mainCam.Priority - 1;
        // (optional) unsubscribe if you like:
        bossStats.OnDeath.RemoveListener(SwitchBack);
    }
}
