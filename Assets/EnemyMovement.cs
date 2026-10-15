using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [HideInInspector] public Transform[] waypoints; //waypoints passed here
    public float speed = 5f;
    private int currentWaypointIndex = 0;
    private bool hasReachedEnd = false;

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
                Destroy(gameObject);
            }
        }
    }
}