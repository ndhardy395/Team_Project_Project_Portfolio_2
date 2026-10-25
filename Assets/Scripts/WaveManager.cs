  using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using TMPro;


public class WaveSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SplineContainer mapSpline;
    [SerializeField] private TextAsset waveJsonFile;

    [Header("Enemy Registry")]
    [SerializeField] private List<EnemyRegistryItem> enemyRegistry = new List<EnemyRegistryItem>();
    private Dictionary<string, GameObject> enemyPrefabDict = new Dictionary<string, GameObject>();


    [Header("Wave UI")]
    [SerializeField] private TMP_Text enemiesRemainingText;
    [SerializeField] private TMP_Text waveNotificationText;
    [SerializeField] private float notificationDuriation = 3f;
    [SerializeField] private GameObject waveNotificationBackground;



    private int activeEnemyCount;

    private Coroutine notificationRoutine;

    private class WaveProgress
    {
        public int waveNumber;
        public int enemiesAlive;
        public bool spawningFinished;
        public bool completed;
        public bool spawnFailed;
    }

    private void UpdateEnemyCountUI()
    {
        if (enemiesRemainingText != null)
        {
            enemiesRemainingText.text = $"Enemies Alive: {activeEnemyCount}";
        }
        
    }

    private void EnemyRemoved(WaveProgress wave)
    {
        wave.enemiesAlive--;
        activeEnemyCount--;

        UpdateEnemyCountUI();
        CheckWaveCompletion(wave);

        Debug.Log($"Wave {wave.waveNumber}: enemy removed. " + $"Alive: {wave.enemiesAlive}, Remaning: {activeEnemyCount}", this);
    }

    private void CheckWaveCompletion(WaveProgress wave)
    {
        if (wave.completed || wave.spawnFailed || !wave.spawningFinished || wave.enemiesAlive > 0)
        {
            return;
        }

        wave.completed = true;

        Debug.Log($"Wave{wave.waveNumber} complete!", this);


        if(waveNotificationText != null )
        {
            if(notificationRoutine != null)
            {
                StopCoroutine(notificationRoutine);
            }

            notificationRoutine = StartCoroutine(ShowWaveNotification(wave.waveNumber));
        }
    }

    private IEnumerator ShowWaveNotification(int  waveNumber)
    {
        waveNotificationText.text = $"Wave: {waveNumber} complete";

        if(waveNotificationBackground != null)
        {
            waveNotificationBackground.SetActive(true);
        }

        yield return new WaitForSeconds(notificationDuriation);

        waveNotificationText.text = "";

        if(waveNotificationBackground != null)
        {
            waveNotificationBackground.SetActive(false);
        }

        notificationRoutine = null;
    }

    private GameData gameData;
    private int currentWaveIndex = 0;

    [System.Serializable]
    public struct EnemyRegistryItem
    {
        public string enemyId;
        public GameObject prefab;
    }

    public static WaveSpawner Instance { get; private set; }



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
        Instance = this;
        UpdateEnemyCountUI();

        if(waveNotificationBackground != null)
        {
            waveNotificationBackground.gameObject.SetActive(false);
        }    

        if(waveNotificationText != null)
        {
            waveNotificationText.text = "";
        }

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

            //Debug.Log($"[DEBUG] Wave {wave.waveNumber} starting. Delay: {wave.delayBeforeWave}s");
            yield return new WaitForSeconds(wave.delayBeforeWave);

            WaveProgress progress = new WaveProgress
            {
                waveNumber = wave.waveNumber
            };

            activeEnemyCount = 0;

            foreach(var group in  wave.spawns)
            {
                activeEnemyCount += group.count;
            }

            UpdateEnemyCountUI();

            // Track how many spawn groups are currently running in this wave
            int activeGroups = wave.spawns.Length;

            // Fire off all spawn groups concurrently
            foreach (var spawnGroup in wave.spawns)
            {
                StartCoroutine(SpawnGroupRoutine(spawnGroup, progress,() => {
                    activeGroups--; // Decrement when a group finishes completely
                }));
            }

            // Wait here until ALL spawn groups in this wave have finished
            yield return new WaitUntil(() => activeGroups <= 0);

            progress.spawningFinished = true;

            yield return new WaitUntil (() => progress.enemiesAlive == 0);

            CheckWaveCompletion(progress);

            currentWaveIndex++;
        }

        Debug.Log(" All waves completed successfully!");
    }

    private IEnumerator SpawnGroupRoutine(SpawnGroup group,WaveProgress wave, System.Action onComplete)
    {
        //Wait before starting this specific group (if a delay is specified)
        if (group.delayBeforeGroup > 0f)
        {
            //Debug.Log($"[DEBUG] Waiting {group.delayBeforeGroup}s before starting group for '{group.enemyId}'");
            yield return new WaitForSeconds(group.delayBeforeGroup);
        }

        //Debug.Log($"[DEBUG] Processing SpawnGroup -> Enemy ID: '{group.enemyId}', Count: {group.count}, Interval: {group.spawnInterval}");

        if (!enemyPrefabDict.TryGetValue(group.enemyId, out GameObject prefab))
        {
            Debug.LogError($"[DEBUG] FATAL: Enemy ID '{group.enemyId}' NOT found in enemyPrefabDict registry!");
            wave.spawnFailed = true;
            onComplete?.Invoke(); //prevents wave from hanging
            yield break;
        }

        GameObject prefabToSpawn = enemyPrefabDict[group.enemyId];

        //Spawn the enemies for this group over time
        for (int i = 0; i < group.count; i++)
        {
            Debug.Log($"[DEBUG] Instantiating enemy {i + 1} of {group.count} for ID '{group.enemyId}'");

            GameObject newEnemy = Instantiate(prefab, transform.position, Quaternion.identity);

            WaveEnemyTracker tracker = newEnemy.GetComponent<WaveEnemyTracker>();

            if(tracker == null)
            {
                tracker = newEnemy.AddComponent<WaveEnemyTracker>();
            }

            wave.enemiesAlive++;
 

            tracker.Initialize(() => EnemyRemoved(wave));

            UpdateEnemyCountUI();

            EnemyMovement movement = newEnemy.GetComponent<EnemyMovement>();

          if(movement != null && mapSpline != null)
            {
                movement.Initialize(mapSpline);
            }
          else
            {
                wave.spawnFailed = true;

                Debug.LogError($" Cannot initialize movement for '{newEnemy.name}'." + "Check EnemyMovement and Map Spline.", newEnemy);

                tracker.ReportRemoved();
                Destroy(newEnemy);
            }
            

            yield return new WaitForSeconds(group.spawnInterval);
        }

        //Let the wave manager know this group is done
        onComplete?.Invoke();
    }

    public void AddWaveCountOne()
    {
        currentWaveIndex++;
    }

    public void UpdateWave()
    {
        StartCoroutine(RunWaveSequence());
    }
}