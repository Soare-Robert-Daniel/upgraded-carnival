using UnityEngine;

namespace Towers.Zones
{
    public class ZoneTowerController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void SetColor(Color color)
        {
            spriteRenderer.color = color;
        }

        public void SetAlpha(float alpha)
        {
            var color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }

        public void SetSprite(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
        }
    }
}