using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class EnemyMovement : MonoBehaviour, IPathProgress
{
    public SplineContainer splineContainer;
    public float speed = 5f;
    public float facingOffset = 180f;

    public BaseHealth baseHealth;
    public int baseDamage = 10;

    private float progress = 0f;
    private float splineLength;
    private bool hasReachedEnd = false;

    public float DistanceRemaining => (1f - progress) * splineLength;

    private Animator animator;

    private void SetMoving(bool isMoving)
    {
        animator = GetComponent<Animator>(); // Ensure animator is caught
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
        }
    }

    void OnDisable()
    {
        SetMoving(false);
    }

    // Called directly by the WaveSpawner upon creation
    public void Initialize(SplineContainer container)
    {
        splineContainer = container;

        if (splineContainer == null)
        {
            SetMoving(false);
            return;
        }

        splineLength = splineContainer.CalculateLength();

        if (splineLength <= 0f)
        {
            SetMoving(false);
            return;
        }

        transform.position = splineContainer.EvaluatePosition(0f);
        SetMoving(true);
    }

    void Start()
    {
        animator = GetComponent<Animator>();

        // Fallback: If placed directly in the scene (not spawned), try using whatever is assigned in the Inspector
        if (splineContainer != null && splineLength <= 0f)
        {
            Initialize(splineContainer);
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
            Quaternion targetRotation = Quaternion.LookRotation(forward) * Quaternion.Euler(0f, facingOffset, 0f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360f * Time.deltaTime);
        }
    }

    void ReachEndPath()
    {
        SetMoving(false);

        // Apply damage before reporting removal so victory checks see updateed health

        if(baseHealth!= null)
        {
            baseHealth.TakeDamage(baseDamage);
        }
        else
        {
            Debug.LogError("Enemy has no BaseHealth reference.", this);
        }
        GetComponent<WaveEnemyTracker>()?.ReportRemoved();

        Destroy(gameObject);
    }
}