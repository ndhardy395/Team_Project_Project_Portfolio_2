using System.Collections;
using UnityEngine;

public class PathFollow : MonoBehaviour
{
<<<<<<< Updated upstream:Assets/Scripts/PathFollow.cs
    private Transform[] waypoints;
    public float speed = 5f;
    private int currentWaypointIndex = 0; //each access level for a variable is marked as such
    private bool hasReachedEnd = false;
    private bool isPathAssigned = false;
=======
    public Transform[] waypoints;
    public int enemyCount = 10;
    public float spawnDelay = 0.5f;
    public GameObject EnemyFab;
>>>>>>> Stashed changes:Assets/PathFollow.cs

    public void SetPath(Transform[] newWaypoints)
    {
<<<<<<< Updated upstream:Assets/Scripts/PathFollow.cs
        waypoints = newWaypoints;
        currentWaypointIndex = 0;
        hasReachedEnd = false;
        isPathAssigned = true; //sets up path for enemy, preventing every enemy from needing to have the path assigned

        if (waypoints.Length > 0 && waypoints[0] != null)
        {
            transform.position = waypoints[0].position; //takes in waypoints
=======
        if (waypoints == null || waypoints.Length == 0) return;
        if (waypoints[0] == null) return;
        if (EnemyFab == null) return;
        //safety checks

        // Move spawner to start point
        transform.position = waypoints[0].position;

        if (enemyCount > 0)
        {
            StartCoroutine(SpawnEnemies());
        }
        else
        {
            return;
>>>>>>> Stashed changes:Assets/PathFollow.cs
        }
    }

    IEnumerator SpawnEnemies()
    {
<<<<<<< Updated upstream:Assets/Scripts/PathFollow.cs
        if (!isPathAssigned || hasReachedEnd) return; //no waypoints or end? do nothing

        //moves sphere to current (next) waypoint

        Transform target = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f) // if waypoint is reached. Epsilon value because floating point imprecision
=======
        for (int i = 0; i < enemyCount; i++)
>>>>>>> Stashed changes:Assets/PathFollow.cs
        {
            GameObject newEnemy = Instantiate(EnemyFab, waypoints[0].position, Quaternion.identity);

<<<<<<< Updated upstream:Assets/Scripts/PathFollow.cs
            if (currentWaypointIndex >= waypoints.Length) //check for last waypoint
                {
                    hasReachedEnd = true;
                    OnPathEnded();
                }
                }
=======
            EnemyMovement movement = newEnemy.GetComponent<EnemyMovement>();
            if (movement != null)
            {
                movement.waypoints = this.waypoints; //feeds waypoints to enemy
            }

            yield return new WaitForSeconds(spawnDelay); //delay, new frame
>>>>>>> Stashed changes:Assets/PathFollow.cs
        }

    void OnPathEnded()
    {
        // enemy reached the end, OH NO BRO!
        Destroy(gameObject);
    }
}