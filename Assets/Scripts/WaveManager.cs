using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Splines;

// Runs a level's waves from its JSON file (format in WaveData.cs). Once a wave has spawned its last
// enemy, Update counts down the next wave's delay and launches it, even if enemies are still alive.
// The Next button and Spacebar launch the next wave right away, so several waves can be on the map
// at once. Calls WinGame once every wave has launched and every enemy is gone. The file is
// WaveManager.cs, but other scripts reach it as WaveSpawner.Instance.
public class WaveSpawner : MonoBehaviour
{
    [Header("References")]
    // Path for spawn groups whose pathID is 0.
    [SerializeField] private SplineContainer defaultMapSpline;
    // This level's waves. Parsed into gameData in Awake.
    [SerializeField] private TextAsset waveJsonFile;
    // The fence. Spawning stops as soon as it dies.
    [SerializeField] private BaseHealth baseHealth;
    // Shows the win screen after the last wave.
    [SerializeField] private GameResultManager gameResultManager;

    // One inspector entry linking a pathID from the JSON to a spline in the scene.
    [System.Serializable]
    public struct PathRegistryItem
    {
        public string pathID;
        public SplineContainer splineContainer;
    }

    // One inspector entry linking an enemyId from the JSON to its prefab.
    [System.Serializable]
    public struct EnemyRegistryItem
    {
        public string enemyId;
        public GameObject prefab;
    }

    [Header("Path Registry")]
    // Extra paths for multi-path maps, matched against each spawn group's pathID.
    [SerializeField] private List<PathRegistryItem> pathRegistry = new List<PathRegistryItem>();

    [Header("Enemy Registry")]
    // Copied into enemyPrefabDict in Awake for quick lookup by enemyId.
    [SerializeField] private List<EnemyRegistryItem> enemyRegistry = new List<EnemyRegistryItem>();
    private readonly Dictionary<string, GameObject> enemyPrefabDict = new Dictionary<string, GameObject>();

    [Header("Wave UI")]
    [SerializeField] private TMP_Text enemiesRemainingText;
    [SerializeField] private TMP_Text waveNotificationText;
    [SerializeField] private TMP_Text baseHealthText;
    // How long the wave banner stays up, in seconds.
    // Keep the original field name so existing Inspector settings are preserved.
    [SerializeField] private float notificationDuriation = 3f;
    [SerializeField] private GameObject waveNotificationBackground;
    [SerializeField] private TMP_Text currentWaveText;

    // Passed to each enemy as it spawns, to scale its health and speed.
    [Header("Difficulty Settings")]
    [SerializeField, Tooltip("0.9 for Easy, 1.0 for Medium, 1.1 for Hard, 1.3 for Hardest")]
    private float healthMultiplier = 1.0f;
    [SerializeField, Tooltip("0.95 for Easy, 1.0 for Medium, 1.05 for Hard, 1.1 for Hardest")]
    private float speedMultiplier = 1.0f;

    [Header("Skip Settings")]
    [SerializeField, Tooltip("Minimum time the player must wait between manually forcing waves.")]
    private float minForceInterval = 0.75f;
    private float lastForcedWaveTime = -999f;

    // Bookkeeping for one launched wave. Waves can overlap, so each keeps its own counts.
    private class WaveProgress
    {
        public int waveNumber;
        public int enemiesAlive;       // spawned and not yet removed
        public int enemiesRemaining;   // alive plus not yet spawned; 0 means the wave is cleared
        public int pendingGroups;      // spawn groups still running
        public bool spawningFinished;  // every group is done; the next wave's countdown can start
        public bool completed;         // the "Wave N complete" banner has been shown
        public bool spawnFailed;       // something couldn't spawn; blocks the win
    }

    // Parsed contents of waveJsonFile.
    private GameData gameData;
    // The most recently launched wave. The countdown to the next wave waits for it to finish spawning.
    private WaveProgress latestWave;
    // This points to the next UNLAUNCHED wave, never to an active wave.
    private int nextWaveIndex;
    // The most recently launched wave's number, shown in the HUD.
    private int currentWaveNumber;
    // Includes living enemies AND enemies reserved by delayed spawn groups.
    private int activeEnemyCount;
    // Spawn groups still running, across every wave. The win waits for this to reach 0.
    private int pendingSpawnGroups;
    // Set in Start once every reference checks out. Nothing runs until then.
    private bool sequenceStarted;
    // Set after a win or when this object is destroyed. Stops all spawning.
    private bool gameStopped;
    // Set when any wave fails to spawn. Stops new waves and blocks the win.
    private bool anySpawnFailed;
    // True while counting down to the next wave.
    private bool preparingNextWave;
    // Seconds left in that countdown.
    private float currentWaveDelayTimer;
    // The banner coroutine that's running, so a new banner can replace it.
    private Coroutine notificationRoutine;

    // The spawner in this scene, so the HUD and WaveSkipper can reach it.
    public static WaveSpawner Instance { get; private set; }
    // GameResultManager records this as the level's best wave on a win or loss.
    public int CurrentWaveNumber => currentWaveNumber;

    // Waves only advance while the level is set up, the fence is standing and the game isn't paused.
    private bool CanRun => sequenceStarted && !gameStopped &&
        baseHealth != null && baseHealth.IsAlive && Time.timeScale > 0f;

    private void Awake()
    {
        // Only one spawner per scene; a second one destroys itself.
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

        // Name exactly what's missing, so whoever hits this knows which slot to fill in.
        List<string> missing = new List<string>();
        if (baseHealth == null) missing.Add("Base Health");
        if (gameResultManager == null) missing.Add("Game Result Manager (the scene needs a GameResultManager object)");
        if (waveJsonFile == null) missing.Add("Wave Json File");
        else if (gameData == null || gameData.waves == null || gameData.waves.Length == 0)
            missing.Add($"waves in '{waveJsonFile.name}' (the file has none, or isn't valid JSON)");

        if (missing.Count > 0)
        {
            Debug.LogError($"WaveSpawner in scene '{gameObject.scene.name}' can't start the waves. Missing: " +
                           string.Join(", ", missing) + ". Click this message to highlight the WaveSpawner.", this);
            return;
        }

        // Everything's in place; Update can start running waves.
        sequenceStarted = true;
    }

    // Counts down to the next wave and launches it when the timer hits 0.
    // After the last wave has launched, it checks for the win every frame instead.
    private void Update()
    {
        if (!CanRun || anySpawnFailed) return;

        if (nextWaveIndex >= gameData.waves.Length)
        {
            TryWinGame();
            return;
        }

        // The countdown starts once the latest wave has spawned its last enemy, not when it's cleared,
        // so the next wave can still arrive while enemies are on the map.
        if (latestWave != null && !latestWave.spawningFinished) return;

        // Start the countdown from this wave's delayBeforeWave in the JSON.
        if (!preparingNextWave)
        {
            preparingNextWave = true;
            currentWaveDelayTimer = Mathf.Max(0f, gameData.waves[nextWaveIndex].delayBeforeWave);
        }

        currentWaveDelayTimer = Mathf.Max(0f, currentWaveDelayTimer - Time.deltaTime);
        if (currentWaveDelayTimer <= 0f) LaunchNextWave(false);
    }

    // Returns the registered spline for a path ID, or null if none matches.
    public SplineContainer GetSpline(string pathId)
    {
        foreach (PathRegistryItem item in pathRegistry)
        {
            if (item.pathID == pathId) return item.splineContainer;
        }
        return null;
    }

    // Launches the next wave right away. Called by the HUD Next button (ClickCode) and Spacebar (WaveSkipper).
    public void ForceNextWave()
    {
        // Each call launches exactly one wave, if it can complete

        if (Time.time < lastForcedWaveTime + minForceInterval) return;

        lastForcedWaveTime = Time.time; //Yeah, it got annoying to send a wave RIGHT after the next spawned, so I added a safety catch.
        //Equal to the cooldown between wave sends.
        LaunchNextWave(true);
    }

    // Same as ForceNextWave. Nothing calls it right now.
    public void RequestNextWave()
    {
        if (Time.time < lastForcedWaveTime + minForceInterval) return;

        lastForcedWaveTime = Time.time;
        ForceNextWave();
    }

    // Starts the wave at nextWaveIndex: reserves its enemies in the counters, shows the "incoming"
    // banner, pays the early-call reward, and starts one coroutine per spawn group.
    // calledEarly is true when the player sent the wave instead of waiting out the countdown.
    private void LaunchNextWave(bool calledEarly)
    {
        if (!CanRun || anySpawnFailed || nextWaveIndex >= gameData.waves.Length) return;

        WaveData wave = gameData.waves[nextWaveIndex];
        // Seconds the player skipped: what's left of the countdown, or the wave's whole delay
        // if the countdown hadn't started because the latest wave was still spawning.
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

        // Count the wave's enemies up front, so the HUD shows the full total before they spawn.
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
            CurrencyManager.instance.AddCurrency(Mathf.RoundToInt(secondsLeft * 3f)); //allows for decimal values, but rounds. That way I can adjust much more precisely
        }

        if (wave.spawns != null)
        {
            foreach (SpawnGroup group in wave.spawns)
            {
                StartCoroutine(SpawnGroupRoutine(group, progress));
            }
        }
        // A wave with no spawn groups is complete as soon as it launches.
        CheckWaveCompletion(progress);
    }

    // Spawns one group from the JSON: checks its enemy and path first, waits delayBeforeGroup,
    // then spawns count enemies spawnInterval seconds apart. The finally block always runs,
    // so the counters stay right even if the group stops partway.
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

            // pathID 0 means the default path; any other ID must be in the path registry.
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

                // Both health scripts scale health and speed for this wave and the difficulty, if the prefab has them.
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

                // The tracker calls EnemyRemoved when this enemy dies or reaches the fence.
                // Prefabs without one get one added.
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

    // Called through an enemy's WaveEnemyTracker when it dies or reaches the fence.
    private void EnemyRemoved(WaveProgress wave)
    {
        wave.enemiesAlive--;
        wave.enemiesRemaining--;
        activeEnemyCount--;
        UpdateEnemyCountUI();
        CheckWaveCompletion(wave);
    }

    // Marks a wave complete once it's done spawning and every enemy from it is gone, then shows
    // the banner. Only fires once per wave. The next wave's countdown doesn't wait for this.
    private void CheckWaveCompletion(WaveProgress wave)
    {
        if (gameStopped || baseHealth == null || !baseHealth.IsAlive || wave.completed ||
            wave.spawnFailed || !wave.spawningFinished || wave.enemiesRemaining != 0) return;

        wave.completed = true;
        Debug.Log($"Wave {wave.waveNumber} complete!", this);
        ShowNotification($"Wave {wave.waveNumber} complete");
    }

    // Wins once every wave has launched, no group is still spawning, and no enemy is left.
    private void TryWinGame()
    {
        if (!CanRun || anySpawnFailed || nextWaveIndex < gameData.waves.Length ||
            pendingSpawnGroups != 0 || activeEnemyCount != 0) return;

        gameStopped = true;
        gameResultManager.WinGame();
    }

    // HUD enemy counter. Includes enemies that haven't spawned yet.
    private void UpdateEnemyCountUI()
    {
        if (enemiesRemainingText != null)
            enemiesRemainingText.text = $"Enemies remaining: {activeEnemyCount}";
    }

    // HUD wave label, e.g. "Wave 12 / 50".
    private void UpdateWaveUI()
    {
        int totalWaves = gameData != null && gameData.waves != null ? gameData.waves.Length : 0;
        if (currentWaveText != null)
            currentWaveText.text = $"Wave {currentWaveNumber} / {totalWaves}";
    }

    // Shows a banner message, replacing any banner already up.
    private void ShowNotification(string message)
    {
        if (waveNotificationText == null || !isActiveAndEnabled) return;
        if (notificationRoutine != null) StopCoroutine(notificationRoutine);
        notificationRoutine = StartCoroutine(ShowWaveNotification(message));
    }

    // Shows the banner for notificationDuriation seconds, then hides it.
    private IEnumerator ShowWaveNotification(string message)
    {
        waveNotificationText.text = message;
        if (waveNotificationBackground != null) waveNotificationBackground.SetActive(true);
        yield return new WaitForSeconds(Mathf.Max(0.01f, notificationDuriation));
        waveNotificationText.text = "";
        if (waveNotificationBackground != null) waveNotificationBackground.SetActive(false);
        notificationRoutine = null;
    }

    // Stop spawning, and clear the static reference so the next scene's spawner can take its place.
    private void OnDestroy()
    {
        gameStopped = true;
        if (Instance == this) Instance = null;
    }
}
