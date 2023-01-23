using System;
using GameEntities;
using Map;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Controllers")]
    [SerializeField] private MapManager mapManager;

    [Header("Game Settings")]
    [SerializeField] private int playerHealth = 100;

    public int PlayerHealth => playerHealth;

    private void Awake()
    {
        mapManager.OnInitUI += Init;
    }

    private void Init()
    {
        mapManager.OnMobReachedFinalRoom += DecreasePlayerHealth;
    }

    private void DecreasePlayerHealth(Mob mob)
    {
        DecreasePlayerHealth(1);
    }

    private void DecreasePlayerHealth(int amount)
    {
        playerHealth -= amount;
        OnPlayerHealthChanged?.Invoke(playerHealth);
        if (playerHealth <= 0)
        {
            OnPlayerLose?.Invoke();
        }
    }

    #region Events

    public event Action<int> OnPlayerHealthChanged;
    public event Action OnPlayerLose;
    public event Action OnPlayerWin;

    #endregion

}