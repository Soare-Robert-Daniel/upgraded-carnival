using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameEntities
{
    public struct RuneToken
    {
        public int mobId;
        public RuneType runeType;
        public float duration;
        public float value;

        public RuneToken(int mobId, RuneType runeType, float duration, float value)
        {
            this.mobId = mobId;
            this.runeType = runeType;
            this.duration = duration;
            this.value = value;
        }

    }

    [Serializable]
    public class RuneStorage
    {

        [SerializeField] private int resizingCapacity;
        [SerializeField] private int[] mobsIds;
        [SerializeField] private RuneType[] runesTypes;
        [SerializeField] private float[] runesDuration; // Update this value every frame.
        [SerializeField] private List<RuneData> runesToBeAdded;

        private int EMPTY_SLOT = -1;
        private Stack<int> emptySlots;

        private Dictionary<int, Dictionary<RuneType, int>> mobsRunesCount; // Read every frame.
        private Dictionary<int, List<int>> mobsRunesSlots; // Read every frame.

        public RuneStorage(int initialCapacity = 0)
        {
            mobsIds = new int[initialCapacity];
            runesTypes = new RuneType[initialCapacity];
            runesDuration = new float[initialCapacity];
            emptySlots = new Stack<int>();
            mobsRunesCount = new Dictionary<int, Dictionary<RuneType, int>>();
            runesToBeAdded = new List<RuneData>();
            mobsRunesSlots = new Dictionary<int, List<int>>();
            resizingCapacity = 1;

            for (var i = 0; i < initialCapacity; i++)
            {
                mobsIds[i] = EMPTY_SLOT;
                emptySlots.Push(i);
            }
        }

        public int ResizingCapacity
        {
            get => resizingCapacity;
            set => resizingCapacity = value;
        }

        public int Capacity => mobsIds.Length;

        public void AddRune(int mobId, RuneType runeType, float runeDuration)
        {
            // Counting runes for each mob.
            if (mobsRunesCount.ContainsKey(mobId))
            {
                if (mobsRunesCount[mobId].ContainsKey(runeType))
                {
                    mobsRunesCount[mobId][runeType] += 1;
                }
                else
                {
                    mobsRunesCount[mobId].Add(runeType, 1);
                }
            }
            else
            {
                mobsRunesCount.Add(mobId, new Dictionary<RuneType, int>());
                mobsRunesCount[mobId].Add(runeType, 1);
            }

            runesToBeAdded.Add(new RuneData
            {
                mobId = mobId,
                runeType = runeType,
                runeDuration = runeDuration
            });
        }

        public void TransferBufferToStorage()
        {
            if (runesToBeAdded.Count == 0) return;

            if (emptySlots.Count < runesToBeAdded.Count)
            {
                var oldSize = mobsIds.Length;
                ResizeStorage(mobsIds.Length + runesToBeAdded.Count - emptySlots.Count + 10);
                for (var i = oldSize; i < mobsIds.Length; i++)
                {
                    mobsIds[i] = EMPTY_SLOT;
                }

                ScanStorage();
            }

            foreach (var runeData in runesToBeAdded.TakeWhile(runeData => emptySlots.Count != 0))
            {
                var slot = emptySlots.Pop();
                mobsIds[slot] = runeData.mobId;
                runesTypes[slot] = runeData.runeType;
                runesDuration[slot] = runeData.runeDuration;
            }

            if (runesToBeAdded.Count > 0)
            {
                Debug.LogError($"RuneStorage: Not enough empty slots. There are {runesToBeAdded.Count} runes that couldn't be added.");
            }

            runesToBeAdded.Clear();
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
        public void RemoveRunesForEntity(RuneType[] runeTypes, int mobId)
        {
            for (var i = 0; i < mobsIds.Length; i++)
            {
                if (mobsIds[i] != mobId) continue;

                if (!Array.Exists(runeTypes, runeType => runeType == runesTypes[i])) continue;

                mobsIds[i] = EMPTY_SLOT;
                mobsRunesCount[mobId][runesTypes[i]] = Math.Min(0, mobsRunesCount[mobId][runesTypes[i]] - 1);
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

        public void ResizeStorageIfNecessary(int newCapacity)
        {
            if (newCapacity <= mobsIds.Length) return;
            ResizeStorage(newCapacity);
        }

        public void ScanStorage()
        {
            for (var i = 0; i < mobsIds.Length; i++)
            {
                if (mobsIds[i] == EMPTY_SLOT)
                {
                    emptySlots.Push(i);
                }
            }
        }

        public Dictionary<RuneType, int> GetRuneCountForEntity(int mobId)
        {
            if (!mobsRunesCount.ContainsKey(mobId))
            {
                mobsRunesCount.Add(mobId, new Dictionary<RuneType, int>());
            }
            return mobsRunesCount[mobId];
        }

        [Serializable]
        public struct RuneData
        {
            public int mobId;
            public RuneType runeType;
            public float runeDuration;
        }
    }
}