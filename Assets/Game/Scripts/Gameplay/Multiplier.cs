using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Multiplier : MonoBehaviour
{
    [SerializeField] private TextMeshPro txtMultiplier;
    [SerializeField] private float multipler;
    [SerializeField] private ColliderEvent colliderEvent;
    [SerializeField] private string keyBall = "Ball";

    private void Awake()
    {
        txtMultiplier.text = multipler.ToString();
        colliderEvent.triggerEnter += ColliderEvent_triggerEnter;
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

            transformBall.GetComponent<Ball>().hasBeenMultiplied = true;
            transformBall.position = transform.position;
        }
    }
}
