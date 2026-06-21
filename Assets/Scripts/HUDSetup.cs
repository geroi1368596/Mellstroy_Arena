using UnityEngine;
using TMPro;

public class HUDSetup : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI bottomText;

    void Start()
    {
        if (titleText) titleText.text = "Boxing Guy vs Exploding Guy";
        if (bottomText) bottomText.text = "Like and Subscribe!";
    }
}
