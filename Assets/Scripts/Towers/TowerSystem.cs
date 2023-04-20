using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Towers
{
    [Serializable]
    public class TowerSystem
    {
        [SerializeField] private List<Tower> towers;
        private int nextTowerId;
        private Dictionary<int, TowerController> towersControllers;

        public TowerSystem()
        {
            towersControllers = new Dictionary<int, TowerController>();
            towers = new List<Tower>();
        }

        public List<Tower> Towers => towers;

        public void Update(float deltaTime)
        {
            foreach (var tower in towers.Where(tower => tower.canFire && !tower.readyToFire))
            {
                tower.currentAttackTime += deltaTime;
                if (tower.currentAttackTime >= tower.attackTimeInterval)
                {
                    tower.currentAttackTime = 0f;
                    tower.readyToFire = true;
                }
            }
        }

        public void AddTowerController(TowerController towerController)
        {
            var id = GenerateTowerId();
            var tower = new Tower
            {
                id = id,
                controllerId = id,
                canFire = true,
                attackTimeInterval = 1.5f,
                currentAttackTime = 0f,
                targetId = -1,
                position = towerController.transform.position,
                projectileBuilder = new ProjectileBuilder()
            };
            towersControllers.Add(id, towerController);
            towers.Add(tower);
        }

        private int GenerateTowerId()
        {
            return nextTowerId++;
        }
    }

    [Serializable]
    public class Tower
    {
        public int id;
        public int controllerId;
        public int targetId;
        public float attackTimeInterval;
        public float currentAttackTime;
        public bool canFire;
        public bool readyToFire;
        public Vector3 position;

        public ProjectileBuilder projectileBuilder;
    }
}