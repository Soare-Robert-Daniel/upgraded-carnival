using UnityEngine;

namespace Mobs
{
    [System.Serializable]
    public class Mob
    {
        public int id;
        public int controllerId;

        public Vector3 position;
        public float health;
        public float baseSpeed;
        public float slow;

        public float Speed => baseSpeed * (1 - slow);
    }
}