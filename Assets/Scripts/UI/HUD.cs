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
                UpdateOpenRoomLabel($"Open menu for room: {roomId}");
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
        }

        public void UpdateOpenRoomLabel(string text)
        {
            openRoomMenuBtn.text = text;
        }
    }
}