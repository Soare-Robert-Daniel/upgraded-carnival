using UnityEngine;

namespace GameEntities
{
    public class Mob : MonoBehaviour
    {
        public EntityStats stats;

        public void ApplyDamage(float damage)
        {
            stats.Health -= damage;
        }
    }
}