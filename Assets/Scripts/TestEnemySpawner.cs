using UnityEngine;

// Spawns capsule test enemies on a timer. They walk along this object's
// blue (Z) arrow for travelDistance, then despawn.
public class TestEnemySpawner : MonoBehaviour
{
    public float spawnInterval = 1.5f;     // seconds between spawns
    public float travelDistance = 40f;     // walk this far, then despawn
    public float enemySpeed = 3f;          // units per second
    public float enemyHealth = 100f;
    public string enemyLayerName = "Enemy";

    float timer;
    int enemyLayer;

    void Start()
    {
        enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        if (enemyLayer == -1)   //Checks if the Enemy layer exists
            Debug.LogWarning($"No layer named '{enemyLayerName}'. Add it under Layers > Edit Layers, or the gun won't see these enemies.");
    }

    void Update()
    {
        //Time to spawn an ememy
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Spawn();
            timer = spawnInterval;
        }
    }

    void Spawn()
    {
        // CreatePrimitive builds a capsule mesh with a CapsuleCollider already on it.
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "TestEnemy";
        go.transform.SetPositionAndRotation(transform.position, transform.rotation);
        if (enemyLayer != -1) go.layer = enemyLayer;
        go.GetComponent<Renderer>().material.color = Color.red;

        EnemyNoah enemy = go.AddComponent<EnemyNoah>();
        enemy.Init(enemySpeed, enemyHealth, travelDistance);
    }

    // Shows the walking lane in the Scene view: cyan = start, magenta = despawn point.
    void OnDrawGizmos()
    {
        Vector3 end = transform.position + transform.forward * travelDistance;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, end);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(end, 0.5f);
    }
}
