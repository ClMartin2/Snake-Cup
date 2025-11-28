using UnityEngine;

public delegate void ValidateBall(Ball sender);

public class Ball : MonoBehaviour, IObjectPool
{
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private ColliderEvent colliderEvent;
    [field: SerializeField] public int damage { get; private set; } = 1;

    public bool hasBeenMultiplied = false;
    public event ValidateBall OnValidateBall;

    private void Awake()
    {
        colliderEvent.triggerEnter += ColliderEvent_triggerEnter;
    }

    private void ColliderEvent_triggerEnter(ColliderEvent sender, Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            ValidateBall();
        }
    }

    private void ValidateBall()
    {
        OnValidateBall?.Invoke(this);   
        ObjectPoolManager.Instance.Release(gameObject);
    }

    public void Release()
    {
        hasBeenMultiplied = false;
    }
}
