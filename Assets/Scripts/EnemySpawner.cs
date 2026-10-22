using System.Collections;
using UnityEngine;
using UnityEngine.Splines; // Required for Unity Splines

public class EnemySpawner : MonoBehaviour
{
    [Header("Portal")]
    public GameObject enemyPortal;

    public float portalDistance;

    [Header("PortalPosition")]
    public Vector3 portalOffset = new Vector3(0f, 0.15f, -2f);

    public GameObject EnemyPrefab;

    [Header("Path Reference")]
    public SplineContainer mapSpline;

    public int totalEnemiesToSpawn = 10;
    public float spawnDelay = 0.5f;


    private EnemyPortalControll portalController;

    void Start()
    {
        if(enemyPortal != null)
        {
            portalController = enemyPortal.GetComponentInChildren<EnemyPortalControll>();
        }

        Debug.Log("EnemySpawner Start() was called.");

        StartCoroutine(SpawnWaveRoutine());

        if(enemyPortal != null && mapSpline != null)
        {
            Vector3 startPos = mapSpline.EvaluatePosition(0f);
            enemyPortal.transform.position = startPos + portalOffset;
            enemyPortal.SetActive(false);

        }


        StartCoroutine(SpawnWaveRoutine());
    }

    IEnumerator SpawnWaveRoutine()
    {
       
        Debug.Log("SpawnWaveRoutine started.");

        if (portalController != null)// Open the portal
        {
            portalController.BeginSpawning();

            if (portalController.portalAnimation != null)
            {
                yield return new WaitUntil(() => portalController.portalAnimation.IsOpen);
            }
        }

        for (int i = 0; i < totalEnemiesToSpawn; i++)
        {
            SpawnEnemy();

            if (i < totalEnemiesToSpawn - 1)
            {
                yield return new WaitForSeconds(spawnDelay);
            }
        }

        if(portalController != null)// close portal
        {
            portalController.FinishSpawning();
        }        

        Debug.Log("All Enemies spawned. Portal Closed");
    }

    void SpawnEnemy()
    {
        if (EnemyPrefab == null)
        {
            Debug.LogError("ERROR: EnemyPrefab is not assigned on the Spawner!");
            return;
        }
        if (mapSpline == null)
        {
            Debug.LogError("ERROR: mapSpline is not assigned on the Spawner!");
            return; //this would be good if we decided to make a map editor, or made you able to load your own maps in from files
        }

        // Get the starting position (at progress = 0) from the spline automatically
        Vector3 startPos = mapSpline.EvaluatePosition(0f);

        GameObject newEnemy = Instantiate(EnemyPrefab, startPos, Quaternion.identity);

        EnemyMovement enemyMover = newEnemy.GetComponent<EnemyMovement>();

        if (enemyMover != null)
        {
            enemyMover.splineContainer = mapSpline;
        }
        else
        {
            Debug.LogWarning("Spawned enemy is missing the movement script!");
        }
    }
}