using System;

namespace Economy
{
    [Serializable]
    public struct Bounty
    {
        public Currency currency;
        public float value;

        public Bounty(Currency currency, float value)
        {
            this.currency = currency;
            this.value = value;
        }

        public Bounty(Bounty bounty)
        {
            currency = bounty.currency;
            value = bounty.value;
        }
    }
}