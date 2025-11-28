using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Vector2 limit = new Vector2(2, 2);

    [SerializeField] private Cup cup;
    [SerializeField] private List<Multiplier> multiplier = new();
    [SerializeField] private SnakeManager snakeManager;

    public static Vector2 Limit;
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance != null)
            Instance = this;
        else
            Destroy(gameObject);

        Limit = limit;
        cup.CreateBall += Cup_CreateBall;
        
        foreach (Multiplier multiplier in multiplier)
        {
            multiplier.CreateBallMultiplier += Multiplier_CreateBallMultiplier;
        }
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
