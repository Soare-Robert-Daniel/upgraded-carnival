using System.Collections.Generic;
using UnityEngine;

namespace Towers
{
    public class TowerRowController : MonoBehaviour
    {
        [SerializeField] private int initialActiveTowers;
        [SerializeField] private TowerManager towerManager;
        [SerializeField] private int currentActiveTowers;
        [SerializeField] private List<TowerController> towerControllers;
        [SerializeField] private TowerDataScriptableObject defaultTowerData;


        private Stack<int> activeTowers;
        private Stack<int> inactiveTowers;

        private void Awake()
        {
            activeTowers = new Stack<int>();
            inactiveTowers = new Stack<int>();

            for (var i = towerControllers.Count; i >= 0; i--)
            {
                inactiveTowers.Push(i);
            }

        }

        private void Start()
        {
            for (var i = 0; i < initialActiveTowers; i++)
            {
                CreateTower(defaultTowerData);
            }
        }

        public void CreateTower(TowerDataScriptableObject towerData)
        {
            if (inactiveTowers.Count == 0) return;

            var towerId = inactiveTowers.Pop();
            activeTowers.Push(towerId);

            var towerController = towerControllers[towerId];

            var tower = new Tower
            {
                id = towerId,
                controllerId = towerId,
                canFire = true,
                attackTimeInterval = towerData.attackInterval,
                currentAttackTime = 0f,
                targetId = -1,
                position = towerController.transform.position,
                projectileBuilder = new ProjectileBuilder()
            };

            tower.projectileBuilder
                .WithSpeed(towerData.projectileData.baseStats.speed)
                .WithDamage(towerData.projectileData.baseStats.damage);

            towerManager.TowerSystem.AddTower(tower);

            currentActiveTowers++;
        }

        private void DeactivateLastTower()
        {
            if (activeTowers.Count == 0) return;

            var towerId = activeTowers.Pop();
            inactiveTowers.Push(towerId);

            towerManager.TowerSystem.RemoveTower(towerId);

            currentActiveTowers--;
        }
    }
}