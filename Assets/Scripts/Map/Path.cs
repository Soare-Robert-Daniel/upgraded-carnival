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
}