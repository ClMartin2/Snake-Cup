using UnityEngine;

public class MovingMultiplier : Multiplier
{
    [SerializeField] private float timeToMove;

    private int direction = -1;
    private float delta;
    private Vector3 rightEndPosition;
    private Vector3 leftEndPosition;
    private Vector3 startPosition;
    private Vector3 endPosition;

    private void Start()
    {
        Vector3 startPositionPlatform =  new Vector3(0, transform.position.y);

        leftEndPosition = startPositionPlatform + transform.right * GameManager.Limit.x;
        rightEndPosition = startPositionPlatform + transform.right * GameManager.Limit.y;

        startPosition = leftEndPosition;
        transform.position = startPosition;
        endPosition = rightEndPosition;
    }

    private void Update()
    {
        delta += Time.deltaTime;
        float alpha = delta / timeToMove;

        if (alpha >= 1)
        {
            direction *= -1;
            endPosition = startPosition;
            startPosition = direction > 0 ? rightEndPosition : leftEndPosition;
            delta = 0;
            alpha = 0;
        }

        transform.position = Vector3.Lerp(startPosition, endPosition, alpha);

    }
}
