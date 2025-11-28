using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SnakeManager : MonoBehaviour
{
    [Header("Segments & movement")]
    [SerializeField] private float distanceBetween = 0.2f;
    [SerializeField] private List<GameObject> bodyPartPrefabs = new();
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private float speed = 1f; // vitesse du paramètre spline

    [Header("Path recording")]
    [SerializeField] private float minRecordDistance = 0.01f;
    [SerializeField] private float maxHistoryLength = 200f;

    [Header("Follow behaviour")]
    [SerializeField] private float moveThreshold = 0.01f; // distance minimale avant qu'un segment se déplace
    [SerializeField] private float followSpeed = 5f;      // vitesse de déplacement interne des segments (units/s)
    [SerializeField] private float rotationSmooth = 8f;   // lissage rotation (plus grand = plus rapide)
    [SerializeField] private float rotationApplyThreshold = 0.001f; // dir threshold pour appliquer la rotation

    private List<Transform> segments = new();
    private List<Vector3> pathPositions = new(); // ancienne -> récente
    private List<float> pathCumulative = new();
    private float progressRatio = 0f;
    private List<SnakePart> snakeParts = new();

    private void Start()
    {
        // Instanciation des segments
        for (int i = 0; i < bodyPartPrefabs.Count; i++)
        {
            Transform t = Instantiate(bodyPartPrefabs[i], transform.position, Quaternion.identity, transform).transform;
            segments.Add(t);

            SnakePart snakePart;

            if (t.TryGetComponent(out snakePart))
            {
                snakeParts.Add(snakePart);
                snakePart.OnDie += SnakePart_OnDie;
            }
        }

        // Place les segments immédiatement à droite de la tête (sans attendre que la tête bouge)
        Vector3 headStart = segments[0].position = splineContainer != null ? splineContainer.EvaluatePosition(progressRatio) : transform.position;
        for (int i = 0; i < segments.Count; i++)
        {
            Vector3 target = headStart - transform.right * i * distanceBetween;
            segments[i].position = target;
            // orientation initiale : orienter vers la droite
            float ang = Mathf.Atan2(transform.right.y, transform.right.x) * Mathf.Rad2Deg;
            segments[i].rotation = Quaternion.Euler(0f, 0f, ang);
        }

        // Pré-remplir l'historique avec ces positions (de l'ancienne vers la plus récente)
        pathPositions.Clear();
        pathCumulative.Clear();

        for (int i = segments.Count - 1; i >= 0; i--)
        {
            pathPositions.Add(segments[i].position);
        }

        pathCumulative.Add(0f);
        for (int i = 1; i < pathPositions.Count; i++)
        {
            float d = Vector3.Distance(pathPositions[i - 1], pathPositions[i]);
            pathCumulative.Add(pathCumulative[pathCumulative.Count - 1] + d);
        }
    }

    private void SnakePart_OnDie(SnakePart sender)
    {
        segments.Remove(segments.Find(x => x == sender.transform));
        snakeParts.Remove(sender);
        Destroy(sender.gameObject);
    }

    private void Update()
    {
        MoveHeadAlongSpline();
        RecordHeadPosition();
        UpdateSegmentsPositions();
        TrimHistoryIfNeeded();
    }

    public void TakeDamage(int Damage)
    {
        snakeParts[0].TakeDamage(Damage);
    }

    private void MoveHeadAlongSpline()
    {
        if (splineContainer == null || segments.Count == 0) return;

        progressRatio += speed * Time.deltaTime;

        Vector3 headPos = splineContainer.EvaluatePosition(progressRatio);
        segments[0].position = headPos;

        Vector3 tangent = splineContainer.EvaluateTangent(splineContainer.Spline, progressRatio);
        float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
        segments[0].rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void RecordHeadPosition()
    {
        if (segments.Count == 0) return;

        Vector3 headPos = segments[0].position;
        if (pathPositions.Count == 0)
        {
            pathPositions.Add(headPos);
            pathCumulative.Add(0f);
            return;
        }

        if ((headPos - pathPositions[pathPositions.Count - 1]).sqrMagnitude < (minRecordDistance * minRecordDistance))
            return;

        pathPositions.Add(headPos);
        float lastCum = pathCumulative[pathCumulative.Count - 1];
        float added = Vector3.Distance(pathPositions[pathPositions.Count - 2], headPos);
        pathCumulative.Add(lastCum + added);
    }

    private void UpdateSegmentsPositions()
    {
        if (pathPositions.Count < 2) return;

        float totalDistance = pathCumulative[pathCumulative.Count - 1];

        // pour chaque segment (sauf tête)
        for (int i = 1; i < segments.Count; i++)
        {
            float desiredDistanceBehindHead = i * distanceBetween;

            Vector3 desiredPos;
            float targetDistanceFromStart = 0f;
            bool hasValidTarget = true;

            if (desiredDistanceBehindHead > totalDistance)
            {
                desiredPos = pathPositions[0];
                hasValidTarget = false;
            }
            else
            {
                targetDistanceFromStart = totalDistance - desiredDistanceBehindHead;
                int j = BinarySearchCumulativeIndex(pathCumulative, targetDistanceFromStart);
                j = Mathf.Clamp(j, 0, pathPositions.Count - 2);

                float d0 = pathCumulative[j];
                float d1 = pathCumulative[j + 1];
                float t = (d1 - d0) > 0f ? (targetDistanceFromStart - d0) / (d1 - d0) : 0f;
                desiredPos = Vector3.Lerp(pathPositions[j], pathPositions[j + 1], t);
            }

            // Déplacement organique : ne bouge que si nécessaire
            float dist = Vector3.Distance(segments[i].position, desiredPos);
            if (dist > moveThreshold)
            {
                float step = followSpeed * Time.deltaTime;
                segments[i].position = Vector3.MoveTowards(segments[i].position, desiredPos, step);
            }

            // --- ROTATION CORRECTE : calcule une position "ahead" relative au desiredPos (pour tangente) ---
            Vector3 dirToUse = Vector3.zero;
            if (hasValidTarget)
            {
                // lookAheadDistance en unités (on prend un petit fraction de distanceBetween)
                float lookAheadDistance = Mathf.Max(0.01f, distanceBetween * 0.25f);

                float targetDistanceAhead = Mathf.Max(0f, targetDistanceFromStart - lookAheadDistance);
                int jAhead = BinarySearchCumulativeIndex(pathCumulative, targetDistanceAhead);
                jAhead = Mathf.Clamp(jAhead, 0, Mathf.Max(0, pathPositions.Count - 2));

                float d0a = pathCumulative[jAhead];
                float d1a = pathCumulative[jAhead + 1];
                float ta = (d1a - d0a) > 0f ? (targetDistanceAhead - d0a) / (d1a - d0a) : 0f;
                Vector3 posAhead = Vector3.Lerp(pathPositions[jAhead], pathPositions[jAhead + 1], ta);

                dirToUse = (posAhead - desiredPos).normalized;
            }
            else
            {
                // fallback : dir vers la tête (mais on lisse plus fortement)
                dirToUse = (segments[0].position - segments[i].position).normalized;
            }

            // n'applique la rotation que si la direction est significative
            if (dirToUse.sqrMagnitude > rotationApplyThreshold)
            {
                float targetAngle = Mathf.Atan2(dirToUse.y, dirToUse.x) * Mathf.Rad2Deg;
                Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngle);
                segments[i].rotation = Quaternion.Slerp(segments[i].rotation, targetRot, Mathf.Clamp01(rotationSmooth * Time.deltaTime));
            }
        }
    }

    private int BinarySearchCumulativeIndex(List<float> cum, float target)
    {
        int low = 0;
        int high = cum.Count - 1;
        while (low <= high)
        {
            int mid = (low + high) / 2;
            if (cum[mid] == target) return Mathf.Max(0, mid - 1);
            if (cum[mid] < target) low = mid + 1;
            else high = mid - 1;
        }
        return Mathf.Max(0, low - 1);
    }

    private void TrimHistoryIfNeeded()
    {
        if (pathCumulative.Count < 2) return;
        float total = pathCumulative[pathCumulative.Count - 1];
        while (pathCumulative.Count > 2 && total - pathCumulative[1] > maxHistoryLength)
        {
            pathPositions.RemoveAt(0);
            pathCumulative.RemoveAt(0);
            float offset = pathCumulative[0];
            for (int i = 0; i < pathCumulative.Count; i++) pathCumulative[i] -= offset;
            total = pathCumulative[pathCumulative.Count - 1];
        }
    }
}
