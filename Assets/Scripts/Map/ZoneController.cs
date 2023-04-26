using System.Collections.Generic;
using Mobs;
using Towers.Zones;
using UnityEngine;

namespace Map
{
    public class ZoneController : MonoBehaviour
    {
        public int zoneId;
        public List<RoomController> roomsControllers;
        public Transform northBound;
        public Transform southBound;
        public ZoneAreaController zoneAreaController;
        public ZoneTowerController zoneTowerController;
        public BoxCollider2D zoneCollider;
        public ContactFilter2D contactFilter2D;

        public List<Collider2D> collidersInZone;
        public List<int> mobsInZone;

        private void Awake()
        {
            collidersInZone = new List<Collider2D>();
        }

        public void ChangeZoneVisuals(ZoneTokenDataScriptableObject zoneTokenData)
        {
            var zoneTowerTexture = zoneTokenData.resourcesScriptableObject.mainSprite;
            var zoneTowerName = zoneTokenData.resourcesScriptableObject.label;

            zoneTowerController.SetSprite(zoneTowerTexture);
            zoneTowerController.SetName(zoneTowerName);
            zoneAreaController.UpdateVisuals(zoneTokenData.zoneResourcesScriptableObject);
        }

        public void CheckColliders()
        {
            var count = zoneCollider.OverlapCollider(contactFilter2D, collidersInZone);
            mobsInZone.Clear();
            for (var i = 0; i < count; i++)
            {
                var mob = collidersInZone[i].GetComponent<MobSimpleController>();
                if (mob == null) continue;

                mobsInZone.Add(mob.MobId);
            }
        }

        public void MarkAsSelected()
        {
            zoneTowerController.SetHighlight(true);
        }

        public void MarkAsUnselected()
        {
            zoneTowerController.SetHighlight(false);
        }
    }
}