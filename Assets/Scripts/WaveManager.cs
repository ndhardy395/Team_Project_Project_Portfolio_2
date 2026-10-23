using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class WaveSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SplineContainer mapSpline;
    [SerializeField] private TextAsset waveJsonFile;

    [Header("Enemy Registry")]
    [SerializeField] private List<EnemyRegistryItem> enemyRegistry = new List<EnemyRegistryItem>();
    private Dictionary<string, GameObject> enemyPrefabDict = new Dictionary<string, GameObject>();

    private GameData gameData;
    private int currentWaveIndex = 0;

    [System.Serializable]
    public struct EnemyRegistryItem
    {
        public string enemyId;
        public GameObject prefab;
    }

    void Awake()
    {
        Debug.Log("[DEBUG] WaveSpawner Awake started.");

        // 1. Build dictionary
        foreach (var item in enemyRegistry)
        {
            if (!string.IsNullOrEmpty(item.enemyId) && item.prefab != null)
            {
                if (!enemyPrefabDict.ContainsKey(item.enemyId))
                {
                    enemyPrefabDict.Add(item.enemyId, item.prefab);
                    Debug.Log($"[DEBUG] Registered Enemy ID: '{item.enemyId}' with Prefab: {item.prefab.name}");
                }
            }
            else
            {
                Debug.LogWarning("[DEBUG] An item in the Enemy Registry has a missing ID or Prefab!");
            }
        }

        // 2. Parse JSON
        if (waveJsonFile != null)
        {
            gameData = JsonUtility.FromJson<GameData>(waveJsonFile.text);
            if (gameData != null && gameData.waves != null)
            {
                Debug.Log($"[DEBUG] JSON successfully parsed. Total waves found: {gameData.waves.Length}");
            }
            else
            {
                Debug.LogError("[DEBUG] JSON parsed, but 'waves' array is NULL! Check your JSON root structure.");
            }
        }
        else
        {
            Debug.LogError("[DEBUG] No wave JSON TextAsset assigned in the Inspector!");
        }
    }

    void Start()
    {
        if (mapSpline == null)
        {
            Debug.LogError("[DEBUG] Map Spline reference is missing on the Spawner!");
        }

        if (gameData != null && gameData.waves != null && gameData.waves.Length > 0)
        {
            Debug.Log("[DEBUG] Starting wave sequence coroutine.");
            StartCoroutine(RunWaveSequence());
        }
        else
        {
            Debug.LogError("[DEBUG] Cannot start waves because gameData or waves array is empty/null!");
        }
    }

    private IEnumerator RunWaveSequence()
    {
        while (currentWaveIndex < gameData.waves.Length)
        {
            WaveData wave = gameData.waves[currentWaveIndex];

            Debug.Log($"[DEBUG] Wave {wave.waveNumber} starting. Delay: {wave.delayBeforeWave}s");
            yield return new WaitForSeconds(wave.delayBeforeWave);

            foreach (var spawnGroup in wave.spawns)
            {
                yield return StartCoroutine(SpawnGroupRoutine(spawnGroup));
            }

            currentWaveIndex++;
        }

        Debug.Log("[DEBUG] All waves completed successfully!");
    }

    private IEnumerator SpawnGroupRoutine(SpawnGroup group)
    {
        Debug.Log($"[DEBUG] Processing SpawnGroup -> Enemy ID: '{group.enemyId}', Count: {group.count}, Interval: {group.spawnInterval}");

        if (!enemyPrefabDict.ContainsKey(group.enemyId))
        {
            Debug.LogError($"[DEBUG] FATAL: Enemy ID '{group.enemyId}' NOT found in enemyPrefabDict registry!");
            yield break;
        }

        GameObject prefabToSpawn = enemyPrefabDict[group.enemyId];

        for (int i = 0; i < group.count; i++)
        {
            Debug.Log($"[DEBUG] Instantiating enemy {i + 1} of {group.count} for ID '{group.enemyId}'");

            GameObject newEnemy = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);

            var enemyMovement = newEnemy.GetComponent<EnemyMovement>();
            if (enemyMovement != null && mapSpline != null)
            {
                enemyMovement.Initialize(mapSpline);
                Debug.Log($"[DEBUG] Successfully initialized movement for spawned enemy {i + 1}");
            }
            else
            {
                Debug.LogWarning("[DEBUG] Instantiated enemy, but MISSING EnemyMovement script on prefab OR MapSpline is null!");
            }

            yield return new WaitForSeconds(group.spawnInterval);
        }
    }
}