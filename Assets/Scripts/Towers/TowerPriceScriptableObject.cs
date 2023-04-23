using Economy;
using UnityEngine;

namespace Towers
{
    [CreateAssetMenu(fileName = "Tower Price", menuName = "Tower/Create new price", order = 0)]
    public class TowerPriceScriptableObject : ScriptableObject
    {
        public ResourceScriptableObject resource;
        public float value;
    }
}