using UnityEngine;
using UnityEngine.Splines; // Required for Unity Splines. That's how we're getting smoother curves, AND it makes map design faster and simpler.
using Unity.Mathematics;

public class EnemyMovement : MonoBehaviour, IPathProgress
{
    public SplineContainer splineContainer; // Replaces the waypoints array, and instead uses the spline
    public float speed = 5f;

    public float facingOffset = 180f; // Direction were the enemy face (new)
    private float progress = 0f; // Goes from 0.0 (start) to 1.0 (end). Fractional now.
    private float splineLength;
    private bool hasReachedEnd = false;

    public float DistanceRemaining => (1f - progress) * splineLength;//distance left for tower targeting

    private Animator animator;


    private void SetMoving(bool isMoving) // this is for the enemy movement animation.
    {
        if(animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
        }
    }

     void OnDisable()
    {
        SetMoving(false);
    }

    void Start() // this places enemy at spline start and turn on its walking animation
    {
        animator = GetComponent<Animator>();

        if(splineContainer == null)
        {
            SetMoving(false);
            return;
        }

        splineLength = splineContainer.CalculateLength();

        if(splineLength <= 0f)
        {
            SetMoving(false);
            return;
        }

        transform.position = splineContainer.EvaluatePosition(0f);
        SetMoving(true);
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
        if (forward != Vector3.zero) // this add enmy facing adjustment and make it turn smoothly
        {
            Quaternion targetRotation = Quaternion.LookRotation(forward) * Quaternion.Euler(0f, facingOffset, 0f);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,360f * Time.deltaTime);
        }
    }

    void ReachEndPath()
    {
        SetMoving(false);
    Destroy(gameObject);
    }
}