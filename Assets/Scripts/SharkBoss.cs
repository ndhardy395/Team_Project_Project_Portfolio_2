using UnityEngine;

public class SharkBoss : MonoBehaviour
{
    public float bobHeight = 0.15f;
    public float bobSpeed = 2f;
    public float swayAngle = 5f;
    public float swaySpeed = 3f;
    private Vector3 startingPosition;
    private Quaternion startingRotation;



    void Start()
    {
        startingPosition = transform.position;
        startingRotation = transform.rotation;
    }


    void Update()
    {
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        float sway = Mathf.Sin(Time.time * swaySpeed) * swayAngle;

        // animate the child while the parent follows the path

        transform.localPosition = startingPosition + Vector3.up * bob;
        transform.localRotation = startingRotation * Quaternion.Euler(0f, sway, 0f);
    }
}
