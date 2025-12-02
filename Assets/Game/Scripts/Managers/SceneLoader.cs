using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private StartLoadingScreen startLoadingScreen;
    [SerializeField] private LoadingScreen loadingScreen;
    [SerializeField] private float durationAnimLoadingScreen;
    [SerializeField] private float lerpLoadingProgress = 10f;

    public static SceneLoader Instance { get; private set; }

    private string currentScene;

    private void Awake()
    {
        loadingScreen.Hide();
        startLoadingScreen.Hide();

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }


    public async Task SwitchScene(string sceneToSwitch, bool startScene = false)
    {
        if (startScene)
            startLoadingScreen.Show();
        else
            loadingScreen.Show();

        await Task.Yield();

        if (currentScene != null)
            await SceneManager.UnloadSceneAsync(currentScene);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneToSwitch, LoadSceneMode.Additive);
        loadOperation.allowSceneActivation = false;

        float displayedProgress = 0f;
        while (!loadOperation.isDone)
        {
            float targetProgress = Mathf.Clamp01(loadOperation.progress / 0.9f);

            if (!startScene)
                displayedProgress = Mathf.Lerp(displayedProgress, targetProgress, Time.deltaTime * lerpLoadingProgress);
            else
                displayedProgress = targetProgress;

            string progressText = ((int)(displayedProgress * 100)) + "%";

            if (startScene)
                startLoadingScreen.txtLoading.text = progressText;

            loadingScreen.UpdateTextPercetage(progressText);

            // Quand on arrive à 0.9, on attend juste un peu pour simuler un chargement plus long
            if (loadOperation.progress >= 0.9f && displayedProgress >= 0.99f)
                loadOperation.allowSceneActivation = true;

            await Task.Yield();
        }

        loadOperation.allowSceneActivation = true;

        // Attendre la fin complète du chargement
        while (!loadOperation.isDone)
            await Task.Yield();

        if (startScene)
            startLoadingScreen.Hide();
        else
        {
            loadingScreen.UpdateTextPercetage("");
            StartCoroutine(LoadingScreenAnimation());
        }

        currentScene = sceneToSwitch;
    }

    private IEnumerator LoadingScreenAnimation()
    {
        float counterAnimLoadingScreen = 0;

        while (counterAnimLoadingScreen < durationAnimLoadingScreen)
        {
            counterAnimLoadingScreen += Time.deltaTime;
            loadingScreen.UpdateBackgroundImage(1 - (counterAnimLoadingScreen / durationAnimLoadingScreen));
            yield return null;
        }

        loadingScreen.UpdateBackgroundImage(0);
        loadingScreen.Hide();

        yield return null;
    }
}
