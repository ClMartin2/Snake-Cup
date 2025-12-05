using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public delegate bool CardUpgradeEventHandler(CardUpgrade sender, int cost, EUpgradesTypes upgradesType, float upgradeValue, Operator _operator);

public class CardUpgrade : MonoBehaviour
{
    [SerializeField] private Button btnBuyUpgrade;
    [SerializeField] private TextMeshProUGUI txtCost;
    [SerializeField] private TextMeshProUGUI txtValue;
    [SerializeField] private int initialCost = 50;
    [SerializeField] private float multiplierCost = 1.5f;
    [SerializeField] private EUpgradesTypes upgradeType;
    [SerializeField] private float upgradesValue;
    [SerializeField] private Operator _operator;

    public event CardUpgradeEventHandler buyUpgrade;

    private int cost;
    private int level;

    private void Awake()
    {
        btnBuyUpgrade.onClick.AddListener(BuyUpgrade);
        cost = initialCost;
        UpdateText();
    }

    private void BuyUpgrade()
    {
        if (buyUpgrade?.Invoke(this, cost, upgradeType,upgradesValue,_operator) == true)
        {
            cost = Mathf.RoundToInt(cost * multiplierCost);
            level ++;
            UpdateText();
        }
    }

    private void UpdateText()
    {
        txtCost.text = cost.ToString();
        txtValue.text = "Level " + level;
    }
}

public enum Operator
{
    Add,
    Subtract,
    Multiply,
    Divide
}

public static class OperatorUtils
{
    public static float Apply(Operator op, float a, float b)
    {
        return op switch
        {
            Operator.Add => a + b,
            Operator.Subtract => a - b,
            Operator.Multiply => a * b,
            Operator.Divide => a / b
        };
    }

    public static string OperatorToString(Operator _operator)
    {
        return _operator switch
        {
            Operator.Add => "+",
            Operator.Subtract => "+",
            Operator.Multiply => "x",
            Operator.Divide => "/"
        };
    }
}
