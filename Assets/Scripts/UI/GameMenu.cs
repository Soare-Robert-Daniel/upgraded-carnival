using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace UI
{
    public class GameMenu : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField] private string mainMenuScene;

        [SerializeField] private string optionsScene;

        [Header("Templates")]
        [SerializeField] private HUD hud;

        [SerializeField] private UIDocument menu;
        [SerializeField] private bool isOpen;

        private VisualElement container;

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            var root = menu.rootVisualElement;

            container = root.Q<VisualElement>("Container");
            isOpen = container.style.display.value == DisplayStyle.None;

            var restartBtn = root.Q<Button>("RestartBtn");
            restartBtn.clicked += () =>
            {
                Debug.Log("Restart");
            };

            var saveGameBtn = root.Q<Button>("SaveGameBtn");
            saveGameBtn.clicked += () =>
            {
                Debug.Log("Save Game");
            };

            var optionsBtn = root.Q<Button>("OptionsBtn");
            optionsBtn.clicked += () =>
            {
                Debug.Log("Options");
                StartCoroutine(LoadScene(optionsScene));
            };

            var mainMenuBtn = root.Q<Button>("MainMenuBtn");
            mainMenuBtn.clicked += () =>
            {
                Debug.Log("Main Menu");
                StartCoroutine(LoadScene(mainMenuScene));
            };

            var backBtn = root.Q<Button>("BackBtn");
            backBtn.clicked += () =>
            {
                Debug.Log("Back");
                ToggleMenu();
            };

            hud.OnSettingsBtnClicked += ToggleMenu;
            UpdateContainerVisibility();
        }

        public void ToggleMenu()
        {
            isOpen = !isOpen;
            UpdateContainerVisibility();
        }

        public void UpdateContainerVisibility()
        {
            container.style.display = isOpen ? DisplayStyle.Flex : DisplayStyle.None;
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
}