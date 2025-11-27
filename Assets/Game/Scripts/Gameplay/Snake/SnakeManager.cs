using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.U2D;

public class SnakeManager : MonoBehaviour
{
    [SerializeField] private float distanceBetween = 0.2f;
    [SerializeField] private List<GameObject> bodyParts = new();
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private float speed;

    private List<Transform> snakeBody = new();
    private float progressRatio = 0;

    private void Start()
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            Transform bodyPart = Instantiate(bodyParts[i], transform.position, transform.rotation, transform).transform;
            bodyPart.position = transform.position - transform.right * i * distanceBetween;

            bodyPart.GetComponent<MarkerManager>().ClearMarkerList();
            snakeBody.Add(bodyPart);
        }
    }

    private void Update()
    {
        for (int i = 1; i < snakeBody.Count; i++)
        {
            MarkerManager markManager = snakeBody[i - 1].GetComponent<MarkerManager>();
            snakeBody[i].position = markManager.markerList[0].position - transform.right * distanceBetween;
            snakeBody[i].rotation = markManager.markerList[0].rotation;
            markManager.markerList.RemoveAt(0);
        }

        var localPoint = splineContainer.transform.InverseTransformPoint(transform.position);
        Transform snakeHead = snakeBody[0];

        progressRatio += speed * Time.deltaTime;

        Vector3 position = splineContainer.EvaluatePosition(progressRatio);
        snakeHead.position = position;

        Vector3 tangent = splineContainer.EvaluateTangent(splineContainer.Spline, progressRatio);

        tangent = splineContainer.transform.TransformDirection(tangent);
        float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
        snakeHead.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
