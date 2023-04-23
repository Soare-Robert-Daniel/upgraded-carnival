using UnityEngine;

namespace Towers.Zones
{
    [CreateAssetMenu(fileName = "Zone Token Data", menuName = "Zone/Create new token", order = 0)]
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
    }
}