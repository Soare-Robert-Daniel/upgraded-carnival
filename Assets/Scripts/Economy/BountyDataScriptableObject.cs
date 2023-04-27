using UnityEngine;

namespace Economy
{
    [CreateAssetMenu(fileName = "Bounty", menuName = "Economy/Create new bounty", order = 0)]
    public class BountyDataScriptableObject : ScriptableObject
    {
        public Currency currency;
        public float value;

        public Bounty ToBounty()
        {
            return new Bounty(currency, value);
        }
    }
}