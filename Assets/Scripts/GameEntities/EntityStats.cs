using System;
using UnityEngine;

namespace GameEntities
{
    [Serializable]
    public class EntityStats
    {
        [SerializeField] private float health;
        [SerializeField] private float maxHealth;

        public float Health
        {
            get => health;
            set => health = value;
        }

        public bool IsAlive()
        {
            return health > 0;
        }

        public float RemainingHealthPercentage()
        {
            if (maxHealth < 0.000000001f)
            {
                return 0f;
            }
            return health / maxHealth;
        }
    }
}