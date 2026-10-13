using System.Collections;
using UnityEngine;

// Spawns capsule test enemies on a timer. They walk along this object's
// blue (Z) arrow for travelDistance, then despawn.
public class EnemySpawner : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public Transform[] waypoints;
    public int totalEnemiesToSpawn = 10; // Number of enemies to spawn
    public float spawnDelay = 0.5f;       // delay between spawns in seconds

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpawnWaveRoutine());
    }

    IEnumerator SpawnWaveRoutine()
    {
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
        GameObject newEnemy = Instantiate(EnemyPrefab, transform.position, Quaternion.identity); //creates instance
        PathFollow PathFollower = newEnemy.GetComponent<PathFollow>();

        if (PathFollower != null)
        {
            //handles passing path to enemy!
            PathFollower.SetPath(waypoints);
    }
        else
    {
            Debug.LogWarning("Enemy didn't get that path from the script");
        }
    }

}
