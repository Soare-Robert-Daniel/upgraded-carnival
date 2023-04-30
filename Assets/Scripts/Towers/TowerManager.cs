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
        [SerializeField] private GlobalResources globalResources;


        [Header("Settings")]
        [SerializeField] private Transform idleSpot;

        [SerializeField] private int projectilePoolSize;
        [SerializeField] private float projectileSpeed;
        [SerializeField] private float projectileExpireTime;
        [SerializeField] private float collisionCheckInterval;
        [SerializeField] private float zoneAttackInterval;
        [SerializeField] private float tokenDurationUpdateInterval;

        [Header("Templates")]
        [SerializeField] private GameObject projectileTemplate;

        [Header("Internal")]
        [SerializeField] private float currentCollisionCheckTime;

        [SerializeField] private float currentZoneAttackTime;
        [SerializeField] private float currentTokenDurationUpdateTime;

        [Header("Systems")]
        [SerializeField] private ProjectilesSystem projectilesSystem;

        [SerializeField] private TowerSystem towerSystem;

        private Queue<(Projectile, List<Mob>)> attacksQueue;
        private TokenZoneSystem tokenZoneSystem;

        public TowerSystem TowerSystem => towerSystem;
        public TokenZoneSystem TokenZoneSystem => tokenZoneSystem;

        private void Awake()
        {
            projectilesSystem = new ProjectilesSystem(projectilePoolSize, this)
            {
                idlePosition = idleSpot.position,
                projectileExpireTime = projectileExpireTime
            };

            towerSystem = new TowerSystem();

            attacksQueue = new Queue<(Projectile, List<Mob>)>();

            currentCollisionCheckTime = 0f;
            currentZoneAttackTime = 0f;

            tokenZoneSystem = new TokenZoneSystem(globalResources.zoneTokenDataScriptableObjects);

            eventChannel.OnZoneControllerAdded += RegisterZone;
            eventChannel.OnZoneTypeChanged += tokenZoneSystem.ChangeZoneType;
            eventChannel.OnMobEliminated += tokenZoneSystem.RemoveMobTokens;
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;

            // Update timers
            currentCollisionCheckTime += deltaTime;
            currentZoneAttackTime += deltaTime;
            currentTokenDurationUpdateTime += deltaTime;

            // Update token durations
            if (currentTokenDurationUpdateTime >= tokenDurationUpdateInterval)
            {
                tokenZoneSystem.UpdateTokenDurations(currentTokenDurationUpdateTime);
                currentTokenDurationUpdateTime = 0f;
            }

            projectilesSystem.Update(deltaTime);
            towerSystem.Update(deltaTime);
        }

        private void LateUpdate()
        {
            // Check zone collisions and update tokens
            if (currentZoneAttackTime >= zoneAttackInterval)
            {
                var tokenDataSO = globalResources.GetZonesResources();
                var mobs = mapManager.MobsController.Mobs;

                currentZoneAttackTime = 0f;
                foreach (var zoneController in mapManager.LevelsControllers)
                {
                    zoneController.CheckColliders();
                    foreach (var mobId in zoneController.mobsInZone)
                    {
                        tokenZoneSystem.AddOrUpdateTokens(mobId, zoneController.zoneId);

                        // Apply timer effects
                        var hasTokens = tokenZoneSystem.TryGetMobTokens(mobId, out var tokens);
                        if (hasTokens)
                        {
                            var damage = 0f;
                            var slow = 0f;

                            foreach (var token in tokens)
                            {
                                var zoneTokenDataScriptableObject = tokenDataSO[token.zoneTokenType];
                                if (zoneTokenDataScriptableObject.trigger == Trigger.TimerTick)
                                {
                                    if (zoneTokenDataScriptableObject.TryGetRankData(token.rank, out var rankData))
                                    {
                                        damage += rankData.damage;
                                        slow += rankData.slow;
                                    }
                                }
                            }


                            if (!mobs.TryGetValue(mobId, out var mob)) continue;

                            // TODO: Investigate why this is not working correctly. There seams to be a delay.
                            // Also slow is 1 for testing purpose.
                            mob.Health -= damage;
                            mob.Slow = slow;
                            mob.UpdateSpeed();

                            Debug.Log($"[ZONE][TIME TICK] Mob <{mobId}> took {damage} damage and slowed by {slow}");
                        }
                    }
                }


            }

            // Check projectile collisions and collect attacks
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

            if (attacksQueue.Count > 0)
            {
                var tokenDataSO = globalResources.GetZonesResources();
                // Process attacks
                while (attacksQueue.Count > 0)
                {
                    var (projectile, mobs) = attacksQueue.Dequeue();
                    foreach (var mob in mobs)
                    {
                        var damage = 10f;

                        var hasTokens = tokenZoneSystem.TryGetMobTokens(mob.id, out var tokens);

                        if (hasTokens)
                        {
                            foreach (var zoneToken in tokens)
                            {
                                if (
                                    tokenDataSO.TryGetValue(zoneToken.zoneTokenType, out var tokenData) &&
                                    tokenData.trigger == Trigger.TowerHit
                                )
                                {
                                    if (tokenData.TryGetRankData(zoneToken.rank, out var rankData))
                                    {
                                        damage += rankData.damage;
                                    }
                                }
                            }
                        }

                        mob.Health -= damage;
                        Debug.Log($"[PROJECTILE][MOB HIT] Mob <{mob.id}> took {damage} damage");
                        mapManager.MobsController.MarkToVisualUpdate(mob.id);
                    }
                }
            }


            // Remove destroyed projectiles
            projectilesSystem.RemoveProjectiles();


            // Create new projectiles
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

        public void RegisterZone(ZoneController zone)
        {
            Debug.Log($"Registering zone [{zone.zoneId}]");
            tokenZoneSystem.AddOrUpdateZone(zone.zoneId, ZoneTokenType.None);
        }
    }

}