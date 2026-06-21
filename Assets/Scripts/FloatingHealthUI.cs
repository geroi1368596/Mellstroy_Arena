using TMPro;
using UnityEngine;

public class FloatingHealthUI : MonoBehaviour
{
    public Transform target;
    public Vector3 worldOffset = new Vector3(0, 1.2f, 0);
    public TextMeshProUGUI healthText;
    RectTransform rect;
    Camera cam;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        cam = Camera.main;
    }

    void Update()
    {
        if (target == null) { gameObject.SetActive(false); return; }
        Vector3 screenPos = cam.WorldToScreenPoint(target.position + worldOffset);
        rect.position = screenPos;

        var fighter = target.GetComponent<Fighter>();
        if (fighter != null && healthText != null)
        {
            healthText.text = Mathf.Max(0, Mathf.RoundToInt(fighter.currentHealth)).ToString();
        }
    }
}
