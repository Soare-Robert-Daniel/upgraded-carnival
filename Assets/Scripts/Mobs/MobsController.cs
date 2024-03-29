﻿using System.Collections.Generic;
using UnityEngine;

namespace Mobs
{
    public class MobsController : MonoBehaviour
    {
        [SerializeField] private EventChannel eventChannel;
        [SerializeField] private List<Mob> mobsList;

        [SerializeField] private Vector3 targetPosition;
        [SerializeField] private Vector3 idlePosition;
        [SerializeField] private bool run;
        public int controllersIdCounter;
        private Stack<int> freeControllersIds;

        private Dictionary<int, MobSimpleController> mobControllers;
        private Dictionary<int, Mob> mobs;
        private Dictionary<int, MobDataScriptableObject> mobsData;
        private Dictionary<int, int> mobsZoneIds;

        private Stack<int> updateVisualsIds;

        public int FreeControllersCount => freeControllersIds.Count;

        public List<Mob> MobsList => mobsList;
        public Dictionary<int, Mob> Mobs => mobs;
        public Dictionary<int, MobDataScriptableObject> MobsData => mobsData;

        public Dictionary<int, int> MobsZoneIds => mobsZoneIds;

        private void Awake()
        {
            mobControllers = new Dictionary<int, MobSimpleController>();
            mobsZoneIds = new Dictionary<int, int>();
            freeControllersIds = new Stack<int>();
            mobs = new Dictionary<int, Mob>();
            updateVisualsIds = new Stack<int>();
            controllersIdCounter = 0;
            mobsData = new Dictionary<int, MobDataScriptableObject>();

            eventChannel.OnMobReachedEnd += RemoveMobById;
        }

        public void Update()
        {
            if (!run) return;

            DetectAndRemoveDeadMobs();

            foreach (var mob in mobsList)
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

        public void SpawnMob(Mob mob, MobDataScriptableObject mobDataScriptableObject)
        {
            var controllerId = freeControllersIds.Pop();

            // Get controller
            if (!mobControllers.TryGetValue(controllerId, out var controller)) return;

            controller.SetMobId(mob.id);
            controller.UpdateHealthBar(1);
            controller.UpdateVisuals(mobDataScriptableObject.mobResourcesScriptableObject);
            mob.controllerId = controllerId;
            mobsList.Add(mob);
            mobs.Add(mob.id, mob);
            mobsData.Add(mob.id, mobDataScriptableObject);
            Debug.Log($"[MOB][SPAWN] Spawned mob {mob.id} with controller {mob.controllerId}");
        }

        public void MarkToVisualUpdate(int mobId)
        {
            updateVisualsIds.Push(mobId);
        }

        public void UpdateVisuals()
        {
            while (updateVisualsIds.Count > 0)
            {
                var mobId = updateVisualsIds.Pop();
                if (mobs.TryGetValue(mobId, out var mob))
                {
                    if (mobControllers.TryGetValue(mob.controllerId, out var controller))
                    {
                        controller.UpdateHealthBar(mob.HealthPercent);
                    }
                }
            }
        }

        public void DetectAndRemoveDeadMobs()
        {
            for (var i = mobsList.Count - 1; i >= 0; i--)
            {
                var mob = mobsList[i];
                if (mob.Health <= 0)
                {
                    RemoveMob(mob);
                }
            }
        }

        public void RemoveMobById(int mobId)
        {
            if (mobs.TryGetValue(mobId, out var mob))
            {
                RemoveMob(mob);
            }
        }

        public void RemoveMob(Mob mob)
        {
            mobsList.Remove(mob);
            freeControllersIds.Push(mob.controllerId);
            mobControllers[mob.controllerId].transform.position = idlePosition;
            mobs.Remove(mob.id);
            eventChannel.EliminateMob(mob.id);
            mobsData.Remove(mob.id);
        }

        public Vector3 GetMobControllerPosition(int controllerId)
        {
            return mobControllers[controllerId].transform.position;
        }
    }
}