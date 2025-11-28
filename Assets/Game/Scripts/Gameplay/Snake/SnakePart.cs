using TMPro;
using UnityEngine;

public class SnakePart : MonoBehaviour
{
    [SerializeField] private float life;
    [SerializeField] private TextMeshPro txtLife;

    private void TakeDamage(float damage)
    {
        life--;
        txtLife.text = life.ToString();
    }
}
