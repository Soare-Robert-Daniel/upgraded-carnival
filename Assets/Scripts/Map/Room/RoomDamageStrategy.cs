using System;
using System.Collections.Generic;
using System.Linq;
using GameEntities;
using UnityEngine;

namespace Map.Room
{
    public class RoomDamageStrategy
    {
        public Dictionary<RuneType, float> DamagePerRune;
        public List<Rune> RuneToAdd;
        public List<RuneType> RuneTypeToRemove;
        public RoomDamageStrategy()
        {
            RuneToAdd = new List<Rune>();
            RuneTypeToRemove = new List<RuneType>();
            DamagePerRune = new Dictionary<RuneType, float>();
        }

        public float BaseDamage { get; private set; } = 0;

        public virtual float CalculateDamageForMob(EntityStats entityStats)
        {
            var damage = BaseDamage;

            foreach (var entityStatsRune in entityStats.Runes)
            {
                if (DamagePerRune.TryGetValue(entityStatsRune.Type, out var value))
                {
                    damage += value;
                }
            }

            return damage;
        }

        public virtual void BeforeDamageCalculationAction(EntityStats entityStats)
        {
            entityStats.AddRunes(RuneToAdd.Select(x => x.Clone()).ToList());
        }

        public virtual void AfterDamageCalculationAction(EntityStats entityStats)
        {
            entityStats.RemoveRunesType(RuneTypeToRemove);
        }


        public virtual void SetBaseDamage(float flatDamage)
        {
            BaseDamage = flatDamage;
        }

        public virtual void SetDamagePerRune(RuneType runeType, float damage)
        {
            DamagePerRune[runeType] = damage;
        }

        public virtual void LoadFromModel(RoomModel roomModel)
        {
            BaseDamage = roomModel.baseDamage;
            foreach (var roomModelDamagePerRune in roomModel.runeAction)
            {
                SetDamagePerRune(roomModelDamagePerRune.type, roomModelDamagePerRune.damage);
                switch (roomModelDamagePerRune.actionType)
                {
                    case RoomRuneHandlingType.Add:
                        RuneToAdd.Add(new Rune().SetRuneType(roomModelDamagePerRune.type).SetDuration(roomModelDamagePerRune.duration));
                        break;
                    case RoomRuneHandlingType.Remove:
                        RuneTypeToRemove.Add(roomModelDamagePerRune.type);
                        break;
                    case RoomRuneHandlingType.Keep:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Debug.Log(RuneToAdd.Count);
        }
    }
}