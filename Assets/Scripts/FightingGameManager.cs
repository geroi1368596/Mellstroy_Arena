using UnityEngine;
using UnityEngine.UI;

public class FightingGameManager : MonoBehaviour
{
    [Header("Characters")]
    public Fighter fighter1;  // LaserGay
    public Fighter fighter2;  // Boxer

    [Header("UI")]
    public Image healthBar1;
    public Image healthBar2;
    public Text hpText1;
    public Text hpText2;

    void Start()
    {
        // try to auto-assign UI elements if not set in inspector
        if (healthBar1 == null || hpText1 == null || healthBar2 == null || hpText2 == null)
        {
            var floats = FindObjectsOfType<FloatingHealthUI>(true);
            if (floats != null && floats.Length > 0)
            {
                if (fighter1 == null && floats.Length > 0) fighter1 = floats[0].target ? floats[0].target.GetComponent<Fighter>() : fighter1;
                if (floats.Length > 1 && fighter2 == null) fighter2 = floats[1].target ? floats[1].target.GetComponent<Fighter>() : fighter2;

                // try to get text components
                if (hpText1 == null && floats.Length > 0)
                {
                    hpText1 = floats[0].GetComponentInChildren<Text>();
                }
                if (hpText2 == null && floats.Length > 1)
                {
                    hpText2 = floats[1].GetComponentInChildren<Text>();
                }

                // health images (if any)
                if (healthBar1 == null && floats.Length > 0)
                {
                    var img = floats[0].GetComponent<Image>();
                    if (img != null) healthBar1 = img;
                }
                if (healthBar2 == null && floats.Length > 1)
                {
                    var img = floats[1].GetComponent<Image>();
                    if (img != null) healthBar2 = img;
                }
            }

            // final fallback: find any Text named HPText
            if (hpText1 == null) hpText1 = GameObject.FindObjectOfType<Text>();
        }
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (fighter1 != null && healthBar1 != null)
        {
            healthBar1.fillAmount = Mathf.Clamp01(fighter1.currentHealth / fighter1.maxHealth);
            if (hpText1) hpText1.text = fighter1.currentHealth.ToString("F0");
        }
        if (fighter2 != null && healthBar2 != null)
        {
            healthBar2.fillAmount = Mathf.Clamp01(fighter2.currentHealth / fighter2.maxHealth);
            if (hpText2) hpText2.text = fighter2.currentHealth.ToString("F0");
        }
    }
}
