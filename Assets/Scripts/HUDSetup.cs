using UnityEngine;
using UnityEngine.UI;

public class HUDSetup : MonoBehaviour
{
    public Text titleText;
    public Text bottomText;

    void Start()
    {
        if (titleText) titleText.text = "Boxing Guy vs Exploding Guy";
        if (bottomText) bottomText.text = "Like and Subscribe!";
    }
}
