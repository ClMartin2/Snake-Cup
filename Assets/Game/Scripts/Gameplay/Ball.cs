using UnityEngine;

public class Ball : MonoBehaviour, IObjectPool
{
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private ColliderEvent colliderEvent;

    public bool hasBeenMultiplied = false;

    private void Awake()
    {
        colliderEvent.triggerEnter += ColliderEvent_triggerEnter;
    }

    private void ColliderEvent_triggerEnter(ColliderEvent sender, Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            ObjectPoolManager.Instance.Release(gameObject);
        }
    }

    public void Release()
    {
        hasBeenMultiplied = false;
    }
}
