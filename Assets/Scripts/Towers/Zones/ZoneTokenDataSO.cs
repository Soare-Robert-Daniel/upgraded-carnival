using UnityEngine;

namespace Towers.Zones
{
    [CreateAssetMenu(fileName = "Zone Token Data", menuName = "Zone/Create new token", order = 0)]
    public class ZoneTokenDataScriptableObject : ScriptableObject
    {
        public ZoneTokenType zoneTokenType;
        public DamageTrigger damageTrigger;

        public TokenRankData[] ranks;
        public TokenTransformationScriptableObject[] transformations;
    }
}