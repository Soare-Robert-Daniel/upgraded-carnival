using System.Collections.Generic;
using GameEntities;
using Map.Room;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Map
{
    public class RoomController : MonoBehaviour
    {
        [Header("Attributes")]
        [SerializeField] private bool canFire;

        [SerializeField] private float currentFireInterval;
        [SerializeField] private float currentFireDuration;

        [FormerlySerializedAs("roomState")] [SerializeField]
        private RoomSettings roomSettings;

        [SerializeField] private bool isSelected;

        [Header("Settings")]
        [SerializeField] private int id;

        [SerializeField] private MapManager mapManager;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private List<int> entitiesInRoom;

        [SerializeField] private Transform startingPoint;
        [SerializeField] private Transform exitPoint;

        [Header("Components")]
        [SerializeField] private TextMeshPro roomName;

        [SerializeField] private SymbolController symbolController;
        [SerializeField] private GameObject selectedBackgroundObj;


        public int ID
        {
            get => id;
            set => id = value;
        }

        public MapManager MapManager
        {
            get => mapManager;
            set => mapManager = value;
        }

        public bool CanFire
        {
            get => canFire;
            set => canFire = value;
        }

        public RoomSettings RoomSettings => roomSettings;

        private void Start()
        {
            symbolController.UpdateVerticalSymbols(SymbolStateV.Top);
            symbolController.UpdateHorizontalSymbols(SymbolStateH.RightAndLeft);
        }

        private void Update()
        {
            // Remove in the future
            UpdateSymbols();

            if (!canFire)
            {
                return;
            }

            currentFireInterval += Time.deltaTime;

            if (currentFireInterval > roomSettings.FireRate + roomSettings.FireDuration)
            {
                mapManager.MarkRoomToFire(id);
                ResetFire();
            }
        }

        public void UpdateState(RoomSettings newSettings)
        {
            roomSettings = newSettings;
            UpdateSymbols();
        }

        public void RefreshStats()
        {

        }

        public void SetPosition(Vector3 newPosition)
        {
            transform.position = newPosition;
        }

        public void SetSprite(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
        }

        public Vector3 GetStartingPosition()
        {
            return startingPoint.position;
        }

        public Vector3 GetExitPosition()
        {
            return exitPoint.position;
        }

        #region UI

        public void UpdateRoomName()
        {
            roomName.text = $"Room {id} {roomSettings.RoomType}";
        }

        public void UpdateVisual(RoomModel roomModel)
        {
            spriteRenderer.sprite = roomModel.sprite;
            UpdateRoomName();
        }

        public void UpdateSymbols()
        {
            symbolController.UpdateVerticalSymbols(roomSettings.VerticalSym);
            symbolController.UpdateHorizontalSymbols(roomSettings.HorizontalSym);
        }

        public void Select()
        {
            isSelected = true;
            selectedBackgroundObj.SetActive(true);
        }

        public void Deselect()
        {
            isSelected = false;
            selectedBackgroundObj.SetActive(false);
        }

        #endregion

        #region Events

        #endregion

        #region Map Manager Interactions

        private void OnTriggerEnter(Collider other)
        {
            var mob = other.gameObject.GetComponent<Mob>();
            if (mob != null)
            {
                mapManager.SetMobRoomStatus(mob.id, EntityRoomStatus.Exit);
            }
        }

        public void ResetFire()
        {
            currentFireInterval = 0f;
        }

        #endregion

    }
}