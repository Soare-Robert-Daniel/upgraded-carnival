﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameEntities
{

    [Serializable]
    public class RuneStorage
    {

        [SerializeField] private int resizingCapacity;
        [SerializeField] private int[] mobsIds;
        [SerializeField] private RuneType[] runesTypes;
        [SerializeField] private float[] runesDuration; // Update this value every frame.

        private int EMPTY_SLOT = -1;
        private Stack<int> emptySlots;

        private Dictionary<int, Dictionary<RuneType, int>> mobsRunes; // Read every frame.
        [SerializeField] private List<RuneData> runesToBeAdded;

        public RuneStorage(int initialCapacity = 0)
        {
            mobsIds = new int[initialCapacity];
            runesTypes = new RuneType[initialCapacity];
            runesDuration = new float[initialCapacity];
            emptySlots = new Stack<int>();
            mobsRunes = new Dictionary<int, Dictionary<RuneType, int>>();
            runesToBeAdded = new List<RuneData>();
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
            if (mobsRunes.ContainsKey(mobId))
            {
                mobsRunes[mobId][runeType] += 1;
            }
            else
            {
                mobsRunes.Add(mobId, new Dictionary<RuneType, int>());

                foreach (var rune in Enum.GetValues(typeof(RuneType)))
                {
                    mobsRunes[mobId].Add((RuneType)rune, 0);
                }

                mobsRunes[mobId][runeType] = 1;
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

            foreach (var runeData in runesToBeAdded)
            {
                var slot = emptySlots.Pop();
                mobsIds[slot] = runeData.mobId;
                runesTypes[slot] = runeData.runeType;
                runesDuration[slot] = runeData.runeDuration;
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
                mobsRunes[mobId][runesTypes[i]] = Math.Min(0, mobsRunes[mobId][runesTypes[i]] - 1);
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

        public struct RuneData
        {
            public int mobId;
            public RuneType runeType;
            public float runeDuration;
        }
    }
}