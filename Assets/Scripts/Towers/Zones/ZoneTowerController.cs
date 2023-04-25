using TMPro;
using UnityEngine;

namespace Towers.Zones
{
    public class ZoneTowerController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TextMeshPro towerName;
        [SerializeField] private GameObject highlight;

        private void Awake()
        {
            SetHighlight(false);
        }

        public void SetSprite(Sprite sprite)
        {
            spriteRenderer.sprite = sprite;
        }

        public void SetName(string label)
        {
            towerName.text = label;
        }

        public void SetHighlight(bool value)
        {
            highlight.SetActive(value);
        }
    }
}