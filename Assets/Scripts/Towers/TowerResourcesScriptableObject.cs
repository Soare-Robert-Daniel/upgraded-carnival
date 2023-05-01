using UnityEngine;

namespace Towers
{
    [CreateAssetMenu(fileName = "Tower Resources", menuName = "Tower/ Create new resources", order = 0)]
    public class TowerResourcesScriptableObject : ScriptableObject
    {
        public string label;
        public string description;

        [Header("Sprites")]
        public Sprite mainSprite;
    }
}