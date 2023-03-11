using System;
using Map.Room;
using UnityEngine;

namespace Map
{
    [Serializable]
    public class RoomsSystem
    {
        [SerializeField] private int currentCapacity;
        [SerializeField] private bool[] roomsCanFire;
        [SerializeField] private float[] roomsCurrentAttackTime;
        [SerializeField] private float[] roomsAttackTimeInterval;
        [SerializeField] private RoomType[] roomsType;
        [SerializeField] private Vector3[] startRoomPositions;
        [SerializeField] private Vector3[] exitRoomPositions;
        private bool[] roomsToDisarm;

        public RoomsSystem(int initialCapacity = 0)
        {
            roomsCanFire = new bool[initialCapacity];
            roomsCurrentAttackTime = new float[initialCapacity];
            roomsAttackTimeInterval = new float[initialCapacity];
            roomsType = new RoomType[initialCapacity];
            roomsToDisarm = new bool[initialCapacity];
            exitRoomPositions = new Vector3[initialCapacity];
            startRoomPositions = new Vector3[initialCapacity];
            currentCapacity = 0;
        }

        public int CurrentCapacity => currentCapacity;

        public void AddRoom(RoomType roomType, float attackTimeInterval, Vector3 startRoomPosition, Vector3 exitRoomPosition)
        {
            var newSlot = currentCapacity;
            if (currentCapacity >= roomsCanFire.Length)
            {
                ResizeStorage(newSlot + 1);
            }
            else
            {
                currentCapacity++;
            }

            roomsCanFire[newSlot] = true;
            roomsCurrentAttackTime[newSlot] = 0;
            roomsAttackTimeInterval[newSlot] = attackTimeInterval;
            roomsType[newSlot] = roomType;

            startRoomPositions[newSlot] = startRoomPosition;
            exitRoomPositions[newSlot] = exitRoomPosition;
        }

        public void UpdateRoomsAttackTime(float deltaTime)
        {
            for (var i = 0; i < currentCapacity; i++)
            {
                if (roomsType[i] == RoomType.Empty || roomsCanFire[i]) continue;

                roomsCurrentAttackTime[i] += deltaTime;
                if (roomsCurrentAttackTime[i] >= roomsAttackTimeInterval[i])
                {
                    roomsCurrentAttackTime[i] = 0;
                    roomsCanFire[i] = true;
                }
            }
        }

        public void SetRoomType(int roomIndex, RoomType roomType)
        {
            roomsType[roomIndex] = roomType;
        }

        public void SetRoomAttackTimeInterval(int roomIndex, float attackTimeInterval)
        {
            roomsAttackTimeInterval[roomIndex] = attackTimeInterval;
        }

        public void ResetRoomCanFire(int roomIndex)
        {
            roomsCanFire[roomIndex] = false;
        }

        public bool CanRoomFire(int roomIndex)
        {
            return roomsCanFire[roomIndex];
        }

        public RoomType GetRoomType(int roomIndex)
        {
            return roomsType[roomIndex];
        }

        public float GetRoomAttackTimeInterval(int roomIndex)
        {
            return roomsAttackTimeInterval[roomIndex];
        }

        public void AddRoomToDisarm(int roomIndex)
        {
            roomsToDisarm[roomIndex] = true;
        }

        public void DisarmRooms()
        {
            for (var i = 0; i < currentCapacity; i++)
            {
                if (!roomsToDisarm[i]) continue;
                roomsCanFire[i] = false;
                roomsToDisarm[i] = false;
            }
        }

        public Vector3 GetStartRoomPosition(int roomIndex)
        {
            return startRoomPositions[roomIndex];
        }

        public Vector3 GetExitRoomPosition(int roomIndex)
        {
            return exitRoomPositions[roomIndex];
        }

        public Vector3[] GetExitRoomPositions()
        {
            return exitRoomPositions;
        }

        public void ResizeStorage(int newCapacity)
        {
            Array.Resize(ref roomsCanFire, newCapacity);
            Array.Resize(ref roomsCurrentAttackTime, newCapacity);
            Array.Resize(ref roomsAttackTimeInterval, newCapacity);
            Array.Resize(ref roomsType, newCapacity);
            Array.Resize(ref roomsToDisarm, newCapacity);
            Array.Resize(ref exitRoomPositions, newCapacity);
            Array.Resize(ref startRoomPositions, newCapacity);
        }
    }
}