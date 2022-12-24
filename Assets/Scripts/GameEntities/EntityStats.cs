using System;
using UnityEngine;

namespace GameEntities
{
    [Serializable]
    public class EntityStats
    {
        [SerializeField] private float health;

        public float Health
        {
            get => health;
            set => health = value;
        }

        public bool IsAlive()
        {
            return health > 0;
        }
    }
}