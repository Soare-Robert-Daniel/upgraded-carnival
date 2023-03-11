using System;
using System.Collections.Generic;
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
        [SerializeField] private int currentCapacity;
        [SerializeField] private int[] mobsRoomIndex;
        [SerializeField] private EntityRoomStatus[] mobsRoomStatus;

        [SerializeField] private EntityClass[] mobsClasses;
        [SerializeField] private float[] mobsHealth;
        [SerializeField] private float[] mobsSpeed;

        [SerializeField] private float[] attackDamageReceived;

        private Stack<int> mobsToDeploy;

        public MobsSystem(int initialCapacity = 0)
        {
            mobsRoomIndex = new int[initialCapacity];
            mobsClasses = new EntityClass[initialCapacity];
            mobsHealth = new float[initialCapacity];
            mobsSpeed = new float[initialCapacity];
            attackDamageReceived = new float[initialCapacity];
            mobsRoomStatus = new EntityRoomStatus[initialCapacity];
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
            AddMobToDeploy(newSlot);
        }

        public void UpdateMobDamageReceived(int mobIndex, float damage)
        {
            attackDamageReceived[mobIndex] += damage;
        }

        public void UpdateMobSpeed(int mobIndex, float speed)
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
                AddMobToDeploy(i);
            }
        }

        public void MoveMobsToNextRooms(Vector3[] mobsCurrentPosition, Vector3[] roomsExitPositions)
        {
            for (var i = 0; i < currentCapacity; i++)
            {
                if (mobsRoomStatus[i] != EntityRoomStatus.Moving ||
                    !CanMoveMobToNextRoom(mobsCurrentPosition[i], roomsExitPositions[mobsRoomIndex[i]])) continue;
                mobsRoomStatus[i] = EntityRoomStatus.Entered;
                mobsRoomIndex[i] = Math.Clamp(mobsRoomIndex[i] + 1, 0, roomsExitPositions.Length - 1);
            }
        }

        public bool CanMoveMobToNextRoom(Vector3 currentPosition, Vector3 currentRoomExitPosition)
        {
            return Vector3.SqrMagnitude(currentPosition - currentRoomExitPosition) < 0.1f;
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

        public void SetMobHealth(int mobIndex, float health)
        {
            mobsHealth[mobIndex] = health;
        }

        public void SetMobRoomStatus(int mobIndex, EntityRoomStatus roomStatus)
        {
            mobsRoomStatus[mobIndex] = roomStatus;
        }

        public void SetMobRoomIndex(int mobIndex, int roomIndex)
        {
            mobsRoomIndex[mobIndex] = roomIndex;
        }

        public void SetMobSpeed(int mobIndex, float speed)
        {
            mobsSpeed[mobIndex] = speed;
        }

        public void SetMobClass(int mobIndex, EntityClass entityClass)
        {
            mobsClasses[mobIndex] = entityClass;
        }

        public int GetMobLocationRoomIndex(int mobIndex)
        {
            return mobsRoomIndex[mobIndex];
        }

        public EntityRoomStatus GetMobRoomStatus(int mobIndex)
        {
            return mobsRoomStatus[mobIndex];
        }

        public float GetMobHealth(int mobIndex)
        {
            return mobsHealth[mobIndex];
        }

        public float GetMobSpeed(int mobIndex)
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

        public bool IsMobDead(int mobIndex)
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
    }
}