using System;
using System.Collections.Generic;
using System.Linq;
using GameEntities;

namespace Map.Room
{

    public class RoomDamageStrategy
    {
        public Dictionary<RuneType, float> DamagePerRune;
        public float[] RuneDurationToAdd;

        public RuneType[] RuneTypeToAdd;
        public RuneType[] RuneTypeToRemove;

        public RoomDamageStrategy()
        {
            RuneTypeToAdd = Array.Empty<RuneType>();
            RuneDurationToAdd = Array.Empty<float>();
            RuneTypeToRemove = Array.Empty<RuneType>();
            DamagePerRune = new Dictionary<RuneType, float>();
        }

        public float BaseDamage { get; private set; } = 0;

        public virtual float ComputeDamageForEntityWithRuneStorage(RuneStorage runeStorage, int entityId)
        {
            var damage = BaseDamage;

            foreach (var rune in runeStorage.GetAllRunesCountForEntity(entityId))
            {
                if (DamagePerRune.TryGetValue(rune.Key, out var value))
                {
                    damage += value * rune.Value;
                }
            }

            return damage;
        }

        public virtual void AddRunesToStorageForEntity(RuneStorage runeStorage, int entityId)
        {
            for (var i = 0; i < RuneTypeToAdd.Length; i++)
            {
                runeStorage.AddRune(entityId, RuneTypeToAdd[i], RuneDurationToAdd[i]);
            }
        }

        public virtual void RemoveRunesFromStorageForEntity(RuneStorage runeStorage, int entityId)
        {
            runeStorage.RemoveRunesForEntity(RuneTypeToRemove.ToArray(), entityId);
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
            foreach (var roomModelDamagePerRune in roomModel.runesAction)
            {
                SetDamagePerRune(roomModelDamagePerRune.type, roomModelDamagePerRune.damage);
            }

            RuneTypeToAdd = roomModel.runesAction.Where(rune => rune.actionType == RoomRuneHandlingType.Add)
                .Select(rune => rune.type).ToArray();

            RuneDurationToAdd = roomModel.runesAction.Where(rune => rune.actionType == RoomRuneHandlingType.Add).Select(rune => rune.duration)
                .ToArray();

            RuneTypeToRemove = roomModel.runesAction.Where(rune => rune.actionType == RoomRuneHandlingType.Remove)
                .Select(rune => rune.type).ToArray();
        }
    }
}