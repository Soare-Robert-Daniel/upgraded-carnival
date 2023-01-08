﻿using Map;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class RoomMenu : MonoBehaviour
    {
        [SerializeField] private MapManager manager;

        [Header("Templates")]
        [SerializeField] private UIDocument menu;

        [SerializeField] private VisualTreeAsset roomBtnTemplate;
        private VisualElement container;
        private VisualElement mainContainer;

        private Label selectedRoomLabel;

        private void Awake()
        {
            container = menu.rootVisualElement.Q<VisualElement>("Container");
            mainContainer = menu.rootVisualElement.Q<VisualElement>("MainContainer");
            selectedRoomLabel = menu.rootVisualElement.Q<Label>("SelectedRoomLabel");

            // container.RegisterCallback<MouseOverEvent>((_) =>
            // {
            //     manager.uiState.DeactivateSelecting();
            // });
            //
            // container.RegisterCallback<MouseOutEvent>((_) =>
            // {
            //     manager.uiState.ActivateSelecting();
            // });

            manager.OnInitUI += InitUI;
        }

        private void OnDisable()
        {
            manager.OnInitUI -= InitUI;
        }

        public void InitUI()
        {
            foreach (var roomModel in manager.RoomModels)
            {
                var btnElem = roomBtnTemplate.CloneTree();
                var btn = btnElem.Q<Button>("BuyRoomBtn");
                var img = btnElem.Q<VisualElement>("MainSprite");
                btn.text = $"Buy {roomModel.roomName}";
                img.style.backgroundImage = roomModel.sprite.texture;
                btn.clicked += () => manager.TryBuyRoomForSelectedRoom(roomModel.roomType);
                mainContainer.Add(btnElem);
            }

            manager.OnSelectedRoomChange += roomId => UpdateSelectedRoom($"{roomId}");
            container.Q<Button>("CloseBtn").clicked += CloseMenu;
        }

        public void UpdateSelectedRoom(string roomLabel)
        {
            selectedRoomLabel.text = $"Selected room: {roomLabel}";
        }

        public void OpenMenu()
        {
            container.visible = true;
            manager.uiState.DeactivateSelecting();
        }

        public void CloseMenu()
        {
            container.visible = false;
            manager.uiState.ActivateSelecting();
        }
    }
}