using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Economy;
using GameEntities;
using Map.Components;
using Map.Room;
using Mobs;
using Runes;
using Towers.Zones;
using UI;
using Unity.Jobs;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Mob = GameEntities.Mob;

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
        [SerializeField] private List<ZoneController> levelsControllers;

        [Header("Rooms")]
        [SerializeField] private List<RoomController> roomsControllers;

        [Header("Resources")]
        public GlobalResources globalResources;

        [SerializeField] private float databaseCleanUpInterval;

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
        [SerializeField] private int selectedRoomId;

        [SerializeField] private float currentWaveTimer;

        [SerializeField] private float currentSpawnTimer;

        [SerializeField] private float runeDatabaseCleanUpTimer;

        [SerializeField] private int currentMobsPerWave;
        [SerializeField] private bool wavePrepared;
        [SerializeField] private Wave waveCreator;
        [SerializeField] private Path path;
        [SerializeField] private EventChannel eventChannel;

        [SerializeField] private RuneDatabase runeDatabase;

        [Header("Systems")]
        [SerializeField] private RuneStorage runeStorage;

        [SerializeField] private MobsSystem mobsSystem;
        [SerializeField] private RoomsSystem roomsSystem;
        [SerializeField] private MobsControllerSystem mobsControllerSystem;

        [Header("Controllers")]
        [SerializeField] private MobsController mobsController;

        public UIState uiState;
        private Dictionary<MobClassType, MobModel> entityClassToModel;
        private JobHandle jobHandle;

        private int mobGenID;

        private Dictionary<int, int> mobsRoom;
        private int roomGenID;
        private Dictionary<RoomType, RoomRuneHandler> roomRuneHandlers;
        private Dictionary<RoomType, RoomModel> roomTypeToModels;

        private Stopwatch stopwatch;

        public EconomyController EconomyController => economyController;
        public MobsController MobsController => mobsController;

        public List<ZoneController> LevelsControllers => levelsControllers;

        private void Awake()
        {
            mobGenID = 0;
            roomGenID = 0;
            levelsNum = 0;

            levelsControllers = new List<ZoneController>();
            roomsControllers = new List<RoomController>();

            roomRuneHandlers = new Dictionary<RoomType, RoomRuneHandler>();

            currentWaveTimer = 0;
            currentSpawnTimer = 0;
            wavePrepared = false;
            currentMobsPerWave = 0;

            runeStorage = new RuneStorage(1000);
            roomsSystem = new RoomsSystem(100);
            mobsSystem = new MobsSystem(100);
            mobsRoom = new Dictionary<int, int>();

            mobsControllerSystem = new MobsControllerSystem(100);

            path = new Path();
            path.AddPoint(mobStartingPoint.position);
            path.AddPoint(mapRootPoint.position);


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
            runeDatabase = new RuneDatabase();

            stopwatch = new Stopwatch();

            eventChannel.OnZoneTypeChanged += (i, type) =>
            {
                Debug.Log($"Zone type changed: {i} {type}");
            };

            eventChannel.OnZoneControllerAdded += (zoneController) =>
            {
                Debug.Log($"Zone controller added: {zoneController.zoneId}");
            };
        }

        private void Start()
        {
            CreateLayout(startingLevelsNum);
            OnInitUI?.Invoke();
        }

        public void Update()
        {
            // UpdateMobsRunes();

            MobsController.UpdateVisuals();

            if (enableWave)
            {
                currentWaveTimer += Time.deltaTime;
                currentSpawnTimer += Time.deltaTime;
                OnNextWaveTimeChanged?.Invoke(waveTimeInterval - currentWaveTimer);
            }
            databaseCleanUpInterval += Time.deltaTime;

            stopwatch.Reset();
            stopwatch.Start();

            // if (mobsControllerSystem.GetMobControllersCount() > 0)
            // {
            //     mobsControllerSystem.UpdateMobsNextPosition(
            //         mobsSystem.GetMobsSpeedArray(),
            //         mobsSystem.GetMobsSlowArray(),
            //         Time.deltaTime
            //     );
            //
            //     mobsControllerSystem.UpdateMobControllersPositions();
            // }
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

            UpdateMobsRooms();

            // UpdateMobs();

            roomsSystem.UpdateRoomsAttackTime(Time.deltaTime);
            runeStorage.UpdateRunesDurations(Time.deltaTime);

            HandleRuneAndDamagePerMob();

            // mobsSystem.MoveRetiringMobsToDeploy();
            roomsSystem.DisarmRooms();

            if (currentWaveTimer > waveTimeInterval)
            {
                StartWave();
            }

            if (databaseCleanUpInterval > 1f)
            {
                runeDatabase.ManualCleanup();
                databaseCleanUpInterval = 0;
            }

            stopwatch.Stop();
            OnMapLogicTimeChanged?.Invoke(stopwatch.ElapsedMilliseconds);
        }


        private bool TrySpawnMob()
        {
            // Debug.Log($"TrySpawnMob {waveCreator.HasMobsToSpawn()} {mobsController.CanSpawnMob()}");
            if (!waveCreator.HasMobsToSpawn() || !mobsController.CanSpawnMob())
                return false;

            var mobData = waveCreator.DequeueMobsToSpawn().First();
            // var mobId = mobsSystem.PullMobsToDeploy().First();
            //
            // mobsSystem.SetBounty(mobId, mobData.Bounty);
            // mobsSystem.SetRoomIndex(mobId, 0);
            // mobsSystem.SetRoomStatus(mobId, EntityRoomStatus.Entered);
            // mobsSystem.SetHealth(mobId, mobData.Stats.health);
            // mobsSystem.SetSpeed(mobId, mobData.Stats.speed);
            // mobsSystem.ResetSlowFor(mobId);

            var mob = new Mobs.Mob(
                new BaseStats
                {
                    baseHealth = mobData.Stats.health,
                    baseSpeed = mobData.Stats.speed
                })
            {
                id = GenMobID(),
                position = mobStartingPoint.position
            };

            mobsController.SpawnMob(mob);

            return true;
        }

        public bool TryFindClosestMob(Vector3 towerPosition, out Mobs.Mob closestMob)
        {
            closestMob = null;
            if (mobsController.MobsList.Count == 0) return false;

            closestMob = mobsController.MobsList[0];
            var closestDistance = Vector3.Distance(towerPosition, closestMob.position);

            foreach (var mob in mobsController.MobsList)
            {
                var distance = Vector3.Distance(towerPosition, mob.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestMob = mob;
                }
            }

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
            var mobsInPool = mobsController.FreeControllersCount;
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
            var mobObj = Instantiate(mobTemplate, mobStartingPoint.transform.position, Quaternion.identity);
            var mobController = mobObj.GetComponent<Mob>();
            mobController.UpdateBodySprite(entityClassToModel[mobClass].assets.RenderedSprite);

            if (mobController == null)
            {
                Debug.LogError("The Mob template DOES NOT HAVE THE CONTROLLER");
                return;
            }

            mobController.Manager = this;
            // RegisterMob(mobController);

            var simpleMobController = mobObj.GetComponent<MobSimpleController>();

            if (simpleMobController == null)
            {
                Debug.LogError("The Mob template DOES NOT HAVE THE CONTROLLER");
                return;
            }

            simpleMobController.transform.position = mobStartingPoint.position;
            mobsController.AddMobController(simpleMobController);
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

        public event Action<int, RoomController> OnRegisterZone;

        public event Action<int, ZoneTokenType> OnZoneTypeChanged;

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
                AddZoneToLayout();
            }
        }

        public void AddZoneToLayout()
        {
            var obj = Instantiate(levelTemplate, mapRootPoint.position + levelsNum * offset, Quaternion.identity);
            mobStartingPoint.transform.position = mapRootPoint.position + (levelsNum + 1) * offset;
            var levelController = obj.GetComponent<ZoneController>();
            RegisterZone(levelController);
            levelsNum += 1;
        }

        private void RegisterZone(ZoneController zone)
        {
            // Might add more things when registering a level (like sound effects).
            levelsControllers.Add(zone);
            zone.zoneId = levelsControllers.Count - 1;
            path.AddZone(zone.northBound.position, zone.southBound.position);
            // RegisterRooms(level.roomsControllers);
            eventChannel.AddZoneController(zone);
        }

        private int GenRoomID()
        {
            var id = roomGenID;
            roomGenID += 1;
            return id;
        }

        private void ChangeZoneTypeForSelectedZone(ZoneTokenType zoneType)
        {
            levelsControllers[selectedRoomId].ChangeZoneVisuals(globalResources.GetZonesResources()[zoneType]);
            eventChannel.ChangeZoneType(selectedRoomId, zoneType);
            Debug.Log($"Changing zone type for {selectedRoomId} to {zoneType}");
        }

        public void TryBuyZoneForSelectedZone(ZoneTokenType zoneTokenType)
        {
            // if (!IsSelectedRoomValid()) return;
            var price = globalResources.GetZonesResources()[zoneTokenType].price.value;
            if (!economyController.CanSpend(Currency.Gold, price)) return;

            Debug.Log($"Buying zone [{zoneTokenType}] for {price} gold");

            economyController.SpendCurrency(Currency.Gold, price);
            ChangeZoneTypeForSelectedZone(zoneTokenType);
        }

        public void TryBuyLevel()
        {
            if (!economyController.CanSpend(Currency.Gold, currentPricePerLevel)) return;

            economyController.SpendCurrency(Currency.Gold, currentPricePerLevel);
            AddZoneToLayout();
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
                // roomRuneHandlers[roomType].AddRunesToStorageBufferForEntity(runeStorage, mobId);
                // roomRuneHandlers[roomType].ComputeDamageAndSlowForMobWithRuneStorage(runeStorage, mobId, out var damage, out var slow);
                // roomRuneHandlers[roomType].RemoveRunesFromStorageForEntity(runeStorage, mobId);

                roomsSystem.GetRunesHandler(roomId).UpdateDatabase(runeDatabase, roomId, mobId);

                var queryResult = new RunesHandlerForRoom.QueryResultRunProcessing();

                switch (roomType)
                {

                    case RoomType.Empty:
                        break;
                    case RoomType.SlowRune:
                        queryResult = roomsSystem
                            .GetRunesHandler(roomId)
                            .QueryRunProcessing(runeDatabase, roomId,
                                RunesProcessorFactory.CreateProcessor(ProcessorType.Slow));
                        break;
                    case RoomType.AttackRune:
                        break;
                    case RoomType.ConstantAttackFire:
                        queryResult = roomsSystem
                            .GetRunesHandler(roomId)
                            .QueryRunProcessing(runeDatabase, roomId,
                                RunesProcessorFactory.CreateProcessor(ProcessorType.Attack))
                            .AddDamage(5);

                        break;
                    case RoomType.BurstAttackFire:
                        queryResult = roomsSystem
                            .GetRunesHandler(roomId)
                            .QueryRunProcessing(runeDatabase, roomId,
                                RunesProcessorFactory.CreateProcessor(ProcessorType.Attack))
                            .MultiplyDamage(5);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                mobsSystem.UpdateDamageReceived(mobId, queryResult.damage);
                mobsSystem.SetSlow(mobId, queryResult.slow);

                roomsSystem.AddRoomToDisarm(roomId);

                // Debug.Log($"Mob {mobId} received {damage} damage and {slow} slow.");

                // Update Mob Controller.

                if (queryResult.damage > 0f)
                {
                    mob.UpdateHealthBar(mobsSystem.GetHealth(mobId) / 100f);
                }
            }

            mobsSystem.ApplyDamageReceivedToMobs();
            runeStorage.TransferBufferToStorage();
        }

        private bool HasMobPassedTheFinalRoom(int mobId)
        {
            return mobsSystem.GetLocationRoomIndex(mobId) == roomsSystem.FinalRoom;
        }

        public int SelectedRoomId
        {
            get => selectedRoomId;
            set => selectedRoomId = value;
        }

        public void ChangeSelectedZoneWithEvent(int zoneId)
        {
            // if (IsSelectedRoomValid())
            // {
            //     roomsControllers[SelectedRoomId].Deselect();
            // }
            //
            // SelectedRoomId = zoneId;
            // if (!IsSelectedRoomValid())
            // {
            //     return;
            // }
            //
            // roomsControllers[zoneId].Select();
            SelectedRoomId = zoneId;
            foreach (var levelsController in levelsControllers)
            {
                if (levelsController.zoneId == zoneId)
                {
                    levelsController.MarkAsSelected();
                }
                else
                {
                    levelsController.MarkAsUnselected();
                }
            }
            OnSelectedRoomChange?.Invoke(zoneId);
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

        private void UpdateMobsRooms()
        {
            foreach (var mob in mobsController.MobsList)
            {
                if (!mobsRoom.ContainsKey(mob.id))
                {
                    mobsRoom.Add(mob.id, path.GetZoneIndex(mob.position));
                }
                else
                {
                    mobsRoom[mob.id] = path.GetZoneIndex(mob.position);
                }
            }

            // Print mobs room using Debug.Log and linq only if we have at least one mob
            // if (mobsRoom.Count > 0)
            // {
            //     Debug.Log(string.Join(", ", mobsRoom.Select(x => x.Key + ":" + x.Value)));
            // }

        }

        #endregion

    }
}