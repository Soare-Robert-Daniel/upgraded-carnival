using System.Collections.Generic;
using System.Linq;
using Map;
using UnityEngine;

namespace Towers
{
    public class TowerManager : MonoBehaviour
    {
        [SerializeField] private MapManager mapManager;
        [SerializeField] private ProjectilesSystem projectilesSystem;
        [SerializeField] private TowerSystem towerSystem;

        [Header("Settings")]
        [SerializeField] private Transform idleSpot;

        [SerializeField] private int projectilePoolSize;
        [SerializeField] private float projectileSpeed;
        [SerializeField] private float projectileExpireTime;
        [SerializeField] private float collisionCheckInterval;
        [SerializeField] private List<TowerController> towerControllers;

        [Header("Templates")]
        [SerializeField] private GameObject projectileTemplate;

        [Header("Internal")]
        [SerializeField] private float currentCollisionCheckTime;

        private void Start()
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

            currentCollisionCheckTime = 0f;
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

            if (currentCollisionCheckTime >= collisionCheckInterval)
            {
                currentCollisionCheckTime = 0f;
                projectilesSystem.CheckCollisions();

                foreach (var (projectileId, mobsHit) in projectilesSystem.MobsHitByProjectiles())
                {
                    if (mobsHit.Count == 0) continue;


                    projectilesSystem.MarkToDestroy(projectileId);
                }
            }

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
                    .WithDestination(mapManager.MobsController.GetMobControllerPosition(target.id));
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
    }

}