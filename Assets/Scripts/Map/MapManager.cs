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

    public enum EntityRoomStatus
    {
        Entered,
        Moving,
        Exit,
        Retired // The mob is waiting to re-enter on the next wave
    }

    public class MapManager : MonoBehaviour
    {
        [Header("Wave")]
        [SerializeField] private bool enableWave;

        [SerializeField] private float waveTimeInterval;

        [Header("Levels")]
        [SerializeField] private int levelsNum;

        [SerializeField] private Vector3 offset;
        [SerializeField] private List<LevelController> levelsControllers;

        [Header("Rooms")]
        [SerializeField] private List<RoomController> roomsControllers;

        [SerializeField] private List<RoomType> roomsTypes;
        [SerializeField] private List<int> entitiesInRooms;
        [SerializeField] private List<EntityRoomStatus> entityRoomStatus;
        [SerializeField] private List<float> damagePerEntity;
        [SerializeField] private List<bool> roomsCanFire;

        [Header("Resources")]
        [SerializeField] private int startingLevelsNum;

        [SerializeField] private GameObject levelTemplate;
        [SerializeField] private Transform mapRootPoint;
        [SerializeField] private Transform startingPoint;

        [SerializeField] private float currentWaveTimer;

        private Dictionary<Mob, int> entitiesToId;
        private Dictionary<int, Mob> idToEntities;

        private int mobGenID;
        private int roomGenID;

        private void Awake()
        {
            mobGenID = 0;
            roomGenID = 0;

            levelsControllers = new List<LevelController>();
            roomsControllers = new List<RoomController>();

            roomsTypes = new List<RoomType>();
            entitiesInRooms = new List<int>();
            damagePerEntity = new List<float>();
            roomsCanFire = new List<bool>();
            entityRoomStatus = new List<EntityRoomStatus>();

            entitiesToId = new Dictionary<Mob, int>();
            idToEntities = new Dictionary<int, Mob>();

            currentWaveTimer = 0;

            CreateLayout(startingLevelsNum);
        }

        public void Update()
        {
            if (enableWave)
            {
                currentWaveTimer += Time.deltaTime;

                if (currentWaveTimer > waveTimeInterval)
                {
                    currentWaveTimer = 0f;
                    CreateWave();
                }
            }
        }


        private void LateUpdate()
        {
            for (var entityId = 0; entityId < entityRoomStatus.Count; entityId++)
            {
                if (entityRoomStatus[entityId] == EntityRoomStatus.Entered)
                {
                    idToEntities[entityId].StartMovingInRoom(roomsControllers[entitiesInRooms[entityId]]);
                    SetMobRoomStatus(entityId, EntityRoomStatus.Moving);
                }

                if (entityRoomStatus[entityId] == EntityRoomStatus.Exit)
                {
                    if (HasMobPassedTheFinalRoom(entityId))
                    {
                        SetMobRoomStatus(entityId, EntityRoomStatus.Retired);
                    }
                    else
                    {
                        MoveMobToNextRoom(entityId);
                    }
                }
            }

            ApplyDamageFromRooms();
            RetireMobs();
        }

        #region Wave Gameplay

        public void CreateWave()
        {
            for (var entityId = 0; entityId < entityRoomStatus.Count; entityId++)
            {
                if (entityRoomStatus[entityId] == EntityRoomStatus.Retired)
                {
                    MoveMobToRoom(entityId, 0);
                    SetMobRoomStatus(entityId, EntityRoomStatus.Entered);
                }
            }
        }

        #endregion


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
                var obj = Instantiate(levelTemplate, mapRootPoint.position + (levelsNum + i) * offset, Quaternion.identity);
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
                RegisterRoom(room);
            }
        }

        private void RegisterRoom(RoomController room)
        {
            room.ID = GenRoomID();
            room.CanFire = true;
            room.MapManager = this;

            roomsTypes.Add(RoomType.Empty);
            roomsCanFire.Add(false);
            roomsControllers.Add(room);
            room.UpdateRoomName();
        }

        public void MarkRoomToFire(int roomId)
        {
            roomsCanFire[roomId] = true;
        }

        private int GenRoomID()
        {
            var id = roomGenID;
            roomGenID += 1;
            return id;
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
                    ApplyDamageToMob(entityId);
                    roomsCanFire[room] = false;
                    roomsControllers[room].ResetFire();
                }
            }
        }

        private void RetireMobs()
        {
            for (var entityId = 0; entityId < entityRoomStatus.Count; entityId++)
            {
                if (entityRoomStatus[entityId] == EntityRoomStatus.Retired)
                {
                    RetireMob(entityId);
                }
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
            return Mathf.Min(entitiesInRooms[entityId] + 1, roomsControllers.Count - 1);
        }

        public void MoveMobToNextRoom(int entityId)
        {
            MoveMobToRoom(entityId, GetNextRoomForMob(entityId));
            SetMobRoomStatus(entityId, EntityRoomStatus.Entered);
        }

        public void SetMobRoomStatus(int entityId, EntityRoomStatus status)
        {
            entityRoomStatus[entityId] = status;
        }

        private bool HasMobPassedTheFinalRoom(int entityId)
        {
            return entitiesInRooms[entityId] == roomsControllers.Count - 1;
        }

        #endregion

        #region Mob Gameplay

        private int GenMobID()
        {
            var id = mobGenID;
            mobGenID += 1;
            return id;
        }

        public void RegisterMob(Mob mob)
        {
            mob.id = GenMobID();

            entitiesInRooms.Add(0);
            entityRoomStatus.Add(EntityRoomStatus.Entered);
            entitiesToId.Add(mob, mob.id);
            idToEntities.Add(mob.id, mob);
            damagePerEntity.Add(0);
        }

        private void ApplyDamageToMob(int entityId)
        {
            idToEntities[entityId].ApplyDamage(damagePerEntity[entityId]);
        }

        private void MoveMobToStartingPoint(int entityId)
        {
            idToEntities[entityId].RelocateToPosition(startingPoint.transform.position);
        }

        private void RetireMob(int entityId)
        {
            MoveMobToStartingPoint(entityId);
        }

        #endregion

    }
}