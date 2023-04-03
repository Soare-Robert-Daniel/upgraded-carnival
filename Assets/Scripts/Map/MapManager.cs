using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Economy;
using GameEntities;
using Map.Components;
using Map.Room;
using UI;
using Unity.Jobs;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Map
{

    public enum EntityRoomStatus
    {
        ReadyToSpawn, // Waiting to re-enter on the next wave.
        Entered, // Just entered in the room.
        Moving, // Moving through the room.
        Exiting, // Need to exit and enter the next room.
        Retiring // Should retired from the scene to the spawn point. The mob is dead.
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

        [SerializeField] private List<EntityRoomStatus> entityRoomStatus;


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

        [SerializeField] private float currentSpawnTimer;

        [SerializeField] private int currentMobsPerWave;
        [SerializeField] private bool wavePrepared;
        [SerializeField] private Wave waveCreator;
        [SerializeField] private Path path;

        [Header("Systems")]
        [SerializeField] private RuneStorage runeStorage;

        [SerializeField] private MobsSystem mobsSystem;
        [SerializeField] private RoomsSystem roomsSystem;
        [SerializeField] private MobsControllerSystem mobsControllerSystem;

        public UIState uiState;

        [SerializeField] private int selectedRoomId;
        private Dictionary<MobClassType, MobModel> entityClassToModel;
        private JobHandle jobHandle;

        private int mobGenID;

        private int roomGenID;
        private Dictionary<RoomType, RoomRuneHandler> roomRuneHandlers;
        private Dictionary<RoomType, RoomModel> roomTypeToModels;

        private Stopwatch stopwatch;

        public EconomyController EconomyController => economyController;

        private void Awake()
        {
            mobGenID = 0;
            roomGenID = 0;
            levelsNum = 0;

            levelsControllers = new List<LevelController>();
            roomsControllers = new List<RoomController>();

            roomRuneHandlers = new Dictionary<RoomType, RoomRuneHandler>();

            currentWaveTimer = 0;
            currentSpawnTimer = 0;
            wavePrepared = false;

            runeStorage = new RuneStorage(1000);
            roomsSystem = new RoomsSystem(100);
            mobsSystem = new MobsSystem(100);
            mobsControllerSystem = new MobsControllerSystem(100);
            path = new Path(100);

            roomTypeToModels = new Dictionary<RoomType, RoomModel>();
            foreach (var roomModel in roomModels.list)
            {
                roomTypeToModels.Add(roomModel.roomType, roomModel);

                var strategy = new RoomRuneHandler();
                strategy.LoadFromModel(roomModel);
                roomRuneHandlers.Add(roomModel.roomType, strategy);
            }

            entityClassToModel = new Dictionary<MobClassType, MobModel>();
            foreach (var entityModel in entityModels.List)
            {
                entityClassToModel.Add(entityModel.stats.mobClass, entityModel);
            }

            waveCreator = new Wave(waveMobNumberPerWaveModel, entityClassToModel);

            CreateLayout(startingLevelsNum);

            stopwatch = new Stopwatch();
        }

        private void Start()
        {
            OnInitUI?.Invoke();
        }

        public void Update()
        {
            // UpdateMobsRunes();

            if (enableWave)
            {
                currentWaveTimer += Time.deltaTime;
                currentSpawnTimer += Time.deltaTime;
                OnNextWaveTimeChanged?.Invoke(waveTimeInterval - currentWaveTimer);
            }

            stopwatch.Reset();
            stopwatch.Start();

            if (mobsControllerSystem.GetMobControllersCount() > 0)
            {
                mobsControllerSystem.UpdateMobsNextPosition(
                    mobsSystem.GetMobsSpeedArray(),
                    mobsSystem.GetMobsSlowArray(),
                    Time.deltaTime
                );

                mobsControllerSystem.UpdateMobControllersPositions();
            }
        }

        private void LateUpdate()
        {
            if (waveCreator.HasMobsToSpawn() && currentSpawnTimer > waveMobSpawnStartOffset)
            {
                if (TrySpawnMob())
                {
                    currentSpawnTimer = 0;
                }
            }

            mobsSystem.UpdateMobsRoomStatus(mobsControllerSystem.GetMobControllersPositionsArray(), path);

            for (var mobId = 0; mobId < mobsSystem.GetMobCount(); mobId++)
            {
                switch (mobsSystem.GetRoomStatusFor(mobId))
                {
                    case EntityRoomStatus.Entered:
                        mobsSystem.SetRoomStatus(mobId, EntityRoomStatus.Moving);
                        if (path.NeedToTeleport(mobsControllerSystem.GetMobPosition(mobId), mobsSystem.GetLocationRoomIndex(mobId)))
                        {
                            mobsControllerSystem.SetMobPosition(mobId, path.GetEnterPoint(mobsSystem.GetLocationRoomIndex(mobId)));
                        }
                        break;
                    case EntityRoomStatus.Moving:
                        break;
                    case EntityRoomStatus.Exiting:
                        if (HasMobPassedTheFinalRoom(mobId))
                        {
                            mobsSystem.SetRoomStatus(mobId, EntityRoomStatus.Retiring);
                            OnMobReachedFinalRoom?.Invoke(mobsControllerSystem.GetController(mobId));
                        }
                        break;
                    case EntityRoomStatus.Retiring:
                        MoveMobToStartingPoint(mobId);
                        break;
                    case EntityRoomStatus.ReadyToSpawn:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            mobsSystem.MoveMobsToNextRooms(roomsControllers.Count);

            roomsSystem.UpdateRoomsAttackTime(Time.deltaTime);
            runeStorage.UpdateRunesDurations(Time.deltaTime);
            HandleRuneAndDamagePerMob();
            runeStorage.TransferBufferToStorage();

            CollectBountyFromMobs();

            mobsSystem.MoveRetiringMobsToDeploy();
            roomsSystem.DisarmRooms();

            if (currentWaveTimer > waveTimeInterval)
            {
                StartWave();
            }

            stopwatch.Stop();

            OnMapLogicTimeChanged?.Invoke(stopwatch.ElapsedMilliseconds);
        }
        private bool TrySpawnMob()
        {
            if (mobsSystem.ReadyToDeployMobCount() == 0 || !waveCreator.HasMobsToSpawn())
                return false;

            var mobData = waveCreator.DequeueMobsToSpawn().First();
            var mobId = mobsSystem.PullMobsToDeploy().First();

            mobsSystem.SetBounty(mobId, mobData.Bounty);
            mobsSystem.SetRoomIndex(mobId, 0);
            mobsSystem.SetRoomStatus(mobId, EntityRoomStatus.Entered);
            mobsSystem.SetHealth(mobId, mobData.Stats.health);
            mobsSystem.SetSpeed(mobId, mobData.Stats.speed);
            mobsSystem.ResetSlowFor(mobId);

            return true;
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
            yield return new WaitForSeconds(1.0f);
            CreateWave();
        }

        public void CreateWave()
        {
            wavePrepared = false;

            waveCreator.SetWave(waveNumber);
            waveCreator.CreateMobsToSpawn();
            ReadjustMobNumbers();
        }


        private void ReadjustMobNumbers()
        {
            // If there are fewer mobs to deploy than the number of the wave, we need to add more mobs to the pool.
            var mobsToDeploy = waveCreator.GetMobsToSpawnCount();
            var mobsInPool = mobsSystem.ReadyToDeployMobCount();
            var mobsToAdd = mobsToDeploy - mobsInPool;
            Debug.Log($"Mobs to deploy: {mobsToDeploy}, Mobs in pool: {mobsInPool}, Mobs to add: {mobsToAdd}");
            if (mobsToAdd > 0)
            {
                AddMobsToPool(mobsToAdd);
            }

        }
        private void AddMobsToPool(int mobsToAdd)
        {
            for (var i = 0; i < mobsToAdd; i++)
            {
                CreateMobAndAddToPool();
            }
        }

        private void CreateMobAndAddToPool(MobClassType mobClass = MobClassType.SimpleMobClass)
        {
            Debug.Log("Creating mob and adding to pool");
            // By letting the room to register himself in Start, it will be available for next round.
            var mobObj = Instantiate(mobTemplate, mobStartingPoint.transform);
            var mobController = mobObj.GetComponent<Mob>();
            mobController.UpdateBodySprite(entityClassToModel[mobClass].assets.RenderedSprite);

            if (mobController == null)
            {
                Debug.LogError("The Mob template DOES NOT HAVE THE CONTROLLER");
                return;
            }

            mobController.Manager = this;
            RegisterMob(mobController);
        }

        #endregion

        #region Events

        public event Action OnInitUI;
        public event Action<int> OnSelectedRoomChange;
        public event Action<float> OnNextWaveTimeChanged;
        public event Action<int> OnNextWaveStarted;

        public event Action<int, float> OnLevelUpdated;

        public event Action<Mob> OnMobReachedFinalRoom;

        public event Action<float> OnMapLogicTimeChanged;

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
            room.MapManager = this;

            roomsSystem.AddRoom(RoomType.Empty, 0, room.StartingPointPosition, room.ExitPointPosition);
            path.AddPath(room.StartingPointPosition, room.ExitPointPosition);

            roomsControllers.Add(room);
            room.UpdateRoomName();
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
            roomsControllers[selectedRoomId].RoomSettings.LoadFromModel(roomTypeToModels[roomType]);
            roomsControllers[selectedRoomId].UpdateVisual(roomTypeToModels[roomType]);

            roomsSystem.SetType(selectedRoomId, roomType);
            roomsSystem.SetAttackTimeInterval(selectedRoomId, roomTypeToModels[roomType].fireRate);
        }

        public void TryBuyRoomForSelectedRoom(RoomType roomType)
        {
            if (!IsSelectedRoomValid()) return;
            if (!economyController.CanSpend(Currency.Gold, roomTypeToModels[roomType].price)) return;

            economyController.SpendCurrency(Currency.Gold, roomTypeToModels[roomType].price);
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

        private void HandleRuneAndDamagePerMob()
        {
            // I wonder if we can put this on a Job.
            for (var mobId = 0; mobId < mobsSystem.GetMobCount(); mobId++)
            {
                var roomId = mobsSystem.GetLocationRoomIndex(mobId);
                if (!roomsSystem.CanRoomFire(roomId)) continue;

                var mob = mobsControllerSystem.GetController(mobId);
                var roomType = roomsSystem.GetRoomType(roomId);

                // Rune Storage.
                roomRuneHandlers[roomType].AddRunesToStorageForEntity(runeStorage, mobId);
                roomRuneHandlers[roomType].ComputeDamageAndSlowForMobWithRuneStorage(runeStorage, mobId, out var damage, out var slow);
                roomRuneHandlers[roomType].RemoveRunesFromStorageForEntity(runeStorage, mobId);

                mobsSystem.UpdateDamageReceived(mobId, damage);
                mobsSystem.SetSlow(mobId, slow);

                roomsSystem.AddRoomToDisarm(roomId);

                // Debug.Log($"Mob {mobId} received {damage} damage and {slow} slow.");

                // Update Mob Controller.

                if (damage > 0f)
                {
                    mob.UpdateHealthBar(mobsSystem.GetHealth(mobId) / 100f);
                }
            }

            mobsSystem.ApplyDamageReceivedToMobs();
        }

        private void CollectBountyFromMobs()
        {
            for (var entityId = 0; entityId < entityRoomStatus.Count; entityId++)
            {
                if (entityRoomStatus[entityId] == EntityRoomStatus.Retiring)
                {
                    CollectBountyFromMob(entityId);
                }
            }
        }

        private bool HasMobPassedTheFinalRoom(int mobId)
        {
            return mobsSystem.GetLocationRoomIndex(mobId) == roomsControllers.Count - 1;
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
            mobsSystem.AddMob(0, new SimpleMobClass(), 100, 3f);
            mobsControllerSystem.AddMobController(mob, mobStartingPoint.transform.position);
            // Debug.Log($"Registering mob {mob.id}. Total mobs: {mobsSystem.GetMobCount()}. Total controllers: {mobsControllerSystem.GetMobControllersCount()}.");
        }

        private void MoveMobToStartingPoint(int mobId)
        {
            mobsControllerSystem.SetMobPosition(mobId, mobStartingPoint.transform.position);
        }

        private void CollectBountyFromMob(int mobId)
        {
            var bounty = mobsSystem.GetBounty(mobId);
            economyController.AddCurrency(bounty.currency, bounty.value);
        }

        #endregion

    }
}