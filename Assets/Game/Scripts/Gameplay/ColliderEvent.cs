using UnityEngine;

public delegate void ColliderEventHandler(ColliderEvent sender,Collider2D collision);

public class ColliderEvent : MonoBehaviour
{
    public event ColliderEventHandler triggerEnter;
    public event ColliderEventHandler triggerExit;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        triggerEnter?.Invoke(this, collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        triggerExit?.Invoke(this, collision);
    }
}
