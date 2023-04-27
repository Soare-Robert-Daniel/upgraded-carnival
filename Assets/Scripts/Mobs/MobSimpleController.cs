using GameEntities;
using UnityEngine;

namespace Mobs
{
    public class MobSimpleController : MonoBehaviour
    {
        [SerializeField] private int mobId;

        [SerializeField] private HealthBarController healthBarController;
        [SerializeField] private SpriteRenderer bodySpriteRenderer;

        public int MobId => mobId;

        public void SetMobId(int id)
        {
            mobId = id;
        }


        public void UpdateHealthBar(float healthPercentage)
        {
            healthBarController.SetPercentage(healthPercentage);
        }

        public void UpdateBodySprite(Sprite sprite)
        {
            bodySpriteRenderer.sprite = sprite;
        }

        public void UpdateVisuals(MobResourcesScriptableObject mobResourcesScriptableObject)
        {
            UpdateBodySprite(mobResourcesScriptableObject.mainSprite);
        }
    }
}