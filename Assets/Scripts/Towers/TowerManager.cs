using System.Collections.Generic;
using System.Linq;
using Map;
using Mobs;
using Towers.Zones;
using UnityEngine;

namespace Towers
{
    public class TowerManager : MonoBehaviour
    {
        [SerializeField] private MapManager mapManager;

        [SerializeField] private EventChannel eventChannel;

        [Header("Settings")]
        [SerializeField] private Transform idleSpot;

        [SerializeField] private int projectilePoolSize;
        [SerializeField] private float projectileSpeed;
        [SerializeField] private float projectileExpireTime;
        [SerializeField] private float collisionCheckInterval;
        [SerializeField] private List<TowerController> towerControllers;
        [SerializeField] private List<ZoneTokenDataScriptableObject> zoneTokenDataScriptableObjects;

        [Header("Templates")]
        [SerializeField] private GameObject projectileTemplate;

        [Header("Internal")]
        [SerializeField] private float currentCollisionCheckTime;

        [Header("Systems")]
        [SerializeField] private ProjectilesSystem projectilesSystem;

        [SerializeField] private TowerSystem towerSystem;

        private Queue<(Projectile, List<Mob>)> attacksQueue;
        [SerializeField] private TokenZoneSystem tokenZoneSystem;

        private void Awake()
        {
            projectilesSystem = new ProjectilesSystem(projectilePoolSize, this)
            {
                idlePosition = idleSpot.position,
                projectileExpireTime = projectileExpireTime
            };

            towerSystem = new TowerSystem();

            foreach (var towerController in towerControllers)
            {
                towerSystem.AddTowerController(towerController);
            }

            attacksQueue = new Queue<(Projectile, List<Mob>)>();

            currentCollisionCheckTime = 0f;

            tokenZoneSystem = new TokenZoneSystem(zoneTokenDataScriptableObjects);

            eventChannel.OnZoneControllerAdded += RegisterZone;
            eventChannel.OnZoneTypeChanged += tokenZoneSystem.ChangeZoneType;
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            currentCollisionCheckTime += deltaTime;

            projectilesSystem.Update(deltaTime);
            towerSystem.Update(deltaTime);
        }

        private void LateUpdate()
        {
            // Check collisions
            if (currentCollisionCheckTime >= collisionCheckInterval)
            {
                currentCollisionCheckTime = 0f;
                projectilesSystem.CheckCollisions();

                foreach (var (projectileId, mobsHit) in projectilesSystem.MobsHitByProjectiles())
                {
                    if (mobsHit.Count == 0) continue;

                    var hasProjectile = projectilesSystem.TryGetProjectile(projectileId, out var projectile);

                    if (!hasProjectile) continue;

                    var mobs = mapManager.MobsController.Mobs;

                    var mobsList = (from mob in mobsHit where mobsHit.Contains(mob) select mobs[mob]).ToList();

                    attacksQueue.Enqueue((projectile, mobsList));

                    projectilesSystem.MarkToDestroy(projectileId);
                }
            }

            // Process attacks
            while (attacksQueue.Count > 0)
            {
                var (projectile, mobs) = attacksQueue.Dequeue();
                foreach (var mob in mobs)
                {
                    mob.Health -= 10f;
                    mapManager.MobsController.MarkToVisualUpdate(mob.id);
                }
            }

            // Remove destroyed projectiles
            projectilesSystem.RemoveProjectiles();


            foreach (var tower in towerSystem.Towers.Where(tower => tower.canFire && tower.readyToFire))
            {
                if (!projectilesSystem.HasFreeProjectile()) continue;

                // Find target
                var foundMob = mapManager.TryFindClosestMob(tower.position, out var target);

                if (!foundMob || target == null) continue;

                var startPosition = tower.position;

                tower.projectileBuilder
                    .WithTargetId(target.id)
                    .WithPosition(startPosition)
                    .WithSpeed(projectileSpeed)
                    .WithDestination(target.position);
                var projectile = projectilesSystem.TryAddProjectile(tower.projectileBuilder, out var projectileObj);
                if (projectile)
                {
                    tower.targetId = target.id;
                    tower.readyToFire = false;
                }
            }
        }

        public GameObject SpawnProjectileTemplate()
        {
            var obj = Instantiate(projectileTemplate, idleSpot.transform.position, Quaternion.identity);
            return obj;
        }

        public void RegisterZone(int zoneId, RoomController zone)
        {
            Debug.Log($"Registering zone {zoneId}");
            tokenZoneSystem.AddOrUpdateZone(zoneId, ZoneTokenType.None);
        }
    }

}