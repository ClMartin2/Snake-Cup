using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : CustomScreen
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI txtPercentage;

    public void UpdateBackgroundImage(float percentage)
    {
        backgroundImage.fillAmount = percentage;
    }

    public void UpdateTextPercetage(string percentage)
    {
        txtPercentage.text = percentage;
    }

    public override void Show()
    {
        base.Show();
         
        UpdateBackgroundImage(1);
        UpdateTextPercetage("0 %");
    }

    public override void Hide()
    {
        base.Hide();
    }
}
