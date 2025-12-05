using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<WorldData> allWorlds = new();
    [SerializeField] private WorldData startWorld;
    [SerializeField] private int startMoney = 10;
    [SerializeField] private Menu menu;
    [SerializeField] private CardUpgrade[] cardUpgrades;
    [SerializeField] private Button btnPlay;
    [SerializeField] private ObjectPoolManager pool;
    [SerializeField] private Vector2 limit = new Vector2(2, 2);

    [Header("Upgrade Settings")]
    [SerializeField] private float startFireRate = 0.5f;
    [SerializeField] private int startAdditionalMultiplier = 0;
    [SerializeField] private float startMultiplierIncome = 1f;

    public static Vector2 Limit;
    public static GameManager Instance;
    
    private static int money = 0;

    private WorldData currentWorldData;
    private int currentLevelIndex = 0;
    private string currentScene;

    private float fireRate;
    private int additionalMultiplier;
    private float multiplierIncome = 1;

    private Cup cup;
    private List<Multiplier> multipliers = new();

    private LocalLevelManager localLevelManager;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        Limit = limit;

        currentWorldData = startWorld;
        LoadWorld(startWorld, true);

        money = startMoney;
        menu.Hide();
        UpdateTxtCoin();

        foreach (CardUpgrade cardUpgrade in cardUpgrades)
        {
            cardUpgrade.buyUpgrade += CardUpgrade_buyUpgrade;
        }

        fireRate = startFireRate;
        additionalMultiplier = startAdditionalMultiplier;
        multiplierIncome = startMultiplierIncome;

        btnPlay.onClick.AddListener(OnClickPlay);
    }

    private void ApplyFireRateUpgrade()
    {
        cup.fireRate = fireRate;
    }

    private void ApplyMultiplierUpgrade()
    {
        foreach (Multiplier multiplier in multipliers)
        {
            multiplier.UpdateMultiplier(multiplier.startMultiplier + additionalMultiplier);
        }
    }

    private void LocalLevelManager_loose(LocalLevelManager sender)
    {
        money += currentWorldData.scenes[currentLevelIndex].moneyWhenLoose;
        UpdateTxtCoin();

        pool.ReleaseAllObjects();
        menu.Show();
    }

    private void LocalLevelManager_win(LocalLevelManager sender)
    {
        money += currentWorldData.scenes[currentLevelIndex].moneyWhenWin;
        UpdateTxtCoin();

        pool.ReleaseAllObjects();
        GoToNextLevel();
    }

    private void OnClickPlay()
    {
        menu.Hide();
        localLevelManager.Play();
    }

    private bool CardUpgrade_buyUpgrade(CardUpgrade sender, int cost, EUpgradesTypes upgradeType, float upgradeValue, Operator _operator)
    {
        if (money < cost)
            return false;

        money -= cost;

        switch (upgradeType)
        {
            case EUpgradesTypes.FIRE_RATE:
                fireRate = OperatorUtils.Apply(_operator,fireRate,upgradeValue);
                ApplyFireRateUpgrade();
                break;
            case EUpgradesTypes.MULTIPLICATOR:
                additionalMultiplier = (int)OperatorUtils.Apply(_operator, additionalMultiplier, upgradeValue);
                ApplyMultiplierUpgrade();
                break;
            case EUpgradesTypes.INCOME:
                multiplierIncome = OperatorUtils.Apply(_operator, multiplierIncome, upgradeValue);
                break;
        }

        UpdateTxtCoin();

        return true;
    }

    private void UpdateTxtCoin()
    {
        menu.UpdateTxtCoin(money);
    }

    private async Task SwitchScene(bool startScene = false)
    {
        await SceneLoader.Instance.SwitchScene(currentScene, startScene);

        localLevelManager = LocalLevelManager.Instance;

        cup = localLevelManager.cup;
        multipliers = localLevelManager.multipliers;

        ApplyFireRateUpgrade();
        ApplyMultiplierUpgrade();

        localLevelManager.win -= LocalLevelManager_win;
        localLevelManager.loose -= LocalLevelManager_loose;

        localLevelManager.win += LocalLevelManager_win;
        localLevelManager.loose += LocalLevelManager_loose;
    }

    private async Task LoadWorld(WorldData worldData, bool startScene = false)
    {
        currentWorldData = worldData;
        currentLevelIndex = 0;
        currentScene = currentWorldData.scenes[0].levelName;

        await SwitchScene(startScene);
    }

    private async Task GoToNextLevel()
    {
        if (currentLevelIndex > currentWorldData.scenes.Length - 1)
        {
            //Check si le monde est deja débloqué
            int currentWorldIndexData = allWorlds.FindIndex((worldData) => worldData == currentWorldData);

            if (currentWorldIndexData < allWorlds.Count - 1)
            {
                currentWorldIndexData++;

                WorldData worldToUnlock = allWorlds[currentWorldIndexData];
                await LoadWorld(worldToUnlock);
            }
            else
            {
                await LoadWorld(allWorlds[0]);
            }
        }
        else
        {
            await SwitchScene();
            currentScene = currentWorldData.scenes[currentLevelIndex].levelName;
        }
    }
}
