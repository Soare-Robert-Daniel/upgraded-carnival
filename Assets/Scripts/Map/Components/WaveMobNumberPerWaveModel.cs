using System;
using System.Collections.Generic;
using System.Linq;
using GameEntities;
using UnityEngine;

namespace Map.Components
{
    [Serializable]
    public class MobsTypePair
    {
        public MobClassType mobClass;
        public int mobsNumber;
    }

    [Serializable]
    public class MobsNumberInterval
    {
        public int fromWave;
        public int mobsNumber;
        public List<MobsTypePair> mobs;
    }

    [CreateAssetMenu(fileName = "Wave Mobs Number Model", menuName = "Wave/ Mobs Per Wave Model", order = 0)]
    public class WaveMobNumberPerWaveModel : ScriptableObject
    {
        public List<MobsNumberInterval> intervals;

        public MobsNumberInterval GetInterval(int waveNumber)
        {
            return intervals.FirstOrDefault(interval => interval.fromWave <= waveNumber);
        }
    }
}