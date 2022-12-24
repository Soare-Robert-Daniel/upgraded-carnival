using System;
using System.Collections.Generic;
using GameEntities;
using UnityEngine;

namespace Map
{
    public enum RoomType
    {
        Empty,
        Simple,
        Medium,
        Heavy
    }
    
    public class MapManager : MonoBehaviour
    {
        [SerializeField] private List<RoomController> rooms;
        [SerializeField] private List<RoomType> roomsTypes;

        [SerializeField] private List<int> entitiesInRooms;
        [SerializeField] private List<bool> roomsCanFire;

        private Dictionary<Mob, int> entitiesToId;
        private Dictionary<int, Mob> idToEntities;

        private void ApplyDamageFromRooms()
        {
            for (var entityId = 0; entityId < entitiesInRooms.Count; entityId++)
            {
                var room = entitiesInRooms[entityId];
                if (roomsCanFire[room])
                {
                    ApplyDamageToEntityFromRoom(entityId, room);
                }
            }
        }

        private void ApplyDamageToEntityFromRoom(int entityId, int roomId)
        {
            var damage = roomsTypes[roomId] switch
            {
                RoomType.Empty => 0f,
                RoomType.Simple => 1f,
                RoomType.Medium => 5f,
                RoomType.Heavy => 10f,
                _ => throw new ArgumentOutOfRangeException($"{roomsTypes[roomId]} is not a valid Room type.")
            };
            
            idToEntities[entityId].ApplyDamage(damage);
        }

        private List<Vector3> CreateLayoutPositions()
        {
            return new List<Vector3>();
        }
    }
}