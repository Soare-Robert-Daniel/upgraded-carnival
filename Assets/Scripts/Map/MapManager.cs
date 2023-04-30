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

        public const int NotZoneFound = -1;
        public const int NoMobFound = -1;

        [Header("Wave")]
        [SerializeField] private bool enableWave;

        [SerializeField] private int waveNumber;
        [SerializeField] private float waveTimeInterval;
        [SerializeField] private float waveMobSpawnStartOffset;
        [SerializeField] private float spawnSpreadDistance;

        [Header("Levels")]
        [SerializeField] private int levelsNum;

        [SerializeField] private float currentPricePerLevel;
        [SerializeField] private float pricePerLevelMultiplier;
        [SerializeField] private Vector3 offset;
        [SerializeField] private List<ZoneController> levelsControllers;

        [Header("Mobs")]
        [SerializeField] private int mobPoolSize;

        [SerializeField] private MobsController mobsController;

        [Header("Resources")]
        public GlobalResources globalResources;

        [SerializeField] private int startingLevelsNum;
        [SerializeField] private GameObject mobTemplate;
        [SerializeField] private GameObject levelTemplate;
        [SerializeField] private Transform mapRootPoint;
        [SerializeField] private Transform mobStartingPoint;
        [SerializeField] private EconomyController economyController;

        [Header("Models")]
        [SerializeField] private WaveMobNumberPerWaveModel waveMobNumberPerWaveModel;

        [Header("Internals")]
        [SerializeField] private int selectedZoneId;

        [SerializeField] private int selectedMobId;

        [SerializeField] private float currentWaveTimer;

        [SerializeField] private float currentSpawnTimer;

        [SerializeField] private Wave waveCreator;
        [SerializeField] private Path path;
        [SerializeField] private EventChannel eventChannel;

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

            currentWaveTimer = 0;
            currentSpawnTimer = 0;

            path = new Path();
            path.AddPoint(mobStartingPoint.position);
            path.AddPoint(mapRootPoint.position);

            waveCreator = new Wave(waveMobNumberPerWaveModel);

            stopwatch = new Stopwatch();

            eventChannel.OnZoneTypeChanged += (i, type) =>
            {
                Debug.Log($"[MAP][OBSERVER] Zone type changed: {i} {type}");
            };

            eventChannel.OnZoneControllerAdded += (zoneController) =>
            {
                Debug.Log($"[MAP][OBSERVER] Zone controller added: {zoneController.zoneId}");
            };

            eventChannel.OnSelectedZoneChanged += ChangeSelectedZoneWithEvent;
        }

        private void Start()
        {
            CreateLayout(startingLevelsNum);
            AddMobsToPool(mobPoolSize);
            OnInitUI?.Invoke();
        }

        public void Update()
        {
            MobsController.UpdateVisuals();

            if (enableWave)
            {
                currentWaveTimer += Time.deltaTime;
                currentSpawnTimer += Time.deltaTime;
                OnNextWaveTimeChanged?.Invoke(waveTimeInterval - currentWaveTimer);
            }

            stopwatch.Reset();
            stopwatch.Start();
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


            if (currentWaveTimer > waveTimeInterval)
            {
                StartWave();
            }

            stopwatch.Stop();
            OnMapLogicTimeChanged?.Invoke(stopwatch.ElapsedMilliseconds);
        }


        private bool TrySpawnMob()
        {
            // Debug.Log($"TrySpawnMob {waveCreator.HasMobsToSpawn()} {mobsController.CanSpawnMob()}");
            if (!waveCreator.HasMobsToSpawn() || !mobsController.CanSpawnMob())
                return false;

            var (mob, mobDataScriptableObject) = waveCreator.DequeueMobsToSpawn().First();

            mob.position = mobStartingPoint.position +
                           Vector3.left * (spawnSpreadDistance *
                                           Mathf.Clamp01(UnityEngine.Random.Range(0f, 1f) * UnityEngine.Random.Range(0, 5)));

            mobsController.SpawnMob(mob, mobDataScriptableObject);

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

        #region Mob Gameplay

        private int GenMobID()
        {
            var id = mobGenID;
            mobGenID += 1;
            return id;
        }

        #endregion

        public void MobReachedEnd(int mobMobId)
        {
            eventChannel.MobReachedEnd(mobMobId);
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
            Debug.Log($"[MAP][DEPLOY] Mobs to deploy: {mobsToDeploy}, Mobs in pool: {mobsInPool}, Mobs to add: {mobsToAdd}");
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

        private void CreateMobAndAddToPool()
        {
            Debug.Log("Creating mob and adding to pool");
            // By letting the room to register himself in Start, it will be available for next round.
            var mobObj = Instantiate(mobTemplate, mobStartingPoint.transform.position, Quaternion.identity);

            var simpleMobController = mobObj.GetComponent<MobSimpleController>();

            if (simpleMobController == null)
            {
                Debug.LogError("[MAP][ERROR] The Mob template DOES NOT HAVE THE CONTROLLER");
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

        public event Action<int> OnZoneControllerNumberChanged;

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
            OnZoneControllerNumberChanged?.Invoke(levelsControllers.Count);
            zone.zoneId = levelsControllers.Count - 1;
            path.AddZone(zone.northBound.position, zone.southBound.position);
            // RegisterRooms(level.roomsControllers);
            eventChannel.AddZoneController(zone);
        }

        private void ChangeZoneTypeForSelectedZone(ZoneTokenType zoneType)
        {
            var zoneResources = globalResources.GetZonesResources()[zoneType];
            levelsControllers[selectedZoneId].ChangeZoneVisuals(zoneResources);
            eventChannel.ChangeZoneType(selectedZoneId, zoneType);
            Debug.Log($"[MAP][ZONE] Changing zone type for {selectedZoneId} to {zoneType}");
        }

        public void TryBuyZoneForSelectedZone(ZoneTokenType zoneTokenType)
        {
            if (!IsSelectedZoneValid()) return;
            var price = globalResources.GetZonesResources()[zoneTokenType].price.value;
            if (!economyController.CanSpend(Currency.Gold, price)) return;

            Debug.Log($"[MAP][ZONE] Buying zone [{zoneTokenType}] for {price} gold");

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

        public int SelectedRoomId
        {
            get => selectedZoneId;
            set => selectedZoneId = value;
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

        private bool IsSelectedZoneValid()
        {
            return selectedZoneId >= 0 && selectedZoneId < levelsControllers.Count;
        }

        #endregion

    }
}