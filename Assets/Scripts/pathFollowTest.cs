using UnityEngine;

//this script is meant to take an array of waypoints and make an object follow a path based on this. THIS is the meat and bones
//of a tower defense game. If this works, then it allows for SO much more, like cuvres, multiple paths, etc.

public class PathFollowTest : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 5f;
    public float facingOffset = 180f;
    private int currentWaypointIndex = 0; //each access level for a variable is marked as such
    private bool hasReachedEnd = false;

    private Animator animator;

    

    void Start()
    {
        // this is the animation for enemys stop walking when at last waypoint
        animator = GetComponent<Animator>();

        if(animator != null)
        {
            animator.SetBool("IsMoving", true);
        }
        if (waypoints != null && waypoints.Length > 0 && waypoints[0] != null)
        {
            transform.position = waypoints[0].position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (hasReachedEnd || waypoints == null || waypoints.Length == 0)
        {
            return;
        }
        //moves sphere to current (next) waypoint
        Transform target = waypoints[currentWaypointIndex];

        if (target == null)
        {
            return;
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if(direction.sqrMagnitude > 0.001f) // this make it so the enemy can turn at diffrent waypoints
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, facingOffset, 0f);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360f * Time.deltaTime);
        }


        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f) // if waypoint is reached
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)//check for last waypoint
            {
                Debug.Log("Reached final waypoint. Animator found" + (animator != null));
                hasReachedEnd = true;

                if(animator != null)
                {
                    animator.SetBool("IsMoving", false);
                }
            }
        }
    }
}