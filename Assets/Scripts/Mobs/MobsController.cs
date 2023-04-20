using System.Collections.Generic;
using UnityEngine;

namespace Mobs
{
    public class MobsController : MonoBehaviour
    {
        [SerializeField] private List<Mob> mobs;

        [SerializeField] private Vector3 targetPosition;
        [SerializeField] private Vector3 idlePosition;
        [SerializeField] private bool run;
        public int controllersIdCounter;
        private Stack<int> freeControllersIds;

        private Dictionary<int, MobSimpleController> mobControllers;

        public int FreeControllersCount => freeControllersIds.Count;

        public List<Mob> Mobs => mobs;

        private void Awake()
        {
            mobControllers = new Dictionary<int, MobSimpleController>();
            freeControllersIds = new Stack<int>();
            controllersIdCounter = 0;
        }

        public void Update()
        {
            if (!run) return;

            foreach (var mob in mobs)
            {
                mob.position -= Vector3.up * (mob.Speed * Time.deltaTime);

                // Update controller position and check if it exists in dictionary
                if (mobControllers.TryGetValue(mob.controllerId, out var controller))
                {
                    controller.transform.position = mob.position;
                }

            }

        }

        public void AddMobController(MobSimpleController mobController)
        {
            // Debug.Log("AddMobController");
            mobControllers.Add(controllersIdCounter, mobController);
            freeControllersIds.Push(controllersIdCounter);
            controllersIdCounter++;
        }

        public bool CanSpawnMob()
        {
            return freeControllersIds.Count > 0;
        }

        public void SpawnMob(Mob mob)
        {
            var controllerId = freeControllersIds.Pop();
            mob.controllerId = controllerId;
            mobs.Add(mob);
        }

        public void RemoveMob(Mob mob)
        {
            mobs.Remove(mob);
            freeControllersIds.Push(mob.controllerId);
            mobControllers[mob.controllerId].transform.position = idlePosition;
        }

        public Vector3 GetMobControllerPosition(int controllerId)
        {
            return mobControllers[controllerId].transform.position;
        }
    }
}