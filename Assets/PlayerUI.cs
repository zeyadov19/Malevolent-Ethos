using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;
    public Slider hpSlider;
    public Slider staminaSlider;

    [Header("Value Texts")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI staminaText;

    void Start()
    {
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
        }

        hpSlider.maxValue = playerStats.maxHealth;
        hpSlider.value = playerStats.currentHealth;

        staminaSlider.maxValue = playerStats.maxStamina;
        staminaSlider.value = playerStats.currentStamina;
    }

    void Update()
    {
        hpSlider.value = playerStats.currentHealth;
        staminaSlider.value = playerStats.currentStamina;

        hpText.text      = $"{playerStats.currentHealth}/{playerStats.maxHealth}";
        staminaText.text = $"{playerStats.currentStamina}/{playerStats.maxStamina}";
    }
}
