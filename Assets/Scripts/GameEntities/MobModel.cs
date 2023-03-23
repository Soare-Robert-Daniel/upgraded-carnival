using Economy;
using UnityEngine;

namespace GameEntities
{
    [CreateAssetMenu(fileName = "MobModel", menuName = "Mob/Create Mob Model", order = 0)]
    public class MobModel : ScriptableObject
    {
        public MobStats stats;
        public Bounty bounty;
        public EntityAssets assets;
    }
}