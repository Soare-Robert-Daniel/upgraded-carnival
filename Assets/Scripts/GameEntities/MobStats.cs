using System;

namespace GameEntities
{
    [Serializable]
    public struct MobStats
    {
        public MobClassType mobClass;
        public float health;
        public float speed;

        public MobStats(MobClassType mobClass = MobClassType.SimpleMobClass, float health = 0, float speed = 0)
        {
            this.mobClass = mobClass;
            this.health = health;
            this.speed = speed;
        }

        public MobStats(MobStats mobStats)
        {
            mobClass = mobStats.mobClass;
            health = mobStats.health;
            speed = mobStats.speed;
        }
    }
}