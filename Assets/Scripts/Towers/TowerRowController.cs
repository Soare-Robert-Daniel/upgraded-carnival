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
                ActivateNextTower();
            }
        }

        private void ActivateNextTower()
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
                attackTimeInterval = 1.5f,
                currentAttackTime = 0f,
                targetId = -1,
                position = towerController.transform.position,
                projectileBuilder = new ProjectileBuilder()
            };

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