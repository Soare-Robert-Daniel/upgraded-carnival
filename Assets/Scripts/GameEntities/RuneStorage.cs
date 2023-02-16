using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameEntities
{

    [Serializable]
    public class RuneStorage
    {

        [SerializeField] private int[] mobsIds;
        [SerializeField] private RuneType[] runesTypes;
        [SerializeField] private float[] runesDuration;
        private int EMPTY_SLOT = -1;
        private Stack<int> emptySlots;

        public RuneStorage(int initialCapacity = 0)
        {
            mobsIds = new int[initialCapacity];
            runesTypes = new RuneType[initialCapacity];
            runesDuration = new float[initialCapacity];
            emptySlots = new Stack<int>();
        }

        public void AddRune(int mobId, RuneType runeType, float runeDuration)
        {
            // Reuse the empty slots from expired runes if possible.
            if (emptySlots.Count > 0)
            {
                var slot = emptySlots.Pop();
                mobsIds[slot] = mobId;
                runesTypes[slot] = runeType;
                runesDuration[slot] = runeDuration;
            }
            else
            {
                var newSlot = mobsIds.Length;
                ResizeStorage(newSlot + 1);
                mobsIds[newSlot] = mobId;
                runesTypes[newSlot] = runeType;
                runesDuration[newSlot] = runeDuration;
            }
        }

        public void UpdateRunesDurations(float deltaTime)
        {
            /*
             * Note: If we call this function before the start of the game, the stack will have all the empty slots available.
             */

            for (var i = 0; i < runesDuration.Length; i++)
            {
                if (runesDuration[i] < 0) continue;

                runesDuration[i] -= deltaTime;

                if (!(runesDuration[i] < 0)) continue;
                mobsIds[i] = EMPTY_SLOT;
                emptySlots.Push(i);
            }
        }

        public Dictionary<RuneType, int> GetAllRunesCountForEntity(int mobId)
        {
            var result = new Dictionary<RuneType, int>();
            for (var i = 0; i < mobsIds.Length; i++)
            {
                if (mobsIds[i] != mobId) continue;
                if (!result.ContainsKey(runesTypes[i]))
                {
                    result[runesTypes[i]] = 0;
                }
                result[runesTypes[i]]++;
            }
            return result;
        }

        public void RemoveRunesForEntity(RuneType[] runeTypes, int mobId)
        {
            for (var i = 0; i < mobsIds.Length; i++)
            {
                if (mobsIds[i] != mobId) continue;

                if (!Array.Exists(runeTypes, runeType => runeType == runesTypes[i])) continue;

                mobsIds[i] = EMPTY_SLOT;
                emptySlots.Push(i);
            }
        }

        public void ResizeStorage(int newCapacity)
        {
            Array.Resize(ref mobsIds, newCapacity);
            Array.Resize(ref runesTypes, newCapacity);
            Array.Resize(ref runesDuration, newCapacity);
        }

        public void Clear()
        {
            mobsIds = Array.Empty<int>();
            runesTypes = Array.Empty<RuneType>();
            runesDuration = Array.Empty<float>();
            emptySlots.Clear();
        }
    }
}