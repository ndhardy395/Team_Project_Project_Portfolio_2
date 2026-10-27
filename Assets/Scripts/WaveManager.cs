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

    [SerializeField] private BaseHealth baseHealth;
    [SerializeField] GameResultManager gameResultManager;



    private int activeEnemyCount;

    private bool canCallNextWave;
    private bool nextWaveRequested;
    private bool sequenceStarted;

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
            enemiesRemainingText.text = $"Enemies Remaining: {activeEnemyCount}";
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


        if (waveNotificationText != null)
        {
            if (notificationRoutine != null)
            {
                StopCoroutine(notificationRoutine);
            }

            notificationRoutine = StartCoroutine(ShowWaveNotification(wave.waveNumber));
        }
    }

    private IEnumerator ShowWaveNotification(int waveNumber)
    {
        waveNotificationText.text = $"Wave: {waveNumber} complete";

        if (waveNotificationBackground != null)
        {
            waveNotificationBackground.SetActive(true);
        }

        yield return new WaitForSeconds(notificationDuriation);

        waveNotificationText.text = "";

        if (waveNotificationBackground != null)
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

        if (waveNotificationBackground != null)
        {
            waveNotificationBackground.gameObject.SetActive(false);
        }

        if (waveNotificationText != null)
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
        if (sequenceStarted) // only one sequence may control wave progression
        {
            yield break;
        }

        sequenceStarted = true;

        if(baseHealth == null)
        {
            Debug.LogError("Assign fence to wave Spawner");
            yield break;
        }


        while (currentWaveIndex < gameData.waves.Length)
        {
            WaveData wave = gameData.waves[currentWaveIndex];

            nextWaveRequested = false;
            canCallNextWave = true;

            float timeRemaining = wave.delayBeforeWave;

            // Next can skip this delay, but cannot skip the wave.

            while (timeRemaining > 0f && !nextWaveRequested)
            {
                if (baseHealth == null || !baseHealth.IsAlive)
                {
                    canCallNextWave = false;
                    yield break;
                }

                timeRemaining -= Time.deltaTime;
                yield return null;

            }

            canCallNextWave = false;

            if (baseHealth == null || !baseHealth.IsAlive)
            {
                yield break;
            }

            WaveProgress progress = new WaveProgress
            {
                waveNumber = wave.waveNumber
            };

            // count all enemies coming in this wave, including pending spawns.

            activeEnemyCount = 0;

            foreach (var group in wave.spawns)
            {
                activeEnemyCount += group.count;          
            }

            UpdateEnemyCountUI();

            int activeGroups = wave.spawns.Length;

            foreach(var group in wave.spawns)
            {
                StartCoroutine(SpawnGroupRoutine(group, progress, () => { activeGroups--; }));
            }

            yield return new WaitUntil(() => activeGroups <= 0 || baseHealth == null || !baseHealth.IsAlive);

            if(baseHealth == null || !baseHealth.IsAlive)
            {
                yield break;
            }

            progress.spawningFinished = true;

            if(progress.spawnFailed)
            {
                Debug.LogError("Wave faild to spawn correctly, Victory Blocked.");
                yield break;
            }

            // Every enemy must be resolved before advancing
            yield return new WaitUntil(() => progress.enemiesAlive == 0 || baseHealth == null || !baseHealth.IsAlive);

            if(baseHealth == null || !baseHealth.IsAlive)
            {
                yield break;
            }

            CheckWaveCompletion(progress);
            currentWaveIndex++;
        }// end of the wave loop.

        // All waves have now Finished

        if(baseHealth != null && baseHealth.IsAlive || activeEnemyCount == 0)
        {
            if(gameResultManager != null)
            {
                gameResultManager.WinGame();
            }
            else
            {
                Debug.LogError("Assign game result manager on waveSpawner");
            }
        }
      
    }

    private IEnumerator SpawnGroupRoutine(SpawnGroup group, WaveProgress wave, System.Action onComplete)
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

            if (tracker == null)
            {
                tracker = newEnemy.AddComponent<WaveEnemyTracker>();
            }

            wave.enemiesAlive++;


            tracker.Initialize(() => EnemyRemoved(wave));

            UpdateEnemyCountUI();

            EnemyMovement movement = newEnemy.GetComponent<EnemyMovement>();

            if (movement != null && mapSpline != null)
            {
                movement.baseHealth = baseHealth;
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

    public void RequestNextWave()
    {
        if (!canCallNextWave || nextWaveRequested)
        {
            return;
        }

        if (baseHealth == null || !baseHealth.IsAlive)
        {
            return;
        }

        nextWaveRequested = true;
    }
}