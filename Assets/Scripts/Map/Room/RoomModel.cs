using UnityEngine;

namespace Map.Room
{
    [CreateAssetMenu(fileName = "Room Model", menuName = "Rooms/Create Room Model", order = 0)]
    public class RoomModel : ScriptableObject
    {
        [Header("Attributes")]
        public RoomType roomType;

        public string roomName;
        public float fireRate;
        public float damage;
    }
}