using Map;
using UnityEngine;

namespace GameEntities
{
    public class Mob : MonoBehaviour
    {
        public int id;
        public Vector3 target;

        [Header("Components")]
        [SerializeField] private MapManager manager;

        [SerializeField] private HealthBarController healthBarController;
        [SerializeField] private SpriteRenderer bodySpriteRenderer;

        #region Room Effects

        public void UpdateHealthBar(float healthPercentage)
        {
            healthBarController.SetPercentage(healthPercentage);
        }

        public void UpdateBodySprite(Sprite sprite)
        {
            bodySpriteRenderer.sprite = sprite;
        }

        public MapManager Manager
        {
            get => manager;
            set => manager = value;
        }

        #endregion

    }
}