using System;
using System.Collections.Generic;
using UnityEngine;

namespace Map.Components
{
    [Serializable]
    public class MobsNumberInterval
    {
        public int fromWave;
        public int mobsNumber;
    }

    [CreateAssetMenu(fileName = "Wave Mobs Number Model", menuName = "Wave/ Mobs Per Wave Model", order = 0)]
    public class WaveMobNumberPerWaveModel : ScriptableObject
    {
        public List<MobsNumberInterval> intervals;
    }
}