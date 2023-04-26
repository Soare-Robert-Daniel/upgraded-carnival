using UnityEngine;

namespace Towers.Zones
{
    public class ZoneAreaController : MonoBehaviour
    {

        private static readonly int MainColor = Shader.PropertyToID("_MainColor");
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
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

        public void UpdateVisuals(ZoneResourcesScriptableObject zoneResourcesScriptableObject)
        {
            spriteRenderer.material.SetColor(MainColor, zoneResourcesScriptableObject.mainColor);
            spriteRenderer.material.SetTexture(MainTex, zoneResourcesScriptableObject.mainTexture);
        }
    }
}