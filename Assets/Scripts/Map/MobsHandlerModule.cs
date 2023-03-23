using System;
using System.Collections.Generic;
using GameEntities;

namespace Map
{
    public class MobsHandlerModule
    {

        public MobsHandlerModule()
        {
            MobsCurrentCount = new Dictionary<MobClassType, int>();
            MobsCountToDeploy = new Dictionary<MobClassType, int>();
        }
        public Dictionary<MobClassType, int> MobsCurrentCount { get; private set; }
        public Dictionary<MobClassType, int> MobsCountToDeploy { get; private set; }

        public void AddMob(MobClassType e)
        {
            if (MobsCurrentCount.ContainsKey(e))
            {
                MobsCurrentCount[e]++;
            }
            else
            {
                MobsCurrentCount.Add(e, 1);
            }
        }

        public void SetMobCountToDeploy(MobClassType e, int count)
        {
            if (MobsCountToDeploy.ContainsKey(e))
            {
                MobsCountToDeploy[e] = count;
            }
            else
            {
                MobsCountToDeploy.Add(e, count);
            }
        }

        public List<Tuple<MobClassType, int>> GetMobsToDeployDiff()
        {
            var result = new List<Tuple<MobClassType, int>>();
            foreach (var mob in MobsCountToDeploy)
            {
                if (MobsCurrentCount.ContainsKey(mob.Key))
                {
                    var diff = mob.Value - MobsCurrentCount[mob.Key];
                    if (diff > 0)
                    {
                        result.Add(new Tuple<MobClassType, int>(mob.Key, diff));
                    }
                }
                else
                {
                    result.Add(new Tuple<MobClassType, int>(mob.Key, mob.Value));
                }
            }
            return result;
        }

        public void Reset()
        {
            MobsCurrentCount.Clear();
        }
    }
}