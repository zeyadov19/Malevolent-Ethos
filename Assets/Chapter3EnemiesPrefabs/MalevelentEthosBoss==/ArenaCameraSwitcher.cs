using UnityEngine;
using Unity.Cinemachine;

public class ArenaCameraSwitcher : MonoBehaviour
{
    [Header("Virtual Cameras")]
    public CinemachineCamera mainCam;
    public CinemachineCamera bossCam;

    [Header("Boss Reference")]
    public GameObject Boss;
    public SlimeKingStats bossStats;
    public GameObject BossUI;

    [Header("BG")]
    public GameObject BG;

    bool hasSwitched = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasSwitched && other.CompareTag("Player"))
        {
            Boss.SetActive(true);
            BossUI.SetActive(true);

            bossCam.Priority = mainCam.Priority + 1;
            hasSwitched = true;
            BG.transform.localScale = new Vector3(2f, 2f, 1f);

            bossStats.OnDeath.AddListener(SwitchBack);
        }
    }

    void SwitchBack()
    {
        // restore priorities
        bossCam.Priority = mainCam.Priority - 1;
        BG.transform.localScale = new Vector3(1f, 1f, 1f);
        // (optional) unsubscribe if you like:
        bossStats.OnDeath.RemoveListener(SwitchBack);
    }
}
