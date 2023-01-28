using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string mainGameScene;

    [SerializeField] private string settingsScene;

    [Header("Resources")]
    [SerializeField] private UIDocument mainMenuUI;

    // Start is called before the first frame update
    private void Start()
    {
        var root = mainMenuUI.rootVisualElement;

        var playBtn = root.Q<Button>("PlayBtn");
        playBtn.clicked += () =>
        {
            Debug.Log("Play button clicked");
            LoadMainGamePlayScene();
        };

        var settingsBtn = root.Q<Button>("SettingsBtn");
        settingsBtn.clicked += () =>
        {
            Debug.Log("Settings button clicked");
            LoadSettingsScene();
        };

        var quitBtn = root.Q<Button>("ExitBtn");
        quitBtn.clicked += () =>
        {
            Debug.Log("Quit button clicked");
            QuitGame();
        };
    }

    public void LoadMainGamePlayScene()
    {
        if (mainGameScene.Length > 0)
        {
            StartCoroutine(LoadScene(mainGameScene));
        }
    }

    public void LoadSettingsScene()
    {
        if (settingsScene.Length > 0)
        {
            StartCoroutine(LoadScene(settingsScene));
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private IEnumerator LoadScene(string sceneName)
    {
        Debug.Log($"Loading scene: {sceneName}");
        var asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}