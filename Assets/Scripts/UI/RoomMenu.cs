using System;
using Map;
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
            try
            {
                foreach (var (zoneTokenType, res) in manager.globalResources.GetZonesResources())
                {
                    var btnElem = roomBtnTemplate.CloneTree();

                    var btn = btnElem.Q<Button>("BuyRoomBtn");
                    var roomPrice = btnElem.Q<Label>("RoomPrice");

                    btn.text = $"Buy {res.resourcesScriptableObject.label}";
                    btn.clicked += () =>
                    {
                        manager.TryBuyZoneForSelectedZone(zoneTokenType);
                    };

                    roomPrice.text = $"{res.price.value}";

                    mainContainer.Add(btnElem);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error while init UI");
                Debug.LogException(e);
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