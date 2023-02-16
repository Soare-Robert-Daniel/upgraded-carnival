﻿using System;
using System.Collections.Generic;
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

        private Stack<int> roomsToDisarm;

        public RoomsSystem(int initialCapacity = 0)
        {
            roomsCanFire = new bool[initialCapacity];
            roomsCurrentAttackTime = new float[initialCapacity];
            roomsAttackTimeInterval = new float[initialCapacity];
            roomsType = new RoomType[initialCapacity];
            currentCapacity = 0;
        }

        public void AddRoom(RoomType roomType, float attackTimeInterval)
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

            roomsCanFire[newSlot] = false;
            roomsCurrentAttackTime[newSlot] = 0;
            roomsAttackTimeInterval[newSlot] = attackTimeInterval;
            roomsType[newSlot] = roomType;
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
            roomsToDisarm ??= new Stack<int>();
            roomsToDisarm.Push(roomIndex);
        }

        public void DisarmRooms()
        {
            if (roomsToDisarm == null) return;
            while (roomsToDisarm.Count > 0)
            {
                var roomIndex = roomsToDisarm.Pop();
                roomsCanFire[roomIndex] = false;
            }
        }

        public void ResizeStorage(int newCapacity)
        {
            Array.Resize(ref roomsCanFire, newCapacity);
            Array.Resize(ref roomsCurrentAttackTime, newCapacity);
            Array.Resize(ref roomsAttackTimeInterval, newCapacity);
            Array.Resize(ref roomsType, newCapacity);
        }
    }
}