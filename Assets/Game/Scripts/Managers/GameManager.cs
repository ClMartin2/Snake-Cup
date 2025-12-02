using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<WorldData> allWorlds = new();
    [SerializeField] private WorldData startWorld;

    [SerializeField] private Vector2 limit = new Vector2(2, 2);

    public static Vector2 Limit;
    public static GameManager Instance;

    private WorldData currentWorldData;
    private int currentLevelIndex = 0;
    private string currentScene;

    private void Awake()
    {
        if (Instance != null)
            Instance = this;
        else
            Destroy(gameObject);

        Limit = limit;

        currentWorldData = startWorld;
        LoadWorld(startWorld, true);
    }

    public void Loose()
    {
        Debug.Log("Loose");
    }

    public void Win()
    {
        GoToNextLevel();
    }

    private async Task LoadWorld(WorldData worldData, bool startScene = false)
    {
        currentWorldData = worldData;
        currentLevelIndex = 0;
        currentScene = currentWorldData.scenes[0];

        await SceneLoader.Instance.SwitchScene(currentScene, startScene);
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
            await SceneLoader.Instance.SwitchScene(currentWorldData.scenes[currentLevelIndex]);
            currentScene = currentWorldData.scenes[currentLevelIndex];
        }
    }
}
