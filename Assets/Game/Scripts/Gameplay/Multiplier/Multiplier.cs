using TMPro;
using UnityEngine;

public delegate void MultiplierEventHandler(Multiplier sender, Ball ball);

public class Multiplier : MonoBehaviour
{
    [field: SerializeField] private int multipler;

    [SerializeField] private TextMeshPro txtMultiplier;
    [SerializeField] private ColliderEvent colliderEvent;
    [SerializeField] private string keyBall = "Ball";

    public event MultiplierEventHandler CreateBallMultiplier;

    public int startMultiplier { get; private set; } = 0;

    private void Awake()
    {
        startMultiplier = multipler;
        UpdateMultiplier(multipler);
        colliderEvent.triggerEnter += ColliderEvent_triggerEnter;
    }

    private void OnValidate()
    {
        UpdateMultiplier(multipler);
    }

    public void UpdateMultiplier(int multiplier)
    {
        multipler = multiplier;
        txtMultiplier.text = multipler.ToString();
    }

    private void ColliderEvent_triggerEnter(ColliderEvent sender, Collider2D collision)
    {
        Ball ball;

        if(collision.TryGetComponent(out ball))
        {
            if (ball.hasBeenMultiplied || Vector3.Dot(Vector3.up,(ball.transform.position - transform.position).normalized) < 0)
                return;
        }

        for (int i = 0; i < multipler - 1; i++)
        {
            Transform transformBall = ObjectPoolManager.Instance.Get(keyBall).transform;
            Ball _ball = transformBall.GetComponent<Ball>();

            _ball.hasBeenMultiplied = true;
            transformBall.position = ball.transform.position;

            CreateBallMultiplier?.Invoke(this, _ball);  
        }
    }
}
