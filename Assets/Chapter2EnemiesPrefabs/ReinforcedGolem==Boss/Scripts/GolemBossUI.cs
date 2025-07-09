using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GolemBossUI : MonoBehaviour
{
    [Header("References")]
    public GolemBossStats GolemBossStats;
    public Slider hpSlider;

    //[Header("Value Texts")]
    //public TextMeshProUGUI hpText;
    //public TextMeshProUGUI staminaText;

    void Start()
    {
        if (GolemBossStats == null)
        {
            GolemBossStats = FindFirstObjectByType<GolemBossStats>();
        }

        hpSlider.maxValue = GolemBossStats.maxHealth;
        hpSlider.value = GolemBossStats.currentHealth;
    }
    
    void Update()
    {
        hpSlider.value = GolemBossStats.currentHealth;
    }
}
