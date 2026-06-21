using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FightingGameManager : MonoBehaviour
{
    [Header("Characters")]
    public Fighter fighter1;  // LaserGay
    public Fighter fighter2;  // Boxer

    [Header("UI")]
    public Image healthBar1;
    public Image healthBar2;
    public TextMeshProUGUI hpText1;
    public TextMeshProUGUI hpText2;

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (fighter1 != null && healthBar1 != null)
        {
            healthBar1.fillAmount = fighter1.currentHealth / fighter1.maxHealth;
            if (hpText1) hpText1.text = fighter1.currentHealth.ToString("F0");
        }
        if (fighter2 != null && healthBar2 != null)
        {
            healthBar2.fillAmount = fighter2.currentHealth / fighter2.maxHealth;
            if (hpText2) hpText2.text = fighter2.currentHealth.ToString("F0");
        }
    }
}
