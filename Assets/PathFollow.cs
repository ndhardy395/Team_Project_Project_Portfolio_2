using UnityEngine;

//this script is meant to take an array of waypoints and make an object follow a path based on this. THIS is the meat and bones
//of a tower defense game. If this works, then it allows for SO much more, like cuvres, multiple paths, etc.

public class PathFollow : MonoBehaviour
{
    private Transform[] waypoints;
    public float speed = 5f;
    private int currentWaypointIndex = 0; //each access level for a variable is marked as such
    private bool hasReachedEnd = false;
    private bool isPathAssigned = false;

    public void SetPath(Transform[] newWaypoints)
    {
        waypoints = newWaypoints;
        currentWaypointIndex = 0;
        hasReachedEnd = false;
        isPathAssigned = true; //sets up path for enemy, preventing every enemy from needing to have the path assigned

        if (waypoints.Length > 0 && waypoints[0] != null)
        {
            transform.position = waypoints[0].position; //takes in waypoints
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPathAssigned || hasReachedEnd) return; //no waypoints or end? do nothing

        //moves sphere to current (next) waypoint

        Transform target = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f) // if waypoint is reached. Epsilon value because floating point imprecision
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length) //check for last waypoint
                {
                    hasReachedEnd = true;
                    OnPathEnded();
                }
                }
        }

    void OnPathEnded()
    {
        // enemy reached the end, OH NO BRO!
        Destroy(gameObject);
    }
}