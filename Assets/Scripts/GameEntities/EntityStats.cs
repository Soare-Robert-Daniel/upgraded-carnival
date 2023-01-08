using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameEntities
{

    public enum RuneType
    {
        Slow,
        Attack
    }

    [Serializable]
    public class Rune
    {
        [SerializeField] protected RuneType type;
        [SerializeField] protected float duration;

        public RuneType Type => type;

        public float Duration => duration;

        public bool CanBeRemoved => duration < 0.0000000001f;

        public void DecreaseDuration(float time)
        {
            duration -= time;
        }

        public Rune SetRuneType(RuneType runeType)
        {
            type = runeType;
            return this;
        }

        public Rune SetDuration(float totalDuration)
        {
            this.duration = totalDuration;
            return this;
        }
    }

    [Serializable]
    public class EntityStats
    {
        [SerializeField] private float health;
        [SerializeField] private float maxHealth;
        [SerializeField] private List<Rune> runes;

        public float Health
        {
            get => health;
            set => health = value;
        }

        public float MaxHealth
        {
            get => maxHealth;
            set => maxHealth = value;
        }

        public List<Rune> Runes
        {
            get => runes;
            set => runes = value;
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

        public void AddRune(Rune rune)
        {
            runes.Add(rune);
        }

        public void DecreaseRunesDuration(float time)
        {
            foreach (var rune in runes)
            {
                rune.DecreaseDuration(time);
            }
        }

        public void RemoveExpiredRunes()
        {
            runes = runes.Where(rune => !rune.CanBeRemoved).ToList();
        }

        public void RemoveRuneType(RuneType runeTypeToRemove)
        {
            runes = runes.Where(rune => runeTypeToRemove != rune.Type).ToList();
        }
    }
}