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

            manager.OnInitUI += InitUI;
        }

        private void OnDisable()
        {
            manager.OnInitUI -= InitUI;
        }

        public void InitUI()
        {
            foreach (var roomModel in manager.RoomModels.list)
            {
                var btnElem = roomBtnTemplate.CloneTree();
                var btn = btnElem.Q<Button>("RoomBtn");
                btn.text = $"Room: {roomModel.roomName}";
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
        }

        public void CloseMenu()
        {
            container.visible = false;
        }
    }
}