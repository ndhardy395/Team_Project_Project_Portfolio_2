using UnityEngine;

public class PathFollow : MonoBehaviour
{
    private Transform[] waypoints;
    public float speed = 5f;
    private int currentWaypointIndex = 0;
    private bool hasReachedEnd = false;

    public void SetPath(Transform[] newWaypoints)
    {
        if (newWaypoints == null || newWaypoints.Length == 0) return;
        if (newWaypoints[0] == null) return;

        // Assign the passed waypoints
        waypoints = newWaypoints;

        // Snap enemy to the start point
        transform.position = waypoints[0].position;
    }

    void Update()
    {
        if (hasReachedEnd || waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)
            {
                hasReachedEnd = true;
                // Optional: Destroy(gameObject); when they reach the end
            }
        }
    }
}