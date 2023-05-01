using UnityEngine;

namespace Towers
{
    [CreateAssetMenu(fileName = "Projectile Data", menuName = "Tower/Create new projectile", order = 0)]
    public class ProjectileDataScriptableObject : ScriptableObject
    {
        public ProjectileBaseStats baseStats;
        public ProjectileResourcesScriptableObject resources;
    }


}