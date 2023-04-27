using System.Collections.Generic;
using Mobs;
using UnityEngine;

namespace Map.Components
{
    public class EndController : MonoBehaviour
    {
        [SerializeField] private MapManager mapManager;
        [SerializeField] private BoxCollider2D boxCollider2D;
        [SerializeField] private float checkInterval;
        [SerializeField] private List<Collider2D> colliders;
        [SerializeField] private ContactFilter2D contactFilter2D;

        private float currentTime;

        private void Awake()
        {
            colliders = new List<Collider2D>();
        }

        private void Update()
        {
            currentTime += Time.deltaTime;
        }

        private void FixedUpdate()
        {
            if (currentTime < checkInterval) return;

            currentTime = 0;
            var count = boxCollider2D.OverlapCollider(new ContactFilter2D(), colliders);
            for (var i = 0; i < count; i++)
            {
                var mob = colliders[i].GetComponent<MobSimpleController>();
                if (mob == null) continue;

                mapManager.MobReachedEnd(mob.MobId);
            }
        }
    }
}