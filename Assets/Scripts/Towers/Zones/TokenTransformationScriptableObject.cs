using System;
using UnityEngine;

namespace Towers.Zones
{
    [Serializable]
    public struct RankTypePair
    {
        public int rank;
        public ZoneTokenType tokenType;
    }

    [CreateAssetMenu(fileName = "Token Transformation", menuName = "Zone/Create new transformation", order = 0)]
    public class TokenTransformationScriptableObject : ScriptableObject
    {
        public RankTypePair[] from;
        public RankTypePair[] to;
    }
}