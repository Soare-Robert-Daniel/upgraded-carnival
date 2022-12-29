using System.Collections.Generic;
using GameEntities;
using TMPro;
using UnityEngine;

namespace Map
{
    public class RoomController : MonoBehaviour
    {
        [Header("Attributes")]
        [SerializeField] private float fireRate;

        [SerializeField] private float currentFireInterval;

        [Header("Settings")]
        [SerializeField] private int id;

        [SerializeField] private MapManager mapManager;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private List<int> entitiesInRoom;

        [SerializeField] private Transform startingPoint;
        [SerializeField] private Transform exitPoint;

        [Header("Components")]
        [SerializeField] private TextMeshPro roomName;

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

        private void Update()
        {
            currentFireInterval += Time.deltaTime;

            if (currentFireInterval > fireRate)
            {
                mapManager.MarkRoomToFire(id);
                ResetFire();
            }
        }

        #region UI

        public void UpdateRoomName()
        {
            roomName.text = $"Room {id}";
        }

        #endregion

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