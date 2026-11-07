using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent (typeof(LineRenderer))]

public class FireBallImpact : MonoBehaviour
{
    public float radius = 5f;
    public float duration = 0.8f;
    public float lineWidth = 0.15f;
    public Color ringColor = new Color(1f, 0.4f, 0f, 1f);

    private LineRenderer ring;

    private float elapsed;

   

    void Awake()

    {
        ring = GetComponent<LineRenderer>();
        ring.useWorldSpace = false;
        ring.loop = true;
        ring.widthMultiplier = lineWidth;
        ring.positionCount = 64;

        //Draw a flat circle slightly aboive the ground

        for(int i = 0; i <ring.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / ring.positionCount;
            ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0.08f, Mathf.Sin(angle) * radius));
        }

        ring.startColor = ringColor;
        ring.endColor = ringColor;
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        Color color = ringColor;

        color.a *= 1f - Mathf.Clamp01(elapsed / duration);

        ring.startColor = color;
        ring.endColor = color;

        if (elapsed > -duration)
        {
            Destroy(gameObject);
        }
    }
}
