﻿using System;
using System.Globalization;
using Map;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private MapManager mapManager;
        [SerializeField] private UIDocument hud;
        [SerializeField] private RoomMenu roomMenu;

        private Button openRoomMenuBtn;

        private void Awake()
        {
            mapManager.OnInitUI += InitUI;
        }

        public event Action OnSettingsBtnClicked;

        public void InitUI()
        {
            openRoomMenuBtn = hud.rootVisualElement.Q<Button>("OpenRoomMenuBtn");
            openRoomMenuBtn.clicked += roomMenu.OpenMenu;

            mapManager.OnSelectedRoomChange += roomId =>
            {
                UpdateOpenRoomLabel($"Open Build Menu ({roomId})");
            };

            openRoomMenuBtn.RegisterCallback<MouseOverEvent>((_) =>
            {
                // Debug.Log("Over the menu button");
                mapManager.uiState.StopSelectionOverHud();
            });

            openRoomMenuBtn.RegisterCallback<MouseOutEvent>((_) =>
            {
                // Debug.Log("Exit the menu button");
                mapManager.uiState.StartSelectionOutHUD();
            });

            var currentGoldLabel = hud.rootVisualElement.Q<Label>("GoldLabel");
            currentGoldLabel.text = $"Gold: {mapManager.EconomyController.CurrentGold}";
            mapManager.EconomyController.OnCurrentGoldChanged += gold =>
            {
                currentGoldLabel.text = $"Gold: {gold}";
            };

            var nextWaveLabel = hud.rootVisualElement.Q<Label>("NextWaveLabel");
            nextWaveLabel.text = "Next wave: 0";
            mapManager.OnNextWaveTimeChanged += time =>
            {
                nextWaveLabel.text = $"Next wave: {Mathf.RoundToInt(time)}s";
            };

            var currentWaveNumberLabel = hud.rootVisualElement.Q<Label>("WaveNumberLabel");
            currentWaveNumberLabel.text = $"Wave: 1";
            mapManager.OnNextWaveStarted += waveNumber =>
            {
                currentWaveNumberLabel.text = $"Wave: {waveNumber + 1}";
            };

            var currentBuyLevelBtn = hud.rootVisualElement.Q<Button>("BuyLevelBtn");
            currentBuyLevelBtn.text = $"Buy Level ({mapManager.NewCurrentPricePerLevel} Gold)";
            currentBuyLevelBtn.clicked += () => mapManager.TryBuyLevel();
            mapManager.OnLevelUpdated += (i, f) =>
            {
                currentBuyLevelBtn.text = $"Buy Level ({f} Gold)";
            };

            var currentPlayerRemainingHealthLabel = hud.rootVisualElement.Q<Label>("RemainingHealthLabel");
            currentPlayerRemainingHealthLabel.text = $"Health: {gameManager.PlayerHealth}";
            gameManager.OnPlayerHealthChanged += health =>
            {
                currentPlayerRemainingHealthLabel.text = $"Health: {health}";
            };

            var settingsBtn = hud.rootVisualElement.Q<Button>("SettingsBtn");
            settingsBtn.clicked += () => OnSettingsBtnClicked?.Invoke();

            var startWaveBtn = hud.rootVisualElement.Q<Button>("StartWaveBtn");
            startWaveBtn.clicked += () => StartCoroutine(mapManager.ManualStartWave());

            var mapLogicTimeLabel = hud.rootVisualElement.Q<Label>("mapLogicTimeLabel");
            mapManager.OnMapLogicTimeChanged += time =>
            {
                mapLogicTimeLabel.text = MathF.Round(time).ToString(CultureInfo.InvariantCulture);
            };
        }

        public void UpdateOpenRoomLabel(string text)
        {
            openRoomMenuBtn.text = text;
        }
    }
}