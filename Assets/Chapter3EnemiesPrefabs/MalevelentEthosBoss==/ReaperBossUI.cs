using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReaperBossUI : MonoBehaviour
{
    [Header("References")]
    public ReaperStats ReaperStats;
    public Slider hpSlider;

    //[Header("Value Texts")]
    //public TextMeshProUGUI hpText;
    //public TextMeshProUGUI staminaText;

    void Start()
    {
        if (ReaperStats == null)
        {
            ReaperStats = FindFirstObjectByType<ReaperStats>();
        }

        hpSlider.maxValue = ReaperStats.maxHealth;
        hpSlider.value = ReaperStats.currentHealth;
    }
    
    void Update()
    {
        hpSlider.value = ReaperStats.currentHealth;
    }
}
