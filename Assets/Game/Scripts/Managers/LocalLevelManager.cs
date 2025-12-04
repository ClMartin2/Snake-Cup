using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalLevelManager : MonoBehaviour
{
    [SerializeField] private Cup cup;
    [SerializeField] private List<Multiplier> multiplier = new();
    [SerializeField] private SnakeManager snakeManager;

    private GameManager gameManager;

    private void Awake()
    {
        cup.CreateBall += Cup_CreateBall;

        foreach (Multiplier multiplier in multiplier)
        {
            multiplier.CreateBallMultiplier += Multiplier_CreateBallMultiplier;
        }

        snakeManager.Win += SnakeManager_Win;
        snakeManager.Loose += SnakeManager_Loose;
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
    }

    private void SnakeManager_Loose(SnakeManager sender)
    {
        gameManager.Loose();

        snakeManager.Pause(true);
        snakeManager.Restart();

        cup.Pause(true);
        cup.Restart();
    }

    private void SnakeManager_Win(SnakeManager sender)
    {
        gameManager.Win();
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
