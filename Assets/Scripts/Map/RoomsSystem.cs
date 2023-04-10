using System;
using System.Collections.Generic;
using Map.Room;
using Runes;
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
        [SerializeField] private List<RunesHandlerForRoom> roomsRunesHandlers;
        private bool[] roomsToDisarm;

        public RoomsSystem(int initialCapacity = 0)
        {
            roomsCanFire = new bool[initialCapacity];
            roomsCurrentAttackTime = new float[initialCapacity];
            roomsAttackTimeInterval = new float[initialCapacity];
            roomsType = new RoomType[initialCapacity];
            roomsToDisarm = new bool[initialCapacity];
            roomsRunesHandlers = new List<RunesHandlerForRoom>();

            currentCapacity = 0;
        }

        public int CurrentCapacity => currentCapacity;

        public int FinalRoom => currentCapacity - 1;

        public int AddRoom(RoomType roomType, float attackTimeInterval, Vector3 startRoomPosition, Vector3 exitRoomPosition)
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
            roomsRunesHandlers.Add(new RunesHandlerForRoom());

            return newSlot;
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

        public void SetType(int roomIndex, RoomType roomType)
        {
            roomsType[roomIndex] = roomType;
        }

        public void SetAttackTimeInterval(int roomIndex, float attackTimeInterval)
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

        public float GetAttackTimeInterval(int roomIndex)
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

        public void ChangeRuneHandler(int roomIndex, RunesHandlerForRoom runesHandler)
        {
            roomsRunesHandlers[roomIndex] = runesHandler;
        }

        public RunesHandlerForRoom GetRunesHandler(int roomIndex)
        {
            return roomsRunesHandlers[roomIndex];
        }

        public void ResizeStorage(int newCapacity)
        {
            Array.Resize(ref roomsCanFire, newCapacity);
            Array.Resize(ref roomsCurrentAttackTime, newCapacity);
            Array.Resize(ref roomsAttackTimeInterval, newCapacity);
            Array.Resize(ref roomsType, newCapacity);
            Array.Resize(ref roomsToDisarm, newCapacity);
        }
    }
}