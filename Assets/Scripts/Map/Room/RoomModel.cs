using UnityEngine;

namespace Map.Room
{
    [CreateAssetMenu(fileName = "Room Model", menuName = "Rooms/Create Room Model", order = 0)]
    public class RoomModel : ScriptableObject
    {
        [Header("Symbols for action influence")]
        public SymbolStateV verticalSym;

        public SymbolStateH horizontalSym;

        [Header("Attributes")]
        public RoomType roomType;

        public float fireRate;
    }
}