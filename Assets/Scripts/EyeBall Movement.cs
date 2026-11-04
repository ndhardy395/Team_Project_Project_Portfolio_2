using UnityEngine;

public class EyeBallMovement : MonoBehaviour
{
    public float hoverHeight = 0.05f;
    public float hoverSpeed = 1.5f;
    public float tiltAngle = 3f;
    public float tiltSpeed = 1f;
    private Vector3 startingPosition;
    private Quaternion startingRotation;



    void Start()
    {
        startingPosition = transform.position;
        startingRotation = transform.rotation;
    }


    void Update()
    {
        float hover = Mathf.Sin(Time.time * hoverHeight) * hoverHeight;
        float tilt = Mathf.Sin(Time.time * tiltSpeed) * tiltAngle;

        // animate the child while the parent follows the path

        transform.localPosition = startingPosition + Vector3.up * hover;
        transform.localRotation = startingRotation * Quaternion.Euler(tilt, 0f, 0f);
    }
}
