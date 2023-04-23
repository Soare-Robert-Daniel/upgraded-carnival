using UnityEngine;

namespace Economy
{

    [CreateAssetMenu(fileName = "Economy", menuName = "Economy/Create new resource", order = 0)]
    public class ResourceScriptableObject : ScriptableObject
    {
        public string label;
        public Currency currency;
    }
}