using System;
using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    [Serializable]
    public class Path
    {

        public const int NotInZone = -1;
        [SerializeField] private List<Vector3> points;
        [SerializeField] private List<(Vector3, Vector3)> zonePoints;

        public Path()
        {
            points = new List<Vector3>();
            zonePoints = new List<(Vector3, Vector3)>();
        }

        public void AddPoint(Vector3 point)
        {
            points.Add(point);
        }

        public void AddZone(Vector3 p1, Vector3 p2)
        {
            zonePoints.Add((p1, p2));
        }

        public bool HasReachEnd(Vector3 position)
        {
            return Vector3.Distance(position, points[^1]) < 0.1f;
        }

        public int GetZoneIndex(Vector3 position)
        {
            var zoneIndex = NotInZone;
            for (var i = 0; i < zonePoints.Count; i++)
            {
                var (p1, p2) = zonePoints[i];
                if (p1.y >= position.y && position.y >= p2.y)
                {
                    zoneIndex = i;
                    break;
                }
            }

            return zoneIndex;
        }
    }

    [Serializable]
    public class Path2
    {

        public const int NoRoomFound = -1;
        [SerializeField] private Vector3[] enterPoints;
        [SerializeField] private Vector3[] exitPoints;
        [SerializeField] private int currentCapacity;
        [SerializeField] private float heightDistanceThreshold;

        public Path2(int initialCapacity = 0)
        {
            enterPoints = new Vector3[initialCapacity];
            exitPoints = new Vector3[initialCapacity];
            currentCapacity = 0;
        }

        public int CurrentCapacity => currentCapacity;

        public float HeightDistanceThreshold { get; set; }

        public void AddPath(Vector3 enterPoint, Vector3 exitPoint)
        {
            var newSlot = currentCapacity;
            if (currentCapacity >= enterPoints.Length)
            {
                ResizeStorage(newSlot + 1);
            }
            else
            {
                currentCapacity++;
            }

            enterPoints[newSlot] = enterPoint;
            exitPoints[newSlot] = exitPoint;
        }

        public bool NeedToTeleport(Vector3 position, int roomIndex)
        {
            return Mathf.Abs(position.y - enterPoints[roomIndex].y) >= HeightDistanceThreshold;
        }

        public bool IsInRoom(Vector3 position, int roomIndex)
        {
            var enterPoint = enterPoints[roomIndex];
            var exitPoint = exitPoints[roomIndex];

            return position.x >= enterPoint.x && position.x <= exitPoint.x && !NeedToTeleport(position, roomIndex);
        }

        public int FindRoomIndex(Vector3 position, int offset = 0)
        {
            for (var i = offset; i < currentCapacity; i++)
            {
                if (IsInRoom(position, i))
                {
                    return i;
                }
            }

            return NoRoomFound;
        }

        public void ResizeStorage(int newCapacity)
        {
            var newEnterPoints = new Vector3[newCapacity];
            var newExitPoints = new Vector3[newCapacity];

            Array.Copy(enterPoints, newEnterPoints, currentCapacity);
            Array.Copy(exitPoints, newExitPoints, currentCapacity);

            enterPoints = newEnterPoints;
            exitPoints = newExitPoints;
        }

        public Vector3 GetEnterPoint(int roomIndex)
        {
            return enterPoints[roomIndex];
        }

        public Vector3 GetExitPoint(int roomIndex)
        {
            return exitPoints[roomIndex];
        }

        public Vector3[] GetExitPoints()
        {
            return exitPoints;
        }
    }
}