using System;
using System.Collections.Generic;
using UnityEngine;

namespace Towers
{
    [Serializable]
    public class ProjectilesSystem
    {

        public const int NoProjectileFound = -1;
        [SerializeField] private int projectileId;

        public Vector3 idlePosition;
        public float projectileExpireTime;

        [SerializeField] private List<ProjectileController> projectilesControllers;
        [SerializeField] private List<int> activeProjectiles;

        private Dictionary<int, float> createdAt;

        private Stack<int> freeProjectiles;

        private Dictionary<int, Projectile> projectiles;

        private Dictionary<int, List<int>> projectilesToMobsHit;
        private Stack<int> projectileToDestroy;

        public ProjectilesSystem(int initialCapacity = 0, TowerManager towerManager = null)
        {
            projectileId = 0;
            projectilesControllers = new List<ProjectileController>();
            freeProjectiles = new Stack<int>();
            projectiles = new Dictionary<int, Projectile>();
            activeProjectiles = new List<int>();
            createdAt = new Dictionary<int, float>();
            projectileToDestroy = new Stack<int>();
            projectilesToMobsHit = new Dictionary<int, List<int>>();

            if (towerManager == null) return;
            for (var i = 0; i < initialCapacity; i++)
            {
                var obj = towerManager.SpawnProjectileTemplate();
                AddProjectileController(obj.GetComponent<ProjectileController>());
            }
        }

        public void Update(float deltaTime)
        {
            var currentTime = Time.time;
            foreach (var activeProjectile in activeProjectiles)
            {
                projectiles[activeProjectile].Move(deltaTime);
                var controller = projectilesControllers[projectiles[activeProjectile].controllerId];
                controller.transform.position = projectiles[activeProjectile].position;
                // Debug.Log(projectiles[activeProjectile].position);
                if (currentTime - createdAt[activeProjectile] > projectileExpireTime)
                {
                    projectileToDestroy.Push(activeProjectile);
                }
            }
        }

        public void CheckCollisions()
        {
            foreach (var activeProjectile in activeProjectiles)
            {
                var mobsHit = projectilesControllers[projectiles[activeProjectile].controllerId].MobsHit();

                if (mobsHit.Count == 0) continue;

                if (!projectilesToMobsHit.ContainsKey(activeProjectile))
                {
                    projectilesToMobsHit.Add(activeProjectile, mobsHit);
                }
                else
                {
                    projectilesToMobsHit[activeProjectile].AddRange(mobsHit);
                }

                // Debug.Log(string.Join(", ", mobsHit.Select(x => x.ToString()).ToArray()));
            }
        }

        public void AddProjectileController(ProjectileController projectile)
        {
            projectilesControllers.Add(projectile);
            freeProjectiles.Push(projectilesControllers.Count - 1);
        }

        public bool TryAddProjectile(ProjectileBuilder builder, out Projectile projectile)
        {
            var controllerId = PullFreeProjectile();
            if (controllerId == NoProjectileFound)
            {
                projectile = new Projectile();
                return false;
            }

            projectile = builder
                .WithId(GenerateProjectileId())
                .WithControllerId(controllerId)
                .Build();
            projectiles.Add(controllerId, projectile);
            activeProjectiles.Add(controllerId);
            createdAt.Add(controllerId, Time.time + projectileExpireTime);
            return true;
        }

        public int PullFreeProjectile()
        {
            return freeProjectiles.Count == 0 ? NoProjectileFound : freeProjectiles.Pop();
        }

        public int GenerateProjectileId()
        {
            var id = projectileId;
            projectileId = (projectileId + 1) % int.MaxValue;
            return id;
        }

        public void RemoveProjectile(int id)
        {
            if (!projectiles.ContainsKey(id)) return;

            activeProjectiles.Remove(id);
            var controllerId = projectiles[id].controllerId;
            projectiles.Remove(id);
            freeProjectiles.Push(controllerId);

            projectilesControllers[controllerId].transform.position = idlePosition;
            createdAt.Remove(id);
            projectilesToMobsHit.Remove(id);
        }

        public void RemoveProjectiles()
        {
            while (projectileToDestroy.Count > 0)
            {
                RemoveProjectile(projectileToDestroy.Pop());
            }
        }

        public bool HasFreeProjectile()
        {
            return freeProjectiles.Count > 0;
        }

        public void MarkToDestroy(int id)
        {
            projectileToDestroy.Push(id);
        }

        public Dictionary<int, List<int>> MobsHitByProjectiles()
        {
            return projectilesToMobsHit;
        }
    }

    [Serializable]
    public class Projectile
    {
        public int id;
        public int controllerId;
        public int targetId;
        public Vector3 destination;
        public Vector3 position;
        public float speed;
        public Vector3 direction;

        public bool Equals(Projectile other)
        {
            return id == other.id && controllerId == other.controllerId;
        }

        public void ChangeSpeed(float newSpeed)
        {
            speed = newSpeed;
        }

        public void ChangePosition(Vector3 newPosition)
        {
            position = newPosition;
        }

        public void ChangeDestination(Vector3 newDestination)
        {
            destination = newDestination;
        }

        public void Move(float deltaTime)
        {
            position += direction * (speed * deltaTime);
        }

        public Projectile Clone()
        {
            return new Projectile
            {
                id = id,
                controllerId = controllerId,
                targetId = targetId,
                destination = destination,
                position = position,
                speed = speed,
                direction = direction
            };
        }
    }

    public class ProjectileBuilder
    {
        private Projectile projectile;

        public ProjectileBuilder()
        {
            projectile = new Projectile();
        }

        public ProjectileBuilder WithId(int id)
        {
            projectile.id = id;
            return this;
        }

        public ProjectileBuilder WithControllerId(int controllerId)
        {
            projectile.controllerId = controllerId;
            return this;
        }

        public ProjectileBuilder WithTargetId(int targetId)
        {
            projectile.targetId = targetId;
            return this;
        }

        public ProjectileBuilder WithPosition(Vector3 position)
        {
            projectile.position = position;
            return this;
        }

        public ProjectileBuilder WithSpeed(float speed)
        {
            projectile.speed = speed;
            return this;
        }

        public ProjectileBuilder WithDestination(Vector3 destination)
        {
            projectile.destination = destination;
            return this;
        }

        public ProjectileBuilder WithDirection(Vector3 direction)
        {
            projectile.direction = direction;
            return this;
        }

        public Projectile Build()
        {
            projectile.direction = (projectile.destination - projectile.position).normalized;
            projectile.direction.z = 0;
            projectile.position.z = -5;
            return projectile.Clone();
        }
    }


}