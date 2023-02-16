using System;
using System.Collections.Generic;
using System.Linq;
using GameEntities;

namespace Map.Room
{

    public class RoomRuneHandler
    {
        private Dictionary<RuneType, float> damagePerRune;

        private float[] runeDurationToAdd;

        private RuneType[] runeTypeToAdd;
        private RuneType[] runeTypeToRemove;
        private Dictionary<RuneType, float> slowPerRune;

        public RoomRuneHandler()
        {
            runeTypeToAdd = Array.Empty<RuneType>();
            runeDurationToAdd = Array.Empty<float>();
            runeTypeToRemove = Array.Empty<RuneType>();
            damagePerRune = new Dictionary<RuneType, float>();
            slowPerRune = new Dictionary<RuneType, float>();
        }

        public float BaseDamage { get; private set; } = 0;

        public void ComputeDamageAndSlowForMobWithRuneStorage(RuneStorage runeStorage, int modId, out float damage, out float slow)
        {
            damage = BaseDamage;
            slow = 0f;

            foreach (var rune in runeStorage.GetAllRunesCountForEntity(modId))
            {
                if (damagePerRune.TryGetValue(rune.Key, out var value))
                {
                    damage += value * rune.Value; // value * Count
                }
                if (slowPerRune.TryGetValue(rune.Key, out var slowValue))
                {
                    slow += slowValue * rune.Value; // value * Count
                }
            }
        }

        public void AddRunesToStorageForEntity(RuneStorage runeStorage, int entityId)
        {
            for (var i = 0; i < runeTypeToAdd.Length; i++)
            {
                runeStorage.AddRune(entityId, runeTypeToAdd[i], runeDurationToAdd[i]);
            }
        }

        public void RemoveRunesFromStorageForEntity(RuneStorage runeStorage, int entityId)
        {
            runeStorage.RemoveRunesForEntity(runeTypeToRemove.ToArray(), entityId);
        }

        public void SetBaseDamage(float flatDamage)
        {
            BaseDamage = flatDamage;
        }

        public void SetDamagePerRune(RuneType runeType, float damage)
        {
            damagePerRune[runeType] = damage;
        }

        public void SetSlowPerRune(RuneType runeType, float slow)
        {
            slowPerRune[runeType] = slow;
        }

        public void LoadFromModel(RoomModel roomModel)
        {
            BaseDamage = roomModel.baseDamage;
            foreach (var roomModelDamagePerRune in roomModel.runesAction)
            {
                SetDamagePerRune(roomModelDamagePerRune.type, roomModelDamagePerRune.damage);
                SetSlowPerRune(roomModelDamagePerRune.type, roomModelDamagePerRune.slow);
            }

            runeTypeToAdd = roomModel.runesAction.Where(rune => rune.actionType == RoomRuneHandlingType.Add)
                .Select(rune => rune.type).ToArray();

            runeDurationToAdd = roomModel.runesAction.Where(rune => rune.actionType == RoomRuneHandlingType.Add).Select(rune => rune.duration)
                .ToArray();

            runeTypeToRemove = roomModel.runesAction.Where(rune => rune.actionType == RoomRuneHandlingType.Remove)
                .Select(rune => rune.type).ToArray();
        }
    }
}