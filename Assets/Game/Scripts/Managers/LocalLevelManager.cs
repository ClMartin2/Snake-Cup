using System;
using System.Collections.Generic;
using UnityEngine;

public delegate void LocalLevelManagerEventHandler(LocalLevelManager sender);

public class LocalLevelManager : MonoBehaviour
{
    [field: SerializeField] public Cup cup { get; private set; }
    [field: SerializeField] public List<Multiplier> multipliers { get; private set; } = new();
    [SerializeField] private SnakeManager snakeManager;

    public static LocalLevelManager Instance;

    public event LocalLevelManagerEventHandler loose;
    public event LocalLevelManagerEventHandler win;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(Instance.gameObject);
            Instance = this;
        }

        cup.CreateBall += Cup_CreateBall;

        foreach (Multiplier multiplier in multipliers)
        {
            multiplier.CreateBallMultiplier += Multiplier_CreateBallMultiplier;
        }

        snakeManager.Win += SnakeManager_Win;
        snakeManager.Loose += SnakeManager_Loose;
    }

    public void Play()
    {
        snakeManager.Pause(false);
        snakeManager.Restart();

        cup.Pause(false);
        cup.Restart();
    }

    private void SnakeManager_Loose(SnakeManager sender)
    {
        loose?.Invoke(this);

        snakeManager.Pause(true);
        snakeManager.Restart();

        cup.Pause(true);
        cup.Restart();
    }

    private void SnakeManager_Win(SnakeManager sender)
    {
        win?.Invoke(this);
    }

    private void Multiplier_CreateBallMultiplier(Multiplier sender, Ball ball)
    {
        ball.OnValidateBall += Ball_OnValidateBall;
    }

    private void Cup_CreateBall(Cup sender, Ball ball)
    {
        ball.OnValidateBall += Ball_OnValidateBall;
    }

    private void Ball_OnValidateBall(Ball sender)
    {
        snakeManager.TakeDamage(sender.damage);
    }
}
