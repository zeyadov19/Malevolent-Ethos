using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

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
            AudioManager.instance.Stop("BGM");
            AudioManager.instance.Play("BossBGM");

            bossCam.Priority = mainCam.Priority + 1;
            hasSwitched = true;
            BG.transform.localScale = new Vector3(2f, 2f, 1f);

            bossStats.OnDeath.AddListener(SwitchBack);
        }
    }

    public void SwitchBack()
    {
        // restore priorities
        bossCam.Priority = mainCam.Priority - 1;
        StartCoroutine(BGSize());
        BG.transform.localScale = new Vector3(1f, 1f, 1f);
        AudioManager.instance.Stop("BossBGM");
        AudioManager.instance.Play("BGM");
        BossUI.SetActive(false);

        // (optional) unsubscribe if you like:
        bossStats.OnDeath.RemoveListener(SwitchBack);
    }

    IEnumerator BGSize()
    {
        BG.transform.localScale = new Vector3(2f, 2f, 1f);
        yield return new WaitForSeconds(0.1f);
        BG.transform.localScale = new Vector3(1.9f, 1.9f, 1f);
        yield return new WaitForSeconds(0.1f);
        BG.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
        yield return new WaitForSeconds(0.1f);
        BG.transform.localScale = new Vector3(1.7f, 1.7f, 1f);
        yield return new WaitForSeconds(0.1f);
        BG.transform.localScale = new Vector3(1.6f, 1.6f, 1f);
        yield return new WaitForSeconds(0.1f);
        BG.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        yield return new WaitForSeconds(0.1f);
        BG.transform.localScale = new Vector3(1.4f, 1.4f, 1f);
        yield return new WaitForSeconds(0.1f);
        BG.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
        yield return new WaitForSeconds(0.1f);
        BG.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        yield return new WaitForSeconds(0.1f);
        BG.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        yield return new WaitForSeconds(0.1f);
        BG.transform.localScale = new Vector3(1f, 1f, 1f);

    }

}
