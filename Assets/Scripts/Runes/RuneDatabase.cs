using System;
using System.Collections.Generic;
using System.Linq;
using GameEntities;

namespace Runes
{
    [Serializable]
    public class RuneDatabase
    {
        public List<RuneToken> data;
        public Dictionary<int, List<int>> mobsRunesCollection;
        public Dictionary<int, Dictionary<RuneType, int>> mobsRunesCount;
        public SortedSet<int> runesToBeRemoved;

        public RuneDatabase()
        {
            data = new List<RuneToken>();
            mobsRunesCount = new Dictionary<int, Dictionary<RuneType, int>>();
            mobsRunesCollection = new Dictionary<int, List<int>>();
            runesToBeRemoved = new SortedSet<int>();
        }

        public void AddRuneToken(RuneToken runeToken)
        {
            data.Add(runeToken);

            if (!mobsRunesCount.ContainsKey(runeToken.mobId))
            {
                mobsRunesCount.Add(runeToken.mobId, new Dictionary<RuneType, int>());
            }
            if (!mobsRunesCount[runeToken.mobId].ContainsKey(runeToken.runeType))
            {
                mobsRunesCount[runeToken.mobId].Add(runeToken.runeType, 0);
            }

            if (!mobsRunesCollection.ContainsKey(runeToken.mobId))
            {
                mobsRunesCollection.Add(runeToken.mobId, new List<int>());
            }

            mobsRunesCollection[runeToken.mobId].Add(runeToken.id);
            mobsRunesCount[runeToken.mobId][runeToken.runeType]++;
        }

        public void RemoveRuneToken(RuneToken runeToken)
        {
            mobsRunesCollection[runeToken.mobId].Remove(runeToken.id);
            runesToBeRemoved.Add(runeToken.id);
            mobsRunesCount[runeToken.mobId][runeToken.runeType]--;
        }

        public void ManualCleanup()
        {
            data = data.Where((runeToken, index) => !runesToBeRemoved.Contains(index)).ToList();
            runesToBeRemoved.Clear();

            // Cleanup empty mobs.
            foreach (var mobId in mobsRunesCollection.Keys.ToList().Where(mobId => mobsRunesCollection[mobId].Count == 0))
            {
                mobsRunesCollection.Remove(mobId);
                mobsRunesCount.Remove(mobId);
            }
        }
    }
}