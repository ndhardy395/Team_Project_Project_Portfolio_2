using UnityEngine;

public class LightningBoltEffect : MonoBehaviour
{
    public Material boltMaterial;

    public float boltHeight = 12f;
    public float boltWidth = 0.15f;
    public float spread = 0.7f;
    public float lifetime = 0.3f;
    public float flickeringInterval = 0.04f;
    public int segments = 12;

    private LineRenderer bolt;
    private float flickeringRemaning;


    void Start()
    {
        bolt = gameObject.AddComponent<LineRenderer>();

        bolt.useWorldSpace = false;
        bolt.positionCount = Mathf.Max(3, segments);
        bolt.sharedMaterial = boltMaterial;

        bolt.startWidth = boltWidth;
        bolt.endWidth = boltWidth * 0.4f;

        bolt.startColor = Color.white;
        bolt.endColor = new Color(0.5f, 0.8f, 1f);

        bolt.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        bolt.receiveShadows = false;

        RedrawBolt();

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        flickeringRemaning -= Time.deltaTime;

        if (flickeringRemaning <= 0f)
        {
            RedrawBolt();
        }
    }

    private void RedrawBolt()
    {
        flickeringRemaning = Mathf.Max(0.01f, flickeringInterval);

        for (int i = 0; i < bolt.positionCount; i++)
        {
            float progress = (float)i / (bolt.positionCount - 1);
            bool endpoint = i == 0 || i == bolt.positionCount - 1;
            float offsetX = endpoint ? 0f : Random.Range(-spread, spread);
            float offsetZ = endpoint ? 0f : Random.Range(-spread, spread);
            bolt.SetPosition(i, new Vector3(offsetX, boltHeight * progress, offsetZ));
        }
    }
}
