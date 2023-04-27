using Economy;
using UnityEngine;

namespace Mobs
{
    [CreateAssetMenu(fileName = "Mob Data", menuName = "Mob/Create new", order = 0)]
    public class MobDataScriptableObject : ScriptableObject
    {
        public BaseStats baseStats;
        public BountyDataScriptableObject bountyDataScriptableObject;
        public MobResourcesScriptableObject mobResourcesScriptableObject;
    }
}