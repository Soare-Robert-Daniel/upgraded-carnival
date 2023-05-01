using UnityEngine;

namespace Towers
{
    [CreateAssetMenu(fileName = "Tower Data", menuName = "Tower/Create new", order = 0)]
    public class TowerDataScriptableObject : ScriptableObject
    {
        public float attackInterval;
        public TowerResourcesScriptableObject resources;
        public TowerPriceScriptableObject price;
        public ProjectileDataScriptableObject projectileData;
    }
}