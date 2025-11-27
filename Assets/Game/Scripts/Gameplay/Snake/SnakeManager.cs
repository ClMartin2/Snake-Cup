using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.U2D;

public class SnakeManager : MonoBehaviour
{
    [Header("Segments & movement")]
    [SerializeField] private float distanceBetween = 0.2f;        // distance souhaitée entre segments (units)
    [SerializeField] private List<GameObject> bodyPartPrefabs = new(); // prefabs des segments (index 0 = tête)
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private float speed = 1f;                   // vitesse de paramètre du spline (ajuste si nécessaire)

    [Header("Path recording")]
    [SerializeField] private float minRecordDistance = 0.01f;    // distance minimale entre deux enregistrements
    [SerializeField] private float maxHistoryLength = 200f;      // longueur maxi en unités pour l'historique (pour la mémoire)

    private List<Transform> segments = new();
    private List<Vector3> pathPositions = new();     // positions historiques (de la plus ancienne à la plus récente)
    private List<float> pathCumulative = new();      // distances cumulées correspondantes (0..total)
    private float progressRatio = 0f;

    private void Start()
    {
        // Instancie les segments (enfant du SnakeManager)
        for (int i = 0; i < bodyPartPrefabs.Count; i++)
        {
            Transform t = Instantiate(bodyPartPrefabs[i], transform.position, Quaternion.identity, transform).transform;
            // place initialement en ligne sur l'axe X (ou comme tu veux)
            t.position = transform.position - transform.right * i * distanceBetween;
            segments.Add(t);
        }

        // initialise l'historique avec la position initiale de la tête
        pathPositions.Clear();
        pathCumulative.Clear();
        Vector3 headPos = segments[0].position;
        pathPositions.Add(headPos);
        pathCumulative.Add(0f);
    }

    private void Update()
    {
        MoveHeadAlongSpline();
        RecordHeadPosition();
        UpdateSegmentsPositions();
        TrimHistoryIfNeeded();
    }

    private void MoveHeadAlongSpline()
    {
        // Avance le paramètre (attention : Explore ta spline API pour ajuster si ton spline attend 0..1)
        progressRatio += speed * Time.deltaTime;

        Vector3 headPos = splineContainer.EvaluatePosition(progressRatio);
        segments[0].position = headPos;

        // calcul de la rotation de la tête (2D)
        Vector3 tangent = splineContainer.EvaluateTangent(splineContainer.Spline, progressRatio);
        float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
        segments[0].rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void RecordHeadPosition()
    {
        Vector3 headPos = segments[0].position;

        // n'enregistre que si on a bougé suffisamment
        if ((headPos - pathPositions[pathPositions.Count - 1]).sqrMagnitude < (minRecordDistance * minRecordDistance))
            return;

        // nouvelle position la plus récente
        pathPositions.Add(headPos);

        // met à jour les distances cumulées
        float lastCum = pathCumulative[pathCumulative.Count - 1];
        float added = Vector3.Distance(pathPositions[pathPositions.Count - 2], headPos);
        pathCumulative.Add(lastCum + added);
    }

    private void UpdateSegmentsPositions()
    {
        if (pathPositions.Count < 2)
            return;

        float totalDistance = pathCumulative[pathCumulative.Count - 1];

        // pour chaque segment (sauf la tête index 0), calcule sa position souhaitée à distance = i * distanceBetween
        for (int i = 1; i < segments.Count; i++)
        {
            float desiredDistanceBehindHead = i * distanceBetween;

            // si l'historique ne contient pas autant de distance, place le segment à l'extrémité la plus ancienne
            if (desiredDistanceBehindHead > totalDistance)
            {
                // placer au point le plus ancien enregistré
                segments[i].position = pathPositions[0];
                // oriente vers le point suivant si possible
                if (pathPositions.Count > 1)
                {
                    Vector3 dir = (pathPositions[Mathf.Min(1, pathPositions.Count - 1)] - pathPositions[0]).normalized;
                    float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    segments[i].rotation = Quaternion.Euler(0f, 0f, ang);
                }
                continue;
            }

            // on veut la position le long du path telle que distanceFromStart = totalDistance - desiredDistanceBehindHead
            float targetDistanceFromStart = totalDistance - desiredDistanceBehindHead;

            // recherche l'intervalle [j, j+1] de pathCumulative contenant targetDistanceFromStart
            int j = BinarySearchCumulativeIndex(pathCumulative, targetDistanceFromStart);

            // clamp j
            j = Mathf.Clamp(j, 0, pathPositions.Count - 2);

            float d0 = pathCumulative[j];
            float d1 = pathCumulative[j + 1];
            float t = (d1 - d0) > 0f ? (targetDistanceFromStart - d0) / (d1 - d0) : 0f;

            Vector3 pos = Vector3.Lerp(pathPositions[j], pathPositions[j + 1], t);
            segments[i].position = pos;

            // rotation : regarde un point légèrement en avant sur le path pour obtenir la tangente
            float lookAheadDistance = 0.01f; // petit offset pour la direction
            float targetDistanceAhead = Mathf.Max(0f, targetDistanceFromStart - lookAheadDistance);
            int jAhead = BinarySearchCumulativeIndex(pathCumulative, targetDistanceAhead);
            jAhead = Mathf.Clamp(jAhead, 0, pathPositions.Count - 2);
            float d0a = pathCumulative[jAhead];
            float d1a = pathCumulative[jAhead + 1];
            float ta = (d1a - d0a) > 0f ? (targetDistanceAhead - d0a) / (d1a - d0a) : 0f;
            Vector3 posAhead = Vector3.Lerp(pathPositions[jAhead], pathPositions[jAhead + 1], ta);

            Vector3 dirToAhead = (posAhead - pos);
            if (dirToAhead.sqrMagnitude > 0.000001f)
            {
                float ang = Mathf.Atan2(dirToAhead.y, dirToAhead.x) * Mathf.Rad2Deg;
                segments[i].rotation = Quaternion.Euler(0f, 0f, ang);
            }
        }
    }

    // recherche index j tel que pathCumulative[j] <= target <= pathCumulative[j+1]
    private int BinarySearchCumulativeIndex(List<float> cum, float target)
    {
        int low = 0;
        int high = cum.Count - 1;
        while (low <= high)
        {
            int mid = (low + high) / 2;
            if (cum[mid] == target) return Mathf.Max(0, mid - 1);
            if (cum[mid] < target)
                low = mid + 1;
            else
                high = mid - 1;
        }
        return Mathf.Max(0, low - 1);
    }

    private void TrimHistoryIfNeeded()
    {
        // Supprime les anciens points tant que la longueur cumulée excède maxHistoryLength
        if (pathCumulative.Count < 2) return;

        float total = pathCumulative[pathCumulative.Count - 1];
        while (pathCumulative.Count > 2 && total - pathCumulative[1] > maxHistoryLength)
        {
            // enlever l'élément 0 (le plus ancien)
            pathPositions.RemoveAt(0);
            pathCumulative.RemoveAt(0);

            // réajuster les cumulative pour que le premier soit 0 (optionnel)
            float offset = pathCumulative[0];
            for (int i = 0; i < pathCumulative.Count; i++)
                pathCumulative[i] -= offset;

            total = pathCumulative[pathCumulative.Count - 1];
        }
    }
}
