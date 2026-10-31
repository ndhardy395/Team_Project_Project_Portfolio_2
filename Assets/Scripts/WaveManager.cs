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

    [SerializeField] private TMP_Text baseHealthText;
    [SerializeField] private float notificationDuriation = 3f;
    [SerializeField] private GameObject waveNotificationBackground;

    [SerializeField] private BaseHealth baseHealth;
    [SerializeField] GameResultManager gameResultManager;

    private int activeEnemyCount;
    private bool canCallNextWave;
    private bool nextWaveRequested;
    private bool sequenceStarted;

    private Coroutine notificationRoutine;
    private bool skipDelayRequested = false;
    private bool forceNextWaveRequested = false;

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

    public void ForceNextWave()
    {
        skipDelayRequested = true;
        forceNextWaveRequested = true;
    }

    private void EnemyRemoved(WaveProgress wave)
    {
        wave.enemiesAlive--;
        activeEnemyCount--;

        UpdateEnemyCountUI();
        CheckWaveCompletion(wave);

        Debug.Log($"Wave {wave.waveNumber}: enemy removed. " + $"Alive: {wave.enemiesAlive}, Remaining: {activeEnemyCount}", this);
    }

 

    private void CheckWaveCompletion(WaveProgress wave)
    {
        if (wave.completed || wave.spawnFailed || !wave.spawningFinished || wave.enemiesAlive > 0)
        {
            return;
        }

        wave.completed = true;

        Debug.Log($"Wave {wave.waveNumber} complete!", this);

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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
       
        Instance = this;

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
        if (sequenceStarted)
        {
            yield break;
        }

        sequenceStarted = true;

        if (baseHealth == null)
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

            while (timeRemaining > 0f && !nextWaveRequested && !skipDelayRequested)
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
            skipDelayRequested = false;

            if (baseHealth == null || !baseHealth.IsAlive)
            {
                yield break;
            }

            WaveProgress progress = new WaveProgress
            {
                waveNumber = wave.waveNumber
            };

            activeEnemyCount = 0;

            foreach (var group in wave.spawns)
            {
                activeEnemyCount += group.count;
            }

            UpdateEnemyCountUI();

            int activeGroups = wave.spawns.Length;

            foreach (var group in wave.spawns)
            {
                StartCoroutine(SpawnGroupRoutine(group, progress, () => { activeGroups--; }));
            }

            yield return new WaitUntil(() => activeGroups <= 0 || baseHealth == null || !baseHealth.IsAlive || forceNextWaveRequested);

            if (baseHealth == null || !baseHealth.IsAlive)
            {
                yield break;
            }

            progress.spawningFinished = true;

            if (progress.spawnFailed)
            {
                Debug.LogError("Wave failed to spawn correctly, Victory Blocked.");
                yield break;
            }

            yield return new WaitUntil(() => progress.enemiesAlive == 0 || baseHealth == null || !baseHealth.IsAlive || forceNextWaveRequested);

            if (baseHealth == null || !baseHealth.IsAlive)
            {
                yield break;
            }

            CheckWaveCompletion(progress);
            currentWaveIndex++;
            forceNextWaveRequested = false;
        }

        if (baseHealth != null && baseHealth.IsAlive && activeEnemyCount == 0)
        {
            if (gameResultManager != null)
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
        if (group.delayBeforeGroup > 0f)
        {
            yield return new WaitForSeconds(group.delayBeforeGroup);
        }

        if (!enemyPrefabDict.TryGetValue(group.enemyId, out GameObject prefab))
        {
            Debug.LogError($"[DEBUG] FATAL: Enemy ID '{group.enemyId}' NOT found in enemyPrefabDict registry!");
            wave.spawnFailed = true;
            onComplete?.Invoke();
            yield break;
        }

        for (int i = 0; i < group.count; i++)
        {
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
                Debug.LogError($"Cannot initialize movement for '{newEnemy.name}'. Check EnemyMovement and Map Spline.", newEnemy);

                tracker.ReportRemoved();
                Destroy(newEnemy);
            }

            yield return new WaitForSeconds(group.spawnInterval);
        }

        onComplete?.Invoke();
    }

    public void RequestNextWave()
    {
        Debug.LogError($"Next Clicked: cancelNextWave={canCallNextWave}, " + $"nextWaveRequested={nextWaveRequested}");

        if (!canCallNextWave || nextWaveRequested)
        {
            Debug.LogError("Next blocked: not waiting between waves, or already requested.");
            return;
        }

        if (baseHealth == null || !baseHealth.IsAlive)
        {
            Debug.Log("Next bloked: base missing or destroyed");
            return;
        }

        nextWaveRequested = true;
    }

    private void OnDestroy()
    {
        if(Instance == this)
        {
            Instance = null;
        }
    }
}