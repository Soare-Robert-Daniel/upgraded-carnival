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
        [Header("Levels")]
        [SerializeField] private int levelsNum;

        [SerializeField] private Vector3 offset;
        [SerializeField] private List<LevelController> levelsControllers;

        [Header("Rooms")]
        [SerializeField] private List<RoomController> roomsControllers;

        [SerializeField] private List<RoomType> roomsTypes;

        [SerializeField] private List<int> entitiesInRooms;
        [SerializeField] private List<float> damagePerEntity;
        [SerializeField] private List<bool> roomsCanFire;

        [Header("Resources")]
        [SerializeField] private int startingLevelsNum;

        [SerializeField] private GameObject levelTemplate;
        [SerializeField] private Transform startPoint;
        private Dictionary<Mob, int> entitiesToId;
        private Dictionary<int, Mob> idToEntities;

        private int mobGenID;

        private void Start()
        {
            mobGenID = 0;
            CreateLayout(startingLevelsNum);
        }


        private List<Vector3> CreateLayoutPositions()
        {
            return new List<Vector3>();
        }

        #region Events

        #endregion

        #region Map Generation

        public void CreateLayout(int levels)
        {
            if (levels < levelsNum)
            {
                return;
            }

            for (int i = 0; i < levels - levelsNum; i++)
            {
                var obj = Instantiate(levelTemplate, startPoint.position + (levelsNum + i) * offset, Quaternion.identity);
                var levelController = obj.GetComponent<LevelController>();
                RegisterLevel(levelController);
            }

            levelsNum = levels;
        }

        private void RegisterLevel(LevelController level)
        {
            // Might add more things when registering a level (like sound effects).
            levelsControllers.Add(level);
            RegisterRooms(level.roomsControllers);
        }

        private void RegisterRooms(List<RoomController> rooms)
        {
            // Might add more things when registering rooms (like sound effects).
            foreach (var room in rooms)
            {
                room.MapManager = this;
            }
            roomsControllers.AddRange(rooms);
        }

        #endregion

        #region Room Gameplay

        private void ApplyDamageFromRooms()
        {
            // I wonder if we can put this on a Job.
            for (var entityId = 0; entityId < entitiesInRooms.Count; entityId++)
            {
                var room = entitiesInRooms[entityId];
                if (roomsCanFire[room])
                {
                    ComputeDamageToEntityFromRoom(entityId, room);
                }

                ApplyDamageToMob(entityId);
            }
        }

        private void ComputeDamageToEntityFromRoom(int entityId, int roomId)
        {
            var damage = roomsTypes[roomId] switch
            {
                RoomType.Empty => 0f,
                RoomType.Simple => 1f,
                RoomType.Medium => 5f,
                RoomType.Heavy => 10f,
                _ => throw new ArgumentOutOfRangeException($"{roomsTypes[roomId]} is not a valid Room type.")
            };

            damagePerEntity[entityId] = damage;
        }

        private void MoveMobToRoom(int entityId, int roomId)
        {
            entitiesInRooms[entityId] = roomId;
        }

        private int GetNextRoomForMob(int entityId)
        {
            return Mathf.Min(entitiesInRooms[entityId] + 1, roomsControllers.Count);
        }

        public void MoveMobToNextRoom(int entityId)
        {
            MoveMobToRoom(entityId, GetNextRoomForMob(entityId));
        }

        #endregion

        #region Mob Gameplay

        private int GenMobID()
        {
            var id = mobGenID;
            mobGenID += 1;
            return id;
        }

        private void RegisterMob(Mob mob)
        {
            mob.id = GenMobID();

            entitiesInRooms.Add(0);
            entitiesToId.Add(mob, mob.id);
            idToEntities.Add(mob.id, mob);
        }

        private void ApplyDamageToMob(int entityId)
        {
            idToEntities[entityId].ApplyDamage(damagePerEntity[entityId]);
        }

        #endregion

    }
}