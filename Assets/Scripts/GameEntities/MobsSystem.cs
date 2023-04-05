using System;
using System.Collections.Generic;
using Economy;
using Map;
using UnityEngine;

namespace GameEntities
{
    /**
     * This class is responsible for keeping track of the mobs state in the game.
     * Animation, visual, and audio are not handled here. Check MobsControllerSystem for that.
     */
    [Serializable]
    public class MobsSystem
    {
        [Header("Settings")]
        [SerializeField] private float moveToNextRoomThreshold = 20;

        [Header("Internals")]
        [SerializeField] private int currentCapacity;

        [SerializeField] private int[] mobsRoomIndex;
        [SerializeField] private EntityRoomStatus[] mobsRoomStatus;

        [SerializeField] private EntityClass[] mobsClasses; // Pretty useless in current implementation.
        [SerializeField] private float[] mobsHealth;
        [SerializeField] private float[] mobsSlow;
        [SerializeField] private float[] mobsSpeed;
        [SerializeField] private float[] attackDamageReceived;
        [SerializeField] private Bounty[] mobsBounty;

        private Stack<int> mobsToDeploy;

        public MobsSystem(int initialCapacity = 0)
        {
            mobsRoomIndex = new int[initialCapacity];
            mobsClasses = new EntityClass[initialCapacity];
            mobsHealth = new float[initialCapacity];
            mobsSpeed = new float[initialCapacity];
            attackDamageReceived = new float[initialCapacity];
            mobsSlow = new float[initialCapacity];
            mobsRoomStatus = new EntityRoomStatus[initialCapacity];
            mobsBounty = new Bounty[initialCapacity];
            mobsToDeploy = new Stack<int>();
            currentCapacity = 0;
        }

        public void AddMob(int roomIndex, EntityClass entityClass, float health, float speed)
        {
            var newSlot = currentCapacity;
            if (currentCapacity >= mobsRoomIndex.Length)
            {
                ResizeStorage(newSlot + 1);
            }
            else
            {
                currentCapacity++;
            }

            mobsRoomIndex[newSlot] = roomIndex;
            mobsClasses[newSlot] = entityClass;
            mobsHealth[newSlot] = health;
            mobsSpeed[newSlot] = speed;
            mobsRoomStatus[newSlot] = EntityRoomStatus.ReadyToSpawn;
            attackDamageReceived[newSlot] = 0;
            mobsSlow[newSlot] = 1;
            AddMobToDeploy(newSlot);
        }

        public void UpdateDamageReceived(int mobIndex, float damage)
        {
            attackDamageReceived[mobIndex] += damage;
        }

        public void UpdateSpeed(int mobIndex, float speed)
        {
            mobsSpeed[mobIndex] = Math.Min(speed, 0.3f); // TODO: Replace magic number with a constant.
        }

        public void ApplyDamageReceivedToMobs()
        {
            for (var i = 0; i < currentCapacity; i++)
            {
                if (!(attackDamageReceived[i] > 0)) continue;

                mobsHealth[i] -= attackDamageReceived[i];
                attackDamageReceived[i] = 0;
            }
        }

        public void ResizeStorage(int newCapacity)
        {
            Array.Resize(ref mobsRoomIndex, newCapacity);
            Array.Resize(ref mobsClasses, newCapacity);
            Array.Resize(ref mobsHealth, newCapacity);
            Array.Resize(ref mobsSpeed, newCapacity);
            Array.Resize(ref attackDamageReceived, newCapacity);
            Array.Resize(ref mobsRoomStatus, newCapacity);
        }

        public void MoveRetiringMobsToDeploy()
        {
            for (var i = 0; i < currentCapacity; i++)
            {
                if (mobsRoomStatus[i] != EntityRoomStatus.Retiring) continue;

                // INFO: In the future, we might want to keep track of the retiring mobs for animation and have another system to keep track of the animations.
                mobsRoomStatus[i] = EntityRoomStatus.ReadyToSpawn;
                mobsSlow[i] = 1;
                AddMobToDeploy(i);
                // TODO: Move mob out of the visual area.
            }
        }

        public void AddMobToDeploy(int mobIndex)
        {
            mobsToDeploy.Push(mobIndex);
        }

        public IEnumerable<int> PullMobsToDeploy()
        {
            while (mobsToDeploy.Count > 0)
            {
                yield return mobsToDeploy.Pop();
            }
        }

        public void SetHealth(int mobIndex, float health)
        {
            mobsHealth[mobIndex] = health;
        }

        public void SetRoomStatus(int mobIndex, EntityRoomStatus roomStatus)
        {
            mobsRoomStatus[mobIndex] = roomStatus;
        }

        public void SetRoomIndex(int mobIndex, int roomIndex)
        {
            mobsRoomIndex[mobIndex] = roomIndex;
        }

        public void SetSpeed(int mobIndex, float speed)
        {
            mobsSpeed[mobIndex] = speed;
        }

        public void SetClass(int mobIndex, EntityClass entityClass)
        {
            mobsClasses[mobIndex] = entityClass;
        }

        public int GetLocationRoomIndex(int mobIndex)
        {
            return mobsRoomIndex[mobIndex];
        }

        public EntityRoomStatus GetRoomStatusFor(int mobIndex)
        {
            return mobsRoomStatus[mobIndex];
        }

        public float GetHealth(int mobIndex)
        {
            return mobsHealth[mobIndex];
        }

        public float GetSpeed(int mobIndex)
        {
            return mobsSpeed[mobIndex];
        }

        public int GetMobCount()
        {
            return currentCapacity;
        }

        public int ReadyToDeployMobCount()
        {
            return mobsToDeploy.Count;
        }

        public bool IsDead(int mobIndex)
        {
            return mobsHealth[mobIndex] <= 0;
        }

        public float GetDamageReceived(int mobIndex)
        {
            return attackDamageReceived[mobIndex];
        }

        public int[] GetMobsToDeployArray()
        {
            return mobsToDeploy.ToArray();
        }

        public float[] GetMobsHealthArray()
        {
            return mobsHealth;
        }

        public float[] GetMobsSpeedArray()
        {
            return mobsSpeed;
        }

        public float[] GetMobsSlowArray()
        {
            return mobsSlow;
        }

        public int[] GetMobsRoomIndexArray()
        {
            return mobsRoomIndex;
        }

        public EntityRoomStatus[] GetMobsRoomStatusArray()
        {
            return mobsRoomStatus;
        }

        public EntityClass[] GetMobsClassArray()
        {
            return mobsClasses;
        }

        public float[] GetMobsDamageReceivedArray()
        {
            return attackDamageReceived;
        }

        public void SetSlow(int mobIndex, float slow)
        {
            mobsSlow[mobIndex] = slow;
        }

        public void ResetSlowFor(int mobIndex)
        {
            mobsSlow[mobIndex] = 0;
        }

        public void SetBounty(int mobIndex, Bounty bounty)
        {
            mobsBounty[mobIndex] = bounty;
        }

        public Bounty GetBounty(int mobIndex)
        {
            return mobsBounty[mobIndex];
        }

        public Bounty[] GetMobsBountyArray()
        {
            return mobsBounty;
        }
    }
}