using System.Collections.Generic;
using Towers.Zones;
using UnityEngine;

namespace Map
{
    public class ZoneController : MonoBehaviour
    {
        public int zoneId;
        public List<RoomController> roomsControllers;
        public Transform northBound;
        public Transform southBound;
        public ZoneAreaController zoneAreaController;
        public ZoneTowerController zoneTowerController;
    }
}