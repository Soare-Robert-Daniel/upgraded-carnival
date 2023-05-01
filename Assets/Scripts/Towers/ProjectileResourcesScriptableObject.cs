using UnityEngine;

namespace Towers
{
    [CreateAssetMenu(fileName = "Projectile Resources", menuName = "Tower/Create new projectile resources", order = 0)]
    public class ProjectileResourcesScriptableObject : ScriptableObject
    {
        public string label;
        public string description;

        [Header("Sprites")] public Sprite mainSprite;
    }
}