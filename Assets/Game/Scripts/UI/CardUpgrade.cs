using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public delegate bool CardUpgradeEventHandler(CardUpgrade sender, int cost, EUpgradesTypes upgradesType);

public class CardUpgrade : MonoBehaviour
{
    [SerializeField] private Button btnBuyUpgrade;
    [SerializeField] private TextMeshProUGUI txtCost;
    [SerializeField] private int initialCost = 50;
    [SerializeField] private float multiplierCost = 1.5f;
    [SerializeField] private EUpgradesTypes upgradeType;

    public event CardUpgradeEventHandler buyUpgrade;

    private int cost;

    private void Awake()
    {
        btnBuyUpgrade.onClick.AddListener(BuyUpgrade);
        cost = initialCost;
        UpdateCostText();
    }

    private void BuyUpgrade()
    {
        if (buyUpgrade?.Invoke(this, cost, upgradeType) == true)
        {
            cost = Mathf.RoundToInt(cost * multiplierCost);
            UpdateCostText();
        }
    }

    private void UpdateCostText()
    {
        txtCost.text = cost.ToString();
    }
}
