using System;
using System.Collections.Generic;
using GameEntities;
using Map.Room;
using UI;
using UnityEngine;

namespace Map
{


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

        [SerializeField] private List<int> entitiesInRooms;
        [SerializeField] private List<EntityRoomStatus> entityRoomStatus;
        [SerializeField] private List<float> damagePerEntity;
        [SerializeField] private List<bool> roomsCanFire;

        [Header("Resources")]
        [SerializeField] private int startingLevelsNum;

        [SerializeField] private GameObject levelTemplate;
        [SerializeField] private Transform mapRootPoint;
        [SerializeField] private Transform startingPoint;

        [Header("Models")]
        [SerializeField] private RoomModels roomModels;

        [Header("Internals")]
        [SerializeField] private float currentWaveTimer;

        public UIState uiState;

        [SerializeField] private int selectedRoomId;

        private Dictionary<Mob, int> entitiesToId;
        private Dictionary<int, Mob> idToEntities;

        private int mobGenID;
        private int roomGenID;
        private Dictionary<RoomType, float> roomsDamage;

        private void Awake()
        {
            mobGenID = 0;
            roomGenID = 0;

            levelsControllers = new List<LevelController>();
            roomsControllers = new List<RoomController>();

            entitiesInRooms = new List<int>();
            damagePerEntity = new List<float>();
            roomsCanFire = new List<bool>();
            entityRoomStatus = new List<EntityRoomStatus>();

            entitiesToId = new Dictionary<Mob, int>();
            idToEntities = new Dictionary<int, Mob>();
            roomsDamage = new Dictionary<RoomType, float>();

            currentWaveTimer = 0;

            CreateLayout(startingLevelsNum);
            UpdateRoomDamageFromModels();
        }

        private void Start()
        {
            OnInitUI?.Invoke();
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

        public event Action OnInitUI;
        public event Action<int> OnSelectedRoomChange;

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
            damagePerEntity[entityId] = GetDamageFromRoom(roomId);
            var roomState = roomsControllers[roomId].GetState();

            switch (roomState.verticalSym)
            {
                case SymbolStateV.None:
                    break;
                case SymbolStateV.Top:
                    damagePerEntity[entityId] += GetDamageFromRoom(entityId + 3);
                    break;
                case SymbolStateV.Bottom:
                    damagePerEntity[entityId] += GetDamageFromRoom(entityId - 3);
                    break;
                case SymbolStateV.TopAndBottom:
                    damagePerEntity[entityId] += GetDamageFromRoom(entityId + 3);
                    damagePerEntity[entityId] += GetDamageFromRoom(entityId - 3);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (roomState.horizontalSym)
            {
                case SymbolStateH.None:
                    break;
                case SymbolStateH.Left:
                    damagePerEntity[entityId] += GetDamageFromRoom(entityId - 1);
                    break;
                case SymbolStateH.Right:
                    damagePerEntity[entityId] += GetDamageFromRoom(entityId + 1);
                    break;
                case SymbolStateH.RightAndLeft:
                    damagePerEntity[entityId] += GetDamageFromRoom(entityId - 1);
                    damagePerEntity[entityId] += GetDamageFromRoom(entityId + 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private float GetDamageFromRoom(int roomId)
        {
            if (roomId >= roomsControllers.Count)
            {
                return 0f;
            }
            var roomState = roomsControllers[roomId].GetState();
            return roomsDamage.TryGetValue(roomState.roomType, out var damage) ? damage : 0f;
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

        private void UpdateRoomDamageFromModels()
        {
            foreach (var roomModel in roomModels.list)
            {
                roomsDamage.Add(roomModel.roomType, roomModel.damage);
            }
        }

        public int SelectedRoomId
        {
            get => selectedRoomId;
            set => selectedRoomId = value;
        }

        public void ChangeSelectedRoomWithEvent(int roomId)
        {
            SelectedRoomId = roomId;
            OnSelectedRoomChange?.Invoke(roomId);
        }

        public RoomModels RoomModels => roomModels;

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