using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Towers.Zones
{
    [CreateAssetMenu(fileName = "Zone Token Data", menuName = "Zone/Create new", order = 0)]
    public class ZoneTokenDataScriptableObject : ScriptableObject
    {
        public ZoneTokenType zoneTokenType;

        public Trigger trigger;

        public float baseTickInterval;

        public TokenRankData[] ranks;
        public TokenTransformationScriptableObject[] transformations;

        [Header("Resources")]
        public TowerPriceScriptableObject price;

        public TowerResourcesScriptableObject resourcesScriptableObject;
        public ZoneResourcesScriptableObject zoneResourcesScriptableObject;

        private int maxRank;
        private Dictionary<int, TokenRankData> ranksDictionary;

        public Dictionary<int, TokenRankData> GetRanksDictionary()
        {
            if (ranksDictionary != null) return ranksDictionary;

            ranksDictionary = new Dictionary<int, TokenRankData>();
            foreach (var rankData in ranks)
            {
                ranksDictionary.Add(rankData.rank, rankData);
            }

            return ranksDictionary;
        }

        public int GetMaxRank()
        {
            if (maxRank != 0) return maxRank;

            maxRank = ranks.Max(rankData => rankData.rank);
            return maxRank;
        }

        public bool TryGetRankData(int rank, out TokenRankData rankData)
        {
            var r = Mathf.Min(rank, GetMaxRank());
            return GetRanksDictionary().TryGetValue(r, out rankData);
        }
    }
}