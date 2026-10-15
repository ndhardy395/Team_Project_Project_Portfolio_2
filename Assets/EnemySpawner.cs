using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public Transform[] waypoints;
    public int totalEnemiesToSpawn = 10;
    public float spawnDelay = 0.5f;

    void Start()
    {
        Debug.Log("EnemySpawner Start() was called.");
        StartCoroutine(SpawnWaveRoutine());
        enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        if (enemyLayer == -1)   //Checks if the Enemy layer exists
            Debug.LogWarning($"No layer named '{enemyLayerName}'. Add it under Layers > Edit Layers, or the gun won't see these enemies.");
    }

    IEnumerator SpawnWaveRoutine()
    {
        Debug.Log("SpawnWaveRoutine started.");
        for (int i = 0; i < totalEnemiesToSpawn; i++)
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnEnemy();

            if (i < totalEnemiesToSpawn - 1)
            {
                yield return new WaitForSeconds(spawnDelay);
            }
        }
    }

    void SpawnEnemy()
    {
        if (EnemyPrefab == null)
        {
            Debug.LogError("ERROR: EnemyPrefab is not assigned on the Spawner!");
            return;
        }
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("ERROR: Waypoints array is empty or unassigned on the Spawner!");
            return;
        }
        if (waypoints[0] == null)
        {
            Debug.LogError("ERROR: Waypoints[0] is null on the Spawner!");
            return;
        }

        Debug.Log("Instantiating enemy number...");
        GameObject newEnemy = Instantiate(EnemyPrefab, waypoints[0].position, Quaternion.identity);
        PathFollow pathFollower = newEnemy.GetComponent<PathFollow>();

        if (pathFollower != null)
        {
            pathFollower.SetPath(waypoints);
        }
        else
        {
            Debug.LogWarning("Spawned enemy is missing the PathFollow script!");
        }
    }
}