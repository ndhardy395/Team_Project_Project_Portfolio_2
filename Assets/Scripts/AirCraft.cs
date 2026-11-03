using UnityEngine;

public class AirCraft : MonoBehaviour
{
    public float bobHeight = 0.15f;
    public float bobSpeed = 1.5f;
    public float bankAngle = 3f;
    public float bankSpeed = 1f;
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
        float bank = Mathf.Sin(Time.time * bankSpeed) * bankAngle;

        


        transform.localPosition = startingPosition + Vector3.up * bob;

        // aircraft nose points along the local z axis
        transform.localRotation = startingRotation * Quaternion.Euler(0f, 0f, bank);
    }
}
