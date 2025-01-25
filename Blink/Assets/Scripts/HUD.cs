using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private Image[] blinkImages;
    [SerializeField] private Player player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Image i in blinkImages)
        {
            i.type = Image.Type.Filled;
            i.fillMethod = Image.FillMethod.Vertical;
        }

        player.OnVariableChange += BlinkHUDHandler;
    }

    private void BlinkHUDHandler(int newVal)
    {
        for (int i = 0; i < player.NumberOfBlinks * 100; i += 100)
        {
            if (player.BlinkCharge > i + 100) { blinkImages[i / 100].fillAmount = 1; continue; }
            blinkImages[i / 100].fillAmount = (float)(player.BlinkCharge - i) / 100f;
        }
    }
}
