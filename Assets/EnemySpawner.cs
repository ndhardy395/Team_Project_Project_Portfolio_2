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
    }

    IEnumerator SpawnWaveRoutine()
    {
        Debug.Log("SpawnWaveRoutine started.");
        for (int i = 0; i < totalEnemiesToSpawn; i++)
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