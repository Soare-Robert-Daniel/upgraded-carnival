using System;
using System.Collections.Generic;
using System.Linq;
using Economy;
using GameEntities;
using Map.Room;
using UI;
using UnityEngine;

namespace Map
{

    public enum EntityRoomStatus
    {
        Entered, // Just entered in the room.
        Moving, // Moving through the room.
        Exit, // Need to exit and enter the next room.
        Retired, // Should retired from the scene to the spawn point.
        ReadyToSpawn // Waiting to re-enter on the next wave.
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
        [SerializeField] private EconomyController economyController;

        [Header("Models")]
        [SerializeField] private RoomModels roomModels;

        [SerializeField] private EntityModels entityModels;

        [Header("Internals")]
        [SerializeField] private float currentWaveTimer;

        [SerializeField] private int currentMobsPerWave;

        public UIState uiState;

        [SerializeField] private int selectedRoomId;

        private Dictionary<Mob, int> entitiesToId;
        private Dictionary<int, Mob> idToEntities;

        private int mobGenID;
        private int roomGenID;
        private Dictionary<RoomType, float> roomsDamagePerType;
        private Stack<int> roomsToDisarm;


        public EconomyController EconomyController => economyController;

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
            roomsDamagePerType = new Dictionary<RoomType, float>();
            roomsToDisarm = new Stack<int>();

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
            UpdateMobsRunes();

            if (enableWave)
            {
                currentWaveTimer += Time.deltaTime;
                OnNextWaveTimeChanged?.Invoke(waveTimeInterval - currentWaveTimer);

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

            AddRunesToMobsFromRooms();
            ApplyDamageFromRooms();
            RetireMobs();
            DisarmRooms();
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
        public event Action<float> OnNextWaveTimeChanged;

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

        private bool IsSelectedRoomValid()
        {
            return 0 <= selectedRoomId && selectedRoomId < roomsControllers.Count;
        }

        private void ChangeRoomTypeForSelectedRoom(RoomType roomType)
        {
            if (!IsSelectedRoomValid()) return;
            foreach (var roomModel in RoomModels)
            {
                if (roomModel.roomType == roomType)
                {
                    if (economyController.CanSpend(Currency.Gold, roomModel.price))
                    {
                        economyController.SpendCurrency(Currency.Gold, roomModel.price);
                        roomsControllers[selectedRoomId].RoomState.LoadFromModel(roomModel);
                        roomsControllers[selectedRoomId].UpdateVisual(roomModel);
                    }

                    break;
                }
            }
        }

        public void TryBuyRoomForSelectedRoom(RoomType roomType)
        {
            ChangeRoomTypeForSelectedRoom(roomType);
        }

        #endregion

        #region Room Gameplay

        private void AddRunesToMobsFromRooms()
        {
            for (var entityId = 0; entityId < entitiesInRooms.Count; entityId++)
            {
                var roomId = entitiesInRooms[entityId];
                if (roomsCanFire[roomId])
                {
                    AddRunesToMobFromRoom(entityId, roomId);
                    AddToDisarmRoom(roomId);
                }
            }
        }

        private void AddRunesToMobFromRoom(int entityId, int roomId)
        {
            var roomType = roomsControllers[roomId].RoomState.RoomType;
            var mob = idToEntities[entityId];

            switch (roomType)
            {
                case RoomType.Empty:
                    break;
                case RoomType.SlowRune:
                    mob.stats.AddRune(rune: new Rune().SetRuneType(RuneType.Slow).SetDuration(120));
                    break;
                case RoomType.AttackRune:
                    mob.stats.AddRune(rune: new Rune().SetRuneType(RuneType.Attack).SetDuration(120));
                    break;
                case RoomType.ConstantAttackFire:
                    break;
                case RoomType.BurstAttackFire:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ApplyDamageFromRooms()
        {
            // I wonder if we can put this on a Job.
            for (var entityId = 0; entityId < entitiesInRooms.Count; entityId++)
            {
                var roomId = entitiesInRooms[entityId];
                if (roomsCanFire[roomId])
                {

                    ApplyRuneActionToEntityFromRoom(entityId, roomId);
                    ApplyDamageToMob(entityId);
                    AddToDisarmRoom(roomId);
                    ShouldRetire(entityId);
                }
            }
        }

        private void ResetRoomFireStatus(int roomId)
        {
            roomsCanFire[roomId] = false;
            roomsControllers[roomId].ResetFire();
        }

        private void ShouldRetire(int entityId)
        {
            if (!idToEntities[entityId].stats.IsAlive())
            {
                SetMobRoomStatus(entityId, EntityRoomStatus.Retired);
            }
        }

        private void RetireMobs()
        {
            for (var entityId = 0; entityId < entityRoomStatus.Count; entityId++)
            {
                if (entityRoomStatus[entityId] == EntityRoomStatus.Retired)
                {
                    RetireMob(entityId);
                    CollectBountyFromMob(entityId);
                    SetMobRoomStatus(entityId, EntityRoomStatus.ReadyToSpawn);
                }
            }
        }

        private void ApplyRuneActionToEntityFromRoom(int entityId, int roomId)
        {
            damagePerEntity[entityId] = 0f;

            var roomState = roomsControllers[roomId].RoomState;
            var roomType = roomsControllers[roomId].RoomState.RoomType;
            var mob = idToEntities[entityId];

            switch (roomType)
            {
                case RoomType.Empty:
                    break;
                case RoomType.SlowRune:
                    mob.AdjustSpeed();
                    break;
                case RoomType.AttackRune:
                    break;
                case RoomType.ConstantAttackFire:
                    damagePerEntity[entityId] = 5f; // flat damage
                    damagePerEntity[entityId] += mob.stats.Runes.Count(x => x.Type == RuneType.Attack) * 1f;
                    break;
                case RoomType.BurstAttackFire:
                    damagePerEntity[entityId] = mob.stats.Runes.Count(x => x.Type == RuneType.Attack) * 5f;
                    mob.stats.RemoveRuneType(RuneType.Attack);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (roomState.VerticalSym)
            {
                case SymbolStateV.None:
                    break;
                case SymbolStateV.Top:

                    break;
                case SymbolStateV.Bottom:

                    break;
                case SymbolStateV.TopAndBottom:

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (roomState.HorizontalSym)
            {
                case SymbolStateH.None:
                    break;
                case SymbolStateH.Left:

                    break;
                case SymbolStateH.Right:

                    break;
                case SymbolStateH.RightAndLeft:

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            foreach (var roomModel in RoomModels)
            {
                roomsDamagePerType.Add(roomModel.roomType, 1f);
            }
        }

        public int SelectedRoomId
        {
            get => selectedRoomId;
            set => selectedRoomId = value;
        }

        public void ChangeSelectedRoomWithEvent(int roomId)
        {
            if (IsSelectedRoomValid())
            {
                roomsControllers[SelectedRoomId].Deselect();
            }

            SelectedRoomId = roomId;
            if (!IsSelectedRoomValid())
            {
                return;
            }

            roomsControllers[roomId].Select();
            OnSelectedRoomChange?.Invoke(roomId);
        }

        private void AddToDisarmRoom(int roomId)
        {
            roomsToDisarm.Push(roomId);
        }

        private void DisarmRooms()
        {
            while (roomsToDisarm.Count > 0)
            {
                ResetRoomFireStatus(roomsToDisarm.Pop());
            }
        }

        public List<RoomModel> RoomModels => roomModels.list;

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

        private void UpdateMobsRunes()
        {
            var time = Time.deltaTime;
            foreach (var mob in idToEntities.Values)
            {
                mob.stats.DecreaseRunesDuration(time);
                mob.stats.RemoveExpiredRunes();
            }
        }

        private void CollectBountyFromMob(int entityId)
        {
            economyController.AddCurrency(idToEntities[entityId].bounty.currency, idToEntities[entityId].bounty.value);
        }

        #endregion

    }
}