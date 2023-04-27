using System;
using System.Collections.Generic;
using Mobs;
using UnityEngine;

namespace Map.Components
{
    [Serializable]
    public class Wave
    {
        public int mobIdGenerator = 0;
        public Wave(WaveMobNumberPerWaveModel waveMobNumberPerWaveModel)
        {
            WaveMobNumberPerWaveModel = waveMobNumberPerWaveModel;
        }

        public int CurrentWave { get; private set; } = 0;

        public Queue<(Mobs.Mob, MobDataScriptableObject)> MobsToSpawn { get; } = new Queue<(Mobs.Mob, MobDataScriptableObject)>();

        public WaveMobNumberPerWaveModel WaveMobNumberPerWaveModel { get; }

        public void SetWave(int wave)
        {
            CurrentWave = wave;
        }

        public void NextWave()
        {
            CurrentWave++;
        }

        public void CreateMobsToSpawn()
        {
            var interval = WaveMobNumberPerWaveModel.GetInterval(CurrentWave);

            foreach (var mob in interval.mobs)
            {
                for (var j = 0; j < mob.mobsNumber; j++)
                {
                    MobsToSpawn.Enqueue((new Mobs.Mob(mob.mobDataScriptableObject.baseStats)
                    {
                        id = mobIdGenerator,
                        bounty = mob.mobDataScriptableObject.bountyDataScriptableObject.ToBounty()
                    }, mob.mobDataScriptableObject));
                    mobIdGenerator++;
                }
            }

            Debug.Log($"[WAVE][GENERATE] Wave {CurrentWave} mobs to spawn: {MobsToSpawn.Count}");
        }

        public IEnumerable<(Mobs.Mob, MobDataScriptableObject)> DequeueMobsToSpawn()
        {
            while (MobsToSpawn.Count > 0)
            {
                yield return MobsToSpawn.Dequeue();
            }
        }

        public bool HasMobsToSpawn()
        {
            return MobsToSpawn.Count > 0;
        }
        public int GetMobsToSpawnCount()
        {
            return MobsToSpawn.Count;
        }
    }
}