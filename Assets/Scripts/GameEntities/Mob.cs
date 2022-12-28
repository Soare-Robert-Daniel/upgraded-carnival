using UnityEngine;

namespace GameEntities
{
    public class Mob : MonoBehaviour
    {
        public int id;
        public Vector3 target;
        public EntityStats stats;
        public Vector3 movementDirection = Vector3.right;
        public float speed = 2f;

        public void ApplyDamage(float damage)
        {
            stats.Health -= damage;
        }
    }
}