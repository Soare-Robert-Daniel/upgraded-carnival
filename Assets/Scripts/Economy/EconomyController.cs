using System;
using UnityEngine;

namespace Economy
{
    public enum Currency
    {
        Gold
    }

    public class EconomyController : MonoBehaviour
    {
        [SerializeField] private float currentGold;

        public Action<float> OnCurrentGoldChanged;

        public float CurrentGold => currentGold;

        public void AddCurrency(Currency currency, float value)
        {
            if (currency == Currency.Gold)
            {
                AddGold(value);
            }
        }

        public void SpendCurrency(Currency currency, float value)
        {
            AddCurrency(currency, -value);
        }

        private void AddGold(float value)
        {
            currentGold += value;

            OnCurrentGoldChanged?.Invoke(currentGold);
        }

        public bool CanSpend(Currency currency, float valueToSpend)
        {
            if (currency == Currency.Gold)
            {
                return currentGold - valueToSpend >= 0f;
            }

            return false;
        }
    }
}