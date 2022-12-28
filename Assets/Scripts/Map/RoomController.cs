using System.Collections.Generic;
using GameEntities;
using UnityEngine;

namespace Map
{
    public class RoomController : MonoBehaviour
    {
        [SerializeField] private MapManager mapManager;
        [SerializeField] private int id;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private List<int> entitiesInRoom;

        [SerializeField] private Transform startingPoint;
        [SerializeField] private Transform exitPoint;

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

        #region Map Manager Interactions

        private void OnTriggerEnter(Collider other)
        {
            var mob = other.gameObject.GetComponent<Mob>();
            if (mob != null)
            {
                mapManager.MoveMobToNextRoom(mob.id);
            }
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
    }
}