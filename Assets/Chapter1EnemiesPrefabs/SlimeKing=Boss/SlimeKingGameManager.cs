using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class SlimeKingGameManager : MonoBehaviour
{
    [Tooltip("Attach your SlimeKingStats here")]
    public SlimeKingStats stats;
    [Tooltip("Phase1 AI component")]
    public SlimeKingPhase1AI phase1;
    [Tooltip("Phase2 AI component")]
    public SlimeKingPhase2AI phase2;

    public GameObject dashUnlockText;
    private GameObject player;

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

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

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
        StartCoroutine(DashChange());

    }
    
    IEnumerator DashChange()
    {
        if (player != null)
        {
            player.GetComponent<DashAbility>().enabled = false;
            player.GetComponent<ShadowDash>().enabled = true;
            yield return new WaitForSeconds(2f);
            dashUnlockText.SetActive(true);
            TextMeshProUGUI text = dashUnlockText.GetComponent<TextMeshProUGUI>();
            Color startColor = text.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f);
            float duration = 1f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                text.color = Color.Lerp(startColor, endColor, elapsed / duration);
                yield return null;
            }
            text.color = endColor;
            yield return new WaitForSeconds(12f);
            // Fade out text alpha smoothly
            startColor = text.color;
            endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                text.color = Color.Lerp(startColor, endColor, elapsed / duration);
                yield return null;
            }
            text.color = endColor;
            dashUnlockText.SetActive(false);
        }
    }
}
