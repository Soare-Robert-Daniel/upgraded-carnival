using System;
using UnityEngine;

namespace Map.Components
{
    [Serializable]
    public class Wave
    {
        [SerializeField] private int number;
        [SerializeField] private float timeInterval;
        [SerializeField] private float currentTime;

        public float Time => currentTime;
        public float TimeInterval => timeInterval;
        public int Number => number;

        public event Action<int> OnNewWaveStart;

        public void UpdateTimeInterval(float time)
        {
            currentTime += time;

            if (currentTime > timeInterval)
            {
                currentTime = 0;
                number += 1;
                OnNewWaveStart?.Invoke(number);
            }
        }
    }
}