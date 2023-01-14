using System;
using UnityEngine;

namespace GameEntities
{
    [CreateAssetMenu(fileName = "MobEntityModel", menuName = "Mob/Create Mob Model", order = 0)]
    public class EntityModel : ScriptableObject
    {
        [SerializeField] protected EntityClassType classType;
        [SerializeField] protected EntityStats stats;

        public EntityClass GetEntityClass()
        {
            return classType switch
            {
                EntityClassType.BaseMobClass => new EntityClass(),
                EntityClassType.SimpleMobClass => new SimpleMobClass(),
                EntityClassType.SimpleFastMobClass => new SimpleFastMobClass(),
                EntityClassType.HeavyMobClass => new HeavyMobClass(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public EntityStats GetStats()
        {
            stats.MobClass = GetEntityClass();
            return stats;
        }
    }
}