using TMPro;
using UnityEngine;

public class Menu : CustomScreen
{
    [SerializeField] private TextMeshProUGUI txtCoins;

    public void UpdateTxtCoin(int coins)
    {
        txtCoins.text = coins.ToString();
    }
}
