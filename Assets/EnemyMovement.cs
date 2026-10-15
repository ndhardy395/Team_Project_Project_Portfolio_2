using UnityEngine;
using UnityEngine.Splines; // Required for Unity Splines. That's how we're getting smoother curves, AND it makes map design faster and simpler.
using Unity.Mathematics;

public class EnemyMovement : MonoBehaviour
{
    [HideInInspector] public SplineContainer splineContainer; // Replaces the waypoints array, and instead uses the spline
    public float speed = 5f;
    private float progress = 0f; // Goes from 0.0 (start) to 1.0 (end). Fractional now.
    private float splineLength;
    private bool hasReachedEnd = false;

    void Start()
    {
        if (splineContainer != null)
        {
            // Cache total length so movement speed is consistent regardless of path length, as intended by the enemy's speed value
            splineLength = splineContainer.CalculateLength();
        }
    }
    void Update()
    {
        if (hasReachedEnd || splineContainer == null) return;
        if (splineLength <= 0f) return;

        progress += (speed / splineLength) * Time.deltaTime;

        if (progress >= 1f)
        {
            progress = 1f;
            hasReachedEnd = true;
            ReachEndPath();
            return;
        }

        Vector3 worldPosition = splineContainer.EvaluatePosition(progress);
        transform.position = worldPosition;

        Vector3 forward = splineContainer.EvaluateTangent(progress);
        if (forward != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(forward);
        }
    }

    void ReachEndPath()
    {
    Destroy(gameObject);
    }
}