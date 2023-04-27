using UnityEngine;

namespace Mobs
{
    [CreateAssetMenu(fileName = "Mob Resources", menuName = "Mob/Create new resources", order = 0)]
    public class MobResourcesScriptableObject : ScriptableObject
    {
        public string label;
        public Sprite mainSprite;
    }
}