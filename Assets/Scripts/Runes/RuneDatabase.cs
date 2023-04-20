using System;
using System.Collections.Generic;
using System.Linq;
using GameEntities;

namespace Runes
{
    [Serializable]
    public class RuneDatabase
    {
        public RuneDuration durationForDynamicRunes;
        public Dictionary<int, List<RuneToken>> mobsRunesCollection;
        public Dictionary<int, Dictionary<RuneType, int>> mobsRunesCount;
        public SortedSet<int> runesToBeRemoved;

        public RuneDatabase()
        {
            mobsRunesCount = new Dictionary<int, Dictionary<RuneType, int>>();
            mobsRunesCollection = new Dictionary<int, List<RuneToken>>();
            runesToBeRemoved = new SortedSet<int>();
            durationForDynamicRunes = new RuneDuration(500);
        }

        public void AddRuneToken(RuneToken runeToken)
        {

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
                mobsRunesCollection.Add(runeToken.mobId, new List<RuneToken>());
            }

            if (runeToken.runeDynamics == RuneDynamics.Dynamic)
            {
                // durationForDynamicRunes.Add(runeToken.id, runeToken.mobId, runeToken.duration);
            }

            mobsRunesCollection[runeToken.mobId].Add(runeToken);
            mobsRunesCount[runeToken.mobId][runeToken.runeType]++;
        }


        public void RemoveTokenFrom(RuneToken runeToken)
        {
            // Remove from mobs collection.
            mobsRunesCollection[runeToken.mobId].Remove(runeToken);
            mobsRunesCount[runeToken.mobId][runeToken.runeType]--;
        }

        public void ManualCleanup()
        {

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