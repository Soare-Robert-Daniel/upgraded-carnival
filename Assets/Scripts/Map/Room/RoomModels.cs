using System.Collections.Generic;
using UnityEngine;

namespace Map.Room
{
    [CreateAssetMenu(fileName = "Room Models", menuName = "Rooms/Create Room Models List", order = 0)]
    public class RoomModels : ScriptableObject
    {
        public List<RoomModel> list;
    }
}