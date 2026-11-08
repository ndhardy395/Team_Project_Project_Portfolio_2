using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Splines;

public class WaveSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SplineContainer defaultMapSpline;
    [SerializeField] private TextAsset waveJsonFile;
    [SerializeField] private BaseHealth baseHealth;
    [SerializeField] private GameResultManager gameResultManager;

    [System.Serializable]
    public struct PathRegistryItem
    {
        public string pathID;
        public SplineContainer splineContainer;
    }

    [System.Serializable]
    public struct EnemyRegistryItem
    {
        public string enemyId;
        public GameObject prefab;
    }

    [Header("Path Registry")]
    [SerializeField] private List<PathRegistryItem> pathRegistry = new List<PathRegistryItem>();

    [Header("Enemy Registry")]
    [SerializeField] private List<EnemyRegistryItem> enemyRegistry = new List<EnemyRegistryItem>();
    private readonly Dictionary<string, GameObject> enemyPrefabDict = new Dictionary<string, GameObject>();

    [Header("Wave UI")]
    [SerializeField] private TMP_Text enemiesRemainingText;
    [SerializeField] private TMP_Text waveNotificationText;
    [SerializeField] private TMP_Text baseHealthText;
    // Keep the original field name so existing Inspector settings are preserved.
    [SerializeField] private float notificationDuriation = 3f;
    [SerializeField] private GameObject waveNotificationBackground;
    [SerializeField] private TMP_Text currentWaveText;

    [Header("Difficulty Settings")]
    [SerializeField, Tooltip("0.9 for Easy, 1.0 for Medium, 1.1 for Hard, 1.3 for Hardest")]
    private float healthMultiplier = 1.0f;
    [SerializeField, Tooltip("0.95 for Easy, 1.0 for Medium, 1.05 for Hard, 1.1 for Hardest")]
    private float speedMultiplier = 1.0f;

    private class WaveProgress
    {
        public int waveNumber;
        public int enemiesAlive;
        public int enemiesRemaining;
        public int pendingGroups;
        public bool spawningFinished;
        public bool completed;
        public bool spawnFailed;
    }

    private GameData gameData;
    private WaveProgress latestWave;
    // This points to the next UNLAUNCHED wave, never to an active wave.
    private int nextWaveIndex;
    private int currentWaveNumber;
    // Includes living enemies AND enemies reserved by delayed spawn groups.
    private int activeEnemyCount;
    private int pendingSpawnGroups;
    private bool sequenceStarted;
    private bool gameStopped;
    private bool anySpawnFailed;
    private bool preparingNextWave;
    private float currentWaveDelayTimer;
    private Coroutine notificationRoutine;

    public static WaveSpawner Instance { get; private set; }
    public int CurrentWaveNumber => currentWaveNumber;

    private bool CanRun => sequenceStarted && !gameStopped &&
        baseHealth != null && baseHealth.IsAlive && Time.timeScale > 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        foreach (EnemyRegistryItem item in enemyRegistry)
        {
            if (!string.IsNullOrEmpty(item.enemyId) && item.prefab != null &&
                !enemyPrefabDict.ContainsKey(item.enemyId))
            {
                enemyPrefabDict.Add(item.enemyId, item.prefab);
            }
        }

        if (waveJsonFile != null)
        {
            gameData = JsonUtility.FromJson<GameData>(waveJsonFile.text);
        }
    }

    private void Start()
    {
        UpdateEnemyCountUI();
        UpdateWaveUI();
        if (waveNotificationText != null) waveNotificationText.text = "";
        if (waveNotificationBackground != null) waveNotificationBackground.SetActive(false);

        if (baseHealth == null || gameResultManager == null || gameData == null ||
            gameData.waves == null || gameData.waves.Length == 0)
        {
            Debug.LogError("Assign BaseHealth, GameResultManager, and a JSON file containing waves.", this);
            return;
        }

        sequenceStarted = true;
    }

    private void Update()
    {
        if (!CanRun || anySpawnFailed) return;

        if (nextWaveIndex >= gameData.waves.Length)
        {
            TryWinGame();
            return;
        }

        // Automatic advancement waits for the most recently launched wave.
        // The Next button bypasses this wait without cancelling older waves.
        if (latestWave != null && !latestWave.completed) return;

        if (!preparingNextWave)
        {
            preparingNextWave = true;
            currentWaveDelayTimer = Mathf.Max(0f, gameData.waves[nextWaveIndex].delayBeforeWave);
        }

        currentWaveDelayTimer = Mathf.Max(0f, currentWaveDelayTimer - Time.deltaTime);
        if (currentWaveDelayTimer <= 0f) LaunchNextWave(false);
    }

    public SplineContainer GetSpline(string pathId)
    {
        foreach (PathRegistryItem item in pathRegistry)
        {
            if (item.pathID == pathId) return item.splineContainer;
        }
        return null;
    }

    public void ForceNextWave()
    {
        // Each call launches exactly one wave; there is no cooldown that drops presses.
        LaunchNextWave(true);
    }

    public void RequestNextWave()
    {
        ForceNextWave();
    }

    private void LaunchNextWave(bool calledEarly)
    {
        if (!CanRun || anySpawnFailed || nextWaveIndex >= gameData.waves.Length) return;

        WaveData wave = gameData.waves[nextWaveIndex];
        float remainingDelay = preparingNextWave
            ? currentWaveDelayTimer : Mathf.Max(0f, wave.delayBeforeWave);

        // Reserve the wave before starting coroutines, which can execute immediately.
        nextWaveIndex++;
        currentWaveNumber = wave.waveNumber;
        preparingNextWave = false;
        currentWaveDelayTimer = 0f;

        WaveProgress progress = new WaveProgress
        {
            waveNumber = wave.waveNumber,
            pendingGroups = wave.spawns != null ? wave.spawns.Length : 0
        };
        latestWave = progress;

        if (wave.spawns != null)
        {
            foreach (SpawnGroup group in wave.spawns)
            {
                if (group.count < 0)
                {
                    progress.spawnFailed = true;
                    anySpawnFailed = true;
                    Debug.LogError($"Wave {wave.waveNumber} contains a negative enemy count. Victory blocked.", this);
                }
                progress.enemiesRemaining += Mathf.Max(0, group.count);
            }
        }

        activeEnemyCount += progress.enemiesRemaining;
        pendingSpawnGroups += progress.pendingGroups;
        progress.spawningFinished = progress.pendingGroups == 0;
        UpdateEnemyCountUI();
        UpdateWaveUI();
        ShowNotification($"Wave {wave.waveNumber} incoming!");

        // The early-call reward is paid once per launched wave, never after the cap.
        int secondsLeft = Mathf.FloorToInt(remainingDelay);
        if (calledEarly && secondsLeft > 0 && CurrencyManager.instance != null)
        {
            CurrencyManager.instance.AddCurrency(secondsLeft * 2);
        }

        if (wave.spawns != null)
        {
            foreach (SpawnGroup group in wave.spawns)
            {
                StartCoroutine(SpawnGroupRoutine(group, progress));
            }
        }
        CheckWaveCompletion(progress);
    }

    private IEnumerator SpawnGroupRoutine(SpawnGroup group, WaveProgress wave)
    {
        int unspawnedCount = Mathf.Max(0, group.count);
        try
        {
            if (unspawnedCount == 0) yield break;

            if (!enemyPrefabDict.TryGetValue(group.enemyId, out GameObject prefab) || prefab == null)
            {
                Debug.LogError($"Wave {wave.waveNumber}: unknown enemy ID '{group.enemyId}'. Victory blocked.", this);
                yield break;
            }

            SplineContainer targetSpline = group.pathID == 0
                ? defaultMapSpline : GetSpline(group.pathID.ToString());

            if (targetSpline == null || targetSpline.CalculateLength() <= 0f ||
                prefab.GetComponent<EnemyMovement>() == null)
            {
                Debug.LogError($"Wave {wave.waveNumber}: check path ID {group.pathID} and EnemyMovement on '{prefab.name}'. Victory blocked.", this);
                yield break;
            }

            if (group.delayBeforeGroup > 0f)
                yield return new WaitForSeconds(group.delayBeforeGroup);

            while (unspawnedCount > 0)
            {
                if (gameStopped || baseHealth == null || !baseHealth.IsAlive) yield break;

                GameObject newEnemy = Instantiate(prefab, transform.position, Quaternion.identity);

                EnemyHealthDisplay healthDisplay = newEnemy.GetComponent<EnemyHealthDisplay>();
                if (healthDisplay != null)
                {
                    healthDisplay.Initialize(wave.waveNumber, healthMultiplier, speedMultiplier);
                }

                Enemy enemyComponent = newEnemy.GetComponent<Enemy>();
                if (enemyComponent != null)
                {
                    // Pass the wave number and configured difficulty multipliers. For HEALTH SCALING!
                    enemyComponent.Initialize(wave.waveNumber, healthMultiplier, speedMultiplier);
                }

                WaveEnemyTracker tracker = newEnemy.GetComponent<WaveEnemyTracker>();
                if (tracker == null) tracker = newEnemy.AddComponent<WaveEnemyTracker>();

                unspawnedCount--;
                wave.enemiesAlive++;
                tracker.Initialize(() => EnemyRemoved(wave));

                EnemyMovement movement = newEnemy.GetComponent<EnemyMovement>();
                movement.baseHealth = baseHealth;
                movement.Initialize(targetSpline);

                // Keep the JSON spacing, but do not wait after the final enemy.
                if (unspawnedCount > 0)
                    yield return new WaitForSeconds(Mathf.Max(0f, group.spawnInterval));
            }
        }
        finally
        {
            // Also release reserved enemies if a group fails or is cancelled.
            if (unspawnedCount > 0)
            {
                wave.enemiesRemaining -= unspawnedCount;
                activeEnemyCount -= unspawnedCount;
                wave.spawnFailed = true;
                anySpawnFailed = true;
            }

            wave.pendingGroups--;
            pendingSpawnGroups--;
            wave.spawningFinished = wave.pendingGroups == 0;
            UpdateEnemyCountUI();
            CheckWaveCompletion(wave);
        }
    }

    private void EnemyRemoved(WaveProgress wave)
    {
        wave.enemiesAlive--;
        wave.enemiesRemaining--;
        activeEnemyCount--;
        UpdateEnemyCountUI();
        CheckWaveCompletion(wave);
    }

    private void CheckWaveCompletion(WaveProgress wave)
    {
        if (gameStopped || baseHealth == null || !baseHealth.IsAlive || wave.completed ||
            wave.spawnFailed || !wave.spawningFinished || wave.enemiesRemaining != 0) return;

        wave.completed = true;
        Debug.Log($"Wave {wave.waveNumber} complete!", this);
        ShowNotification($"Wave {wave.waveNumber} complete");
    }

    private void TryWinGame()
    {
        if (!CanRun || anySpawnFailed || nextWaveIndex < gameData.waves.Length ||
            pendingSpawnGroups != 0 || activeEnemyCount != 0) return;

        gameStopped = true;
        gameResultManager.WinGame();
    }

    private void UpdateEnemyCountUI()
    {
        if (enemiesRemainingText != null)
            enemiesRemainingText.text = $"Enemies remaining: {activeEnemyCount}";
    }

    private void UpdateWaveUI()
    {
        int totalWaves = gameData != null && gameData.waves != null ? gameData.waves.Length : 0;
        if (currentWaveText != null)
            currentWaveText.text = $"Wave {currentWaveNumber} / {totalWaves}";
    }

    private void ShowNotification(string message)
    {
        if (waveNotificationText == null || !isActiveAndEnabled) return;
        if (notificationRoutine != null) StopCoroutine(notificationRoutine);
        notificationRoutine = StartCoroutine(ShowWaveNotification(message));
    }

    private IEnumerator ShowWaveNotification(string message)
    {
        waveNotificationText.text = message;
        if (waveNotificationBackground != null) waveNotificationBackground.SetActive(true);
        yield return new WaitForSeconds(Mathf.Max(0.01f, notificationDuriation));
        waveNotificationText.text = "";
        if (waveNotificationBackground != null) waveNotificationBackground.SetActive(false);
        notificationRoutine = null;
    }

    private void OnDestroy()
    {
        gameStopped = true;
        if (Instance == this) Instance = null;
    }
}
