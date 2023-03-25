using System;
using System.Collections.Generic;
using Economy;
using GameEntities;
using UnityEngine;

namespace Map.Components
{
    public class MobToSpawn
    {
        public MobStats Stats { get; set; }
        public Bounty Bounty { get; set; }
    }

    public class MobBuilder
    {
        private Bounty bounty;
        private MobModel mobModel;
        private MobStats mobStats;

        public MobBuilder(MobModel mobModel)
        {
            this.mobModel = mobModel;
            mobStats = new MobStats(mobModel.stats);
            bounty = new Bounty(mobModel.bounty);
        }

        public MobToSpawn Build()
        {
            return new MobToSpawn
            {
                Stats = mobStats,
                Bounty = bounty
            };
        }

        #region Accessors

        public MobBuilder SetHealth(float health)
        {
            mobStats.health = health;
            return this;
        }

        public MobBuilder SetSpeed(float speed)
        {
            mobStats.speed = speed;
            return this;
        }

        public MobBuilder SetCurrency(Currency currency)
        {
            bounty.currency = currency;
            return this;
        }

        public MobBuilder SetBountyValue(float value)
        {
            bounty.value = value;
            return this;
        }

        #endregion

    }

    [Serializable]
    public class Wave
    {
        public Wave(WaveMobNumberPerWaveModel waveMobNumberPerWaveModel, Dictionary<MobClassType, MobModel> mobModels)
        {
            WaveMobNumberPerWaveModel = waveMobNumberPerWaveModel;
            foreach (var mobModel in mobModels)
            {
                MobBuilders.Add(mobModel.Key, new MobBuilder(mobModel.Value));
            }
        }

        public int CurrentWave { get; private set; } = 0;

        public Queue<MobToSpawn> MobsToSpawn { get; } = new Queue<MobToSpawn>();

        public WaveMobNumberPerWaveModel WaveMobNumberPerWaveModel { get; }

        public Dictionary<MobClassType, MobBuilder> MobBuilders { get; } = new Dictionary<MobClassType, MobBuilder>();

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
                    MobsToSpawn.Enqueue(MobBuilders[mob.mobClass].Build());
                }
            }

            Debug.Log($"Wave {CurrentWave} mobs to spawn: {MobsToSpawn.Count}");
        }

        public IEnumerable<MobToSpawn> DequeueMobsToSpawn()
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