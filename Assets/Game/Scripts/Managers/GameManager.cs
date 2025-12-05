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

    [Header("Upgrade Settings")]
    [SerializeField] private float startFireRate = 0.5f;
    [SerializeField] private float startMultiplicator = 1f;
    [SerializeField] private float startMultiplierIncome = 1f;

    [SerializeField] private Vector2 limit = new Vector2(2, 2);

    public static Vector2 Limit;
    public static GameManager Instance;
    
    private static int money = 0;

    private WorldData currentWorldData;
    private int currentLevelIndex = 0;
    private string currentScene;

    private float fireRate;
    private float multiplicator;
    private float multiplierIncome;

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
        multiplicator = startMultiplicator;
        multiplierIncome = startMultiplierIncome;

        btnPlay.onClick.AddListener(OnClickPlay);
    }

    private void LocalLevelManager_updateRef(LocalLevelManager sender, Cup _cup, List<Multiplier> _multipliers)
    {
        cup = _cup;
        multipliers = _multipliers;

        cup.fireRate = fireRate;

        foreach (Multiplier multiplier in multipliers)
        {
            multiplier.multipler *= multiplicator;
        }
    }

    private void LocalLevelManager_loose(LocalLevelManager sender)
    {
        pool.ReleaseAllObjects();
        menu.Show();
    }

    private void LocalLevelManager_win(LocalLevelManager sender)
    {
        pool.ReleaseAllObjects();
        GoToNextLevel();
    }

    private void OnClickPlay()
    {
        menu.Hide();
        localLevelManager.Play();
    }

    private bool CardUpgrade_buyUpgrade(CardUpgrade sender, int cost, EUpgradesTypes upgradeType)
    {
        if (money < cost)
            return false;

        money -= cost;

        switch (upgradeType)
        {
            case EUpgradesTypes.FIRE_RATE:
                break;
            case EUpgradesTypes.MULTIPLICATOR:
                break;
            case EUpgradesTypes.INCOME:
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

        localLevelManager.win -= LocalLevelManager_win;
        localLevelManager.loose -= LocalLevelManager_loose;
        localLevelManager.updateRef -= LocalLevelManager_updateRef;

        localLevelManager.win += LocalLevelManager_win;
        localLevelManager.loose += LocalLevelManager_loose;
        localLevelManager.updateRef += LocalLevelManager_updateRef;
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
