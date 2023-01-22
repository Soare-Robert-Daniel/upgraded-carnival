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

        public Rune Clone()
        {
            return new Rune().SetRuneType(type).SetDuration(duration);
        }
    }

    public enum EntityClassType
    {
        BaseMobClass = 0,
        SimpleMobClass = 10,
        SimpleFastMobClass,
        HeavyMobClass = 100
    }

    [Serializable]
    public class EntityClass
    {
        [SerializeField] protected EntityClassType classType;

        public EntityClass()
        {
            classType = EntityClassType.BaseMobClass;
        }

        public EntityClassType ClassType => classType;
    }

    [SerializeField]
    public class SimpleMobClass : EntityClass
    {
        public SimpleMobClass()
        {
            classType = EntityClassType.SimpleMobClass;
        }
    }

    [SerializeField]
    public class SimpleFastMobClass : SimpleMobClass
    {
        public SimpleFastMobClass()
        {
            classType = EntityClassType.SimpleFastMobClass;
        }
    }

    [SerializeField]
    public class HeavyMobClass : EntityClass
    {
        public HeavyMobClass()
        {
            classType = EntityClassType.HeavyMobClass;
        }
    }

    [Serializable]
    public class EntityStats
    {
        [SerializeField] private EntityClass mobClass;
        [SerializeField] private float health;
        [SerializeField] private float maxHealth;
        [SerializeField] private float baseSpeed;
        [SerializeField] private List<Rune> runes;

        public float Health
        {
            get => health;
            set => health = value;
        }

        public float MaxHealth => maxHealth;

        public List<Rune> Runes
        {
            get => runes;
            set => runes = value;
        }

        public EntityClass MobClass
        {
            get => mobClass;
            set => mobClass = value;
        }

        public float BaseSpeed => baseSpeed;

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

        public void AddRunes(IEnumerable<Rune> runesToAdd)
        {
            runes.AddRange(runesToAdd);
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
            RemoveRunesType(new List<RuneType> { runeTypeToRemove });
        }

        public void RemoveRunesType(List<RuneType> runeTypesToRemove)
        {
            runes = runes.Where(rune => !runeTypesToRemove.Contains(rune.Type)).ToList();
        }
    }
}