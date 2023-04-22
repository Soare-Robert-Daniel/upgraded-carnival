using System.Collections.Generic;
using Mobs;
using UnityEngine;

namespace Towers
{
    public class ProjectileController : MonoBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private CircleCollider2D circleCollider2D;
        private ContactFilter2D contactFilter2D;
        private List<Collider2D> hitColliders;
        private List<int> mobsHit;

        public CircleCollider2D Collider => circleCollider2D;

        private void Awake()
        {
            hitColliders = new List<Collider2D>();
            contactFilter2D = new ContactFilter2D();
            mobsHit = new List<int>();
        }

        public List<int> MobsHit()
        {
            mobsHit.Clear();
            var count = Collider.OverlapCollider(contactFilter2D, hitColliders);
            for (var i = 0; i < count; i++)
            {
                var mob = hitColliders[i].GetComponent<MobSimpleController>();
                if (mob == null) continue;

                mobsHit.Add(mob.MobId);
            }

            return mobsHit;
        }
    }
}