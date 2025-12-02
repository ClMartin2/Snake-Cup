using TMPro;
using UnityEngine;

public class StartLoadingScreen : CustomScreen
{
    [field: SerializeField] public TextMeshProUGUI txtLoading { get; private set; }

    public override void Show()
    {
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
    }
}
