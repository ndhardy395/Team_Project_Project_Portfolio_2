using UnityEngine;

public class PortalAnimation : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeed = 30f;

    [Header("Pulse")]
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.2f;


    [Header("Opening / Closing")]
    public float animationSpeed = 3f;

    public bool IsOpen => currentSize <= 0.001f;
    public bool IsClosed => currentSize <= 0.999f;

    private Vector3 originalScale;
    private float currentSize = 0f;

    private float targetSize = 0f;


    void Awake()
    {
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }



    void Update()
    {
        // Gradually grow or shrink the portal
        currentSize = Mathf.MoveTowards(currentSize, targetSize, animationSpeed * Time.deltaTime);

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;    //gentle pulsing while portal is open


        transform.localScale = originalScale * currentSize * pulse;

        // rotate around the vertical axis

        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }

    public void OpenPortal()
    {
        targetSize = 1f;
    }

    public void ClosePortal()
    {
        targetSize = 0f;
    }
}
