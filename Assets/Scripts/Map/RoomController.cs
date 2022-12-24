using System.Collections.Generic;
using GameEntities;
using UnityEngine;

namespace Map
{
    public class RoomController : MonoBehaviour
    {

        [SerializeField] private int id;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private List<EntityStats> entitiesInRoom;

        public int ID
        {
            get => id;
            set => id = value;
        }

        public void SetPosition(Vector3 newPosition)
        {
            transform.position = newPosition;
        }

        public void SetSprite(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
        }
    }
}