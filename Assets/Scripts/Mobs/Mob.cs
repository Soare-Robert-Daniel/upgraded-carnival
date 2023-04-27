using System;
using Economy;
using UnityEngine;

namespace Mobs
{
    [Serializable]
    public struct BaseStats
    {
        public float baseSpeed;
        public float baseHealth;
    }

    [Serializable]
    public class Mob : ICloneable
    {
        public int id;
        public int controllerId;

        public Vector3 position;
        public Bounty bounty;

        public readonly BaseStats baseStats;

        private float health;
        private float slow;
        private float speed;

        public Mob(BaseStats baseStats)
        {
            this.baseStats = baseStats;
            id = -1;
            controllerId = -1;
            position = Vector3.zero;
            health = baseStats.baseHealth;
            speed = baseStats.baseSpeed;
            slow = 0f;
        }

        public bool IsDead => health <= 0;

        public float Speed
        {
            get => speed;
            set => speed = Mathf.Max(0f, value);
        }

        public float Health
        {
            get => health;
            set => health = Mathf.Max(0f, value);
        }

        public float Slow
        {
            get => slow;
            set => slow = Mathf.Clamp01(value);
        }

        public float HealthPercent => health / baseStats.baseHealth;
        public object Clone()
        {
            return new Mob(baseStats)
            {
                id = id,
                controllerId = controllerId,
                position = position,
                bounty = bounty,
                health = health,
                slow = slow,
                speed = speed
            };
        }

        public void UpdateSpeed()
        {
            speed = baseStats.baseSpeed * (1f - slow);
            Debug.Log($"Speed: {speed} | Slow: {slow} | BaseSpeed: {baseStats.baseSpeed}");
        }

        public void ResetAll()
        {
            ResetHealth();
            ResetSpeed();
            ResetSlow();
        }

        public void ResetHealth()
        {
            health = baseStats.baseHealth;
        }

        public void ResetSpeed()
        {
            speed = baseStats.baseSpeed;
        }

        public void ResetSlow()
        {
            slow = 0f;
        }
    }
}