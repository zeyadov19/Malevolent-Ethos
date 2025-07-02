using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlimeKingUI : MonoBehaviour
{
    [Header("References")]
    public SlimeKingStats SlimeKingStats;
    public Slider hpSlider;

    //[Header("Value Texts")]
    //public TextMeshProUGUI hpText;
    //public TextMeshProUGUI staminaText;

    void Start()
    {
        if (SlimeKingStats == null)
        {
            SlimeKingStats = FindFirstObjectByType<SlimeKingStats>();
        }

        hpSlider.maxValue = SlimeKingStats.maxHealth;
        hpSlider.value = SlimeKingStats.currentHealth;
    }
    
    void Update()
    {
        hpSlider.value = SlimeKingStats.currentHealth;
    }
}
