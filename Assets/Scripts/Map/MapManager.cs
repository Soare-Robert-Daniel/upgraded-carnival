using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Economy;
using GameEntities;
using Map.Components;
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

        [SerializeField] private int waveNumber;
        [SerializeField] private float waveTimeInterval;
        [SerializeField] private float waveMobSpawnStartOffset;

        [Header("Levels")]
        [SerializeField] private int levelsNum;

        [SerializeField] private float currentPricePerLevel;
        [SerializeField] private float pricePerLevelMultiplier;
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

        [SerializeField] private GameObject mobTemplate;
        [SerializeField] private GameObject levelTemplate;
        [SerializeField] private Transform mapRootPoint;
        [SerializeField] private Transform mobStartingPoint;
        [SerializeField] private EconomyController economyController;

        [Header("Models")]
        [SerializeField] private RoomModels roomModels;

        [SerializeField] private EntityModels entityModels;
        [SerializeField] private WaveMobNumberPerWaveModel waveMobNumberPerWaveModel;

        [Header("Internals")]
        [SerializeField] private float currentWaveTimer;

        [SerializeField] private int currentMobsPerWave;
        [SerializeField] private bool wavePrepared;
        [SerializeField] private List<float> spawnStartPerMob;
        [SerializeField] private RuneStorage runeStorage;

        public UIState uiState;

        [SerializeField] private int selectedRoomId;

        private Dictionary<Mob, int> _entitiesToId;
        private Dictionary<int, Mob> _idToEntities;

        private int _mobGenID;
        private Dictionary<RoomType, RoomDamageStrategy> _roomDamageStrategies;
        private int _roomGenID;
        private Dictionary<RoomType, float> _roomsDamagePerType;
        private Stack<int> _roomsToDisarm;

        private Dictionary<RoomType, RoomModel> _roomTypeToModels;


        public EconomyController EconomyController => economyController;

        private void Awake()
        {
            _mobGenID = 0;
            _roomGenID = 0;
            levelsNum = 0;

            levelsControllers = new List<LevelController>();
            roomsControllers = new List<RoomController>();

            entitiesInRooms = new List<int>();
            damagePerEntity = new List<float>();
            roomsCanFire = new List<bool>();
            entityRoomStatus = new List<EntityRoomStatus>();

            _entitiesToId = new Dictionary<Mob, int>();
            _idToEntities = new Dictionary<int, Mob>();
            _roomsDamagePerType = new Dictionary<RoomType, float>();
            _roomsToDisarm = new Stack<int>();
            spawnStartPerMob = new List<float>();
            _roomDamageStrategies = new Dictionary<RoomType, RoomDamageStrategy>();

            currentWaveTimer = 0;
            wavePrepared = false;

            runeStorage = new RuneStorage(200);

            _roomTypeToModels = new Dictionary<RoomType, RoomModel>();
            foreach (var roomModel in roomModels.list)
            {
                _roomTypeToModels.Add(roomModel.roomType, roomModel);

                var strategy = new RoomDamageStrategy();
                strategy.LoadFromModel(roomModel);
                _roomDamageStrategies.Add(roomModel.roomType, strategy);
            }

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
            }
        }

        private void LateUpdate()
        {
            for (var entityId = 0; entityId < entityRoomStatus.Count; entityId++)
            {
                if (entityRoomStatus[entityId] == EntityRoomStatus.Entered)
                {
                    _idToEntities[entityId].StartMovingInRoom(roomsControllers[entitiesInRooms[entityId]]);
                    SetMobRoomStatus(entityId, EntityRoomStatus.Moving);
                }

                if (entityRoomStatus[entityId] == EntityRoomStatus.Exit)
                {
                    if (HasMobPassedTheFinalRoom(entityId))
                    {
                        SetMobRoomStatus(entityId, EntityRoomStatus.Retired);
                        OnMobReachedFinalRoom?.Invoke(_idToEntities[entityId]);
                    }
                    else
                    {
                        MoveMobToNextRoom(entityId);
                    }
                }
            }

            runeStorage.UpdateRunesDurations(Time.deltaTime);
            HandleRuneAndDamagePerEntity();
            RetireMobs();
            DisarmRooms();

            if (!wavePrepared && waveTimeInterval - currentWaveTimer < 5f)
            {
                PrepareNextRound();
            }

            if (currentWaveTimer > waveTimeInterval)
            {
                StartWave();
            }

            CheckAndStartMobs();
        }

        #region Wave Gameplay

        public void StartWave()
        {
            currentWaveTimer = 0f;
            waveNumber += 1;
            CreateWave();
            OnNextWaveStarted?.Invoke(waveNumber);
        }

        public IEnumerator ManualStartWave()
        {
            PrepareNextRound();
            yield return new WaitForSeconds(1.0f);
            StartWave();
        }

        public void CreateWave()
        {
            wavePrepared = false;
            var startTime = 0f;
            for (var entityId = 0; entityId < entityRoomStatus.Count; entityId++)
            {
                if (entityRoomStatus[entityId] == EntityRoomStatus.Retired)
                {
                    SetMobRoomStatus(entityId, EntityRoomStatus.ReadyToSpawn);
                }

                if (entityRoomStatus[entityId] == EntityRoomStatus.ReadyToSpawn)
                {
                    spawnStartPerMob[entityId] = startTime;
                    startTime += waveMobSpawnStartOffset;
                }
            }
        }

        public void PrepareNextRound()
        {
            wavePrepared = true;

            // Refactor this.
            foreach (var mobsNumberInterval in waveMobNumberPerWaveModel.intervals)
            {
                if (mobsNumberInterval.fromWave <= waveNumber + 1) // We prepare in advance.
                {
                    currentMobsPerWave = mobsNumberInterval.mobsNumber;
                }
                else
                {
                    break;
                }
            }

            ReadjustMobNumbers();
        }

        private void ReadjustMobNumbers()
        {
            // Create more if we don't have enough _available_ mobs.
            // TODO: Add new _available_ mobs 

            // Count the mobs with status ReadyToSpawn or Retied.
            var readyToSpawnMobs = entityRoomStatus.Count(t => t is EntityRoomStatus.ReadyToSpawn or EntityRoomStatus.Retired);

            if (currentMobsPerWave > readyToSpawnMobs)
            {
                Debug.Log($"Create {currentMobsPerWave - readyToSpawnMobs} mobs");
                for (var i = 0; i < currentMobsPerWave - readyToSpawnMobs; i++)
                {
                    CreateMob();
                }
            }
        }

        private void CheckAndStartMobs()
        {
            for (var entityId = 0; entityId < entityRoomStatus.Count; entityId++)
            {
                if (entityRoomStatus[entityId] != EntityRoomStatus.ReadyToSpawn || !(spawnStartPerMob[entityId] < currentWaveTimer)) continue;
                MoveMobToRoom(entityId, 0);
                SetMobRoomStatus(entityId, EntityRoomStatus.Entered);
            }
        }

        private void CreateMob()
        {
            // By letting the room to register himself in Start, it will be available for next round.
            var mobObj = Instantiate(mobTemplate, mobStartingPoint.transform);
            var mobController = mobObj.GetComponent<Mob>();
            if (mobController == null)
            {
                Debug.LogError("The Mob template DOES NOT HAVE THE CONTROLLER");
                return;
            }

            mobController.Manager = this;
        }

        #endregion

        #region Events

        public event Action OnInitUI;
        public event Action<int> OnSelectedRoomChange;
        public event Action<float> OnNextWaveTimeChanged;
        public event Action<int> OnNextWaveStarted;

        public event Action<int, float> OnLevelUpdated;

        public event Action<Mob> OnMobReachedFinalRoom;

        #endregion

        #region Map Generation

        public void CreateLayout(int levels)
        {
            if (levels < levelsNum)
            {
                return;
            }

            while (levels > levelsNum)
            {
                AddLevelToLayout();
            }
        }

        public void AddLevelToLayout()
        {
            var obj = Instantiate(levelTemplate, mapRootPoint.position + levelsNum * offset, Quaternion.identity);
            var levelController = obj.GetComponent<LevelController>();
            RegisterLevel(levelController);
            levelsNum += 1;
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
            var id = _roomGenID;
            _roomGenID += 1;
            return id;
        }

        private bool IsSelectedRoomValid()
        {
            return 0 <= selectedRoomId && selectedRoomId < roomsControllers.Count;
        }

        private void ChangeRoomTypeForSelectedRoom(RoomType roomType)
        {
            roomsControllers[selectedRoomId].RoomSettings.LoadFromModel(_roomTypeToModels[roomType]);
            roomsControllers[selectedRoomId].UpdateVisual(_roomTypeToModels[roomType]);
        }

        public void TryBuyRoomForSelectedRoom(RoomType roomType)
        {
            if (!IsSelectedRoomValid()) return;
            if (!economyController.CanSpend(Currency.Gold, _roomTypeToModels[roomType].price)) return;

            economyController.SpendCurrency(Currency.Gold, _roomTypeToModels[roomType].price);
            ChangeRoomTypeForSelectedRoom(roomType);
        }

        public void TryBuyLevel()
        {
            if (!economyController.CanSpend(Currency.Gold, currentPricePerLevel)) return;

            economyController.SpendCurrency(Currency.Gold, currentPricePerLevel);
            AddLevelToLayout();
            IncreaseBuyLevelPrice();
            OnLevelUpdated?.Invoke(levelsNum, currentPricePerLevel);
        }

        private void IncreaseBuyLevelPrice()
        {
            // FUTURE: Make this a model.
            currentPricePerLevel = MathF.Round(currentPricePerLevel * pricePerLevelMultiplier);
        }

        public float NewCurrentPricePerLevel => currentPricePerLevel;

        #endregion

        #region Room Gameplay

        private void HandleRuneAndDamagePerEntity()
        {
            // I wonder if we can put this on a Job.
            for (var entityId = 0; entityId < entitiesInRooms.Count; entityId++)
            {
                var roomId = entitiesInRooms[entityId];
                if (!roomsCanFire[roomId]) continue;

                var mob = _idToEntities[entityId];
                var roomType = roomsControllers[roomId].RoomSettings.RoomType;

                _roomDamageStrategies[roomType].AddRunesToStorageForEntity(runeStorage, entityId);

                damagePerEntity[entityId] = _roomDamageStrategies[roomType].ComputeDamageForEntityWithRuneStorage(runeStorage, entityId);

                ApplyDamageToMob(entityId);

                _roomDamageStrategies[roomType].RemoveRunesFromStorageForEntity(runeStorage, entityId);

                mob.AdjustSpeed();

                AddToDisarmRoom(roomId);
                ShouldRetire(entityId);
            }
        }

        private void ResetRoomFireStatus(int roomId)
        {
            roomsCanFire[roomId] = false;
            roomsControllers[roomId].ResetFire();
        }

        private void ShouldRetire(int entityId)
        {
            if (!_idToEntities[entityId].stats.IsAlive())
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
                _roomsDamagePerType.Add(roomModel.roomType, 1f);
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
            _roomsToDisarm.Push(roomId);
        }

        private void DisarmRooms()
        {
            while (_roomsToDisarm.Count > 0)
            {
                ResetRoomFireStatus(_roomsToDisarm.Pop());
            }
        }

        public List<RoomModel> RoomModels => roomModels.list;

        #endregion

        #region Mob Gameplay

        private int GenMobID()
        {
            var id = _mobGenID;
            _mobGenID += 1;
            return id;
        }

        public void RegisterMob(Mob mob)
        {
            mob.id = GenMobID();

            entitiesInRooms.Add(0);
            entityRoomStatus.Add(EntityRoomStatus.ReadyToSpawn
            );
            _entitiesToId.Add(mob, mob.id);
            _idToEntities.Add(mob.id, mob);
            spawnStartPerMob.Add(3000f);
            damagePerEntity.Add(0);
        }

        private void ApplyDamageToMob(int entityId)
        {
            _idToEntities[entityId].ApplyDamage(damagePerEntity[entityId]);
        }

        private void MoveMobToStartingPoint(int entityId)
        {
            _idToEntities[entityId].RelocateToPosition(mobStartingPoint.transform.position);
        }

        private void RetireMob(int entityId)
        {
            MoveMobToStartingPoint(entityId);
        }

        private void UpdateMobsRunes()
        {
            var time = Time.deltaTime;
            foreach (var mob in _idToEntities.Values)
            {
                mob.stats.DecreaseRunesDuration(time);
                mob.stats.RemoveExpiredRunes();
            }
        }

        private void CollectBountyFromMob(int entityId)
        {
            economyController.AddCurrency(_idToEntities[entityId].bounty.currency, _idToEntities[entityId].bounty.value);
        }

        #endregion

    }
}