using Map;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] private MapManager manager;
        [SerializeField] private UIDocument hud;
        [SerializeField] private RoomMenu roomMenu;

        private Button openRoomMenuBtn;

        private void Awake()
        {
            manager.OnInitUI += InitUI;
        }

        public void InitUI()
        {
            openRoomMenuBtn = hud.rootVisualElement.Q<Button>("OpenRoomMenuBtn");
            openRoomMenuBtn.clicked += roomMenu.OpenMenu;

            manager.OnSelectedRoomChange += roomId =>
            {
                UpdateOpenRoomLabel($"Open Build Menu ({roomId})");
            };

            openRoomMenuBtn.RegisterCallback<MouseOverEvent>((_) =>
            {
                Debug.Log("Over the menu button");
                manager.uiState.StopSelectionOverHud();
            });

            openRoomMenuBtn.RegisterCallback<MouseOutEvent>((_) =>
            {
                Debug.Log("Exit the menu button");
                manager.uiState.StartSelectionOutHUD();
            });

            var currentGoldLabel = hud.rootVisualElement.Q<Label>("GoldLabel");
            currentGoldLabel.text = $"Gold: {manager.EconomyController.CurrentGold}";
            manager.EconomyController.OnCurrentGoldChanged += gold =>
            {
                currentGoldLabel.text = $"Gold: {gold}";
            };

            var nextWaveLabel = hud.rootVisualElement.Q<Label>("NextWaveLabel");
            nextWaveLabel.text = "Next wave: 0";
            manager.OnNextWaveTimeChanged += time =>
            {
                nextWaveLabel.text = $"Next wave: {Mathf.RoundToInt(time)}";
            };

            var currentWaveNumberLabel = hud.rootVisualElement.Q<Label>("WaveNumberLabel");
            currentWaveNumberLabel.text = $"Wave: 1";
            manager.OnNextWaveStarted += waveNumber =>
            {
                currentWaveNumberLabel.text = $"Wave: {waveNumber + 1}";
            };

            var currentBuyLevelBtn = hud.rootVisualElement.Q<Button>("BuyLevelBtn");
            currentBuyLevelBtn.text = $"Buy Level ({manager.NewCurrentPricePerLevel} Gold)";
            currentBuyLevelBtn.clicked += () => manager.TryBuyLevel();
            manager.OnLevelUpdated += ((i, f) =>
            {
                currentBuyLevelBtn.text = $"Buy Level ({f} Gold)";
            });
        }

        public void UpdateOpenRoomLabel(string text)
        {
            openRoomMenuBtn.text = text;
        }
    }
}