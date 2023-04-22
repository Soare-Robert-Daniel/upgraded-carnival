using UnityEngine;

namespace Towers.Zones
{
    [CreateAssetMenu(fileName = "Token Rank Data", menuName = "Zone/Create new token rank", order = 0)]
    public class TokenRankData : ScriptableObject
    {
        public int rank;
        public float totalDuration;
        public float damage;

        [Range(0f, 1f)]
        public float slow;
    }
}