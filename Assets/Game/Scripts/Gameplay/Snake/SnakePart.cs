using TMPro;
using UnityEngine;

public delegate void SnakePartEventHandler(SnakePart sender);

public class SnakePart : MonoBehaviour
{
    [SerializeField] private int life = 10;
    [SerializeField] private TextMeshPro txtLife;

    public event SnakePartEventHandler OnDie;

    private void Start()
    {
        txtLife.text = life.ToString();
    }

    public void TakeDamage(int damage)
    {
        life--;
        txtLife.text = life.ToString();

        if (life <= 0)
            Die();
    }

    private void Die()
    {
        OnDie?.Invoke(this);
    }
}
