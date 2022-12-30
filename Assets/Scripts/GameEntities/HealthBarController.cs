using UnityEngine;

namespace GameEntities
{
    public class HealthBarController : MonoBehaviour
    {
        [SerializeField] private Transform healthBar;

        public void SetPercentage(float percentage)
        {
            var originalScale = healthBar.localScale;
            originalScale.x = Mathf.Clamp01(percentage);
            healthBar.localScale = originalScale;
        }
    }
}