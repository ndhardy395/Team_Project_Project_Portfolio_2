using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthDisplay : MonoBehaviour, IDamageable
{
    [Header("Base Stats (Wave 1, Medium 1.0)")]
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float baseMoveSpeed = 5f;
    public float moveSpeed = 5f; // Kept if other scripts reference it here

    [Header("Scaling Configuration")]
    [SerializeField] private bool isBoss = false;
    [Tooltip("Manual health override for bosses. Set to 0 to use scaling formula instead.")]
    [SerializeField] private float overrideHealth = 0f;
    [SerializeField] private int firstWaveUnlocked = 1;
    [SerializeField] private float hpPerWave = 6f;

    [Header("UI & References")]
    public TMP_Text enemyHealthText;
    public GameObject healthBar;
    public Slider healthSlider;
    public string enemyName = "Big Imp";

    [TextArea(2, 5)]
    public string discription = "A large enemy with a high health.";

    [Header("Extra scripts to stop on death")]
    public Behaviour[] scriptsToStopOnDeath;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    public bool useDeathAnimation = true;
    public GameObject deathEffect;
    public SfxEvent deathSound;     //SFX upon death
    public float despawnDelay = 2.5f;

    private float maxHealth;
    private float currentHealth;
    private Animator animator;
    private PathFollowTest pathFollow;
    private EnemyMovement enemyMovement;
    private bool isDead;

    // Called by WaveSpawner right after instantiation
    public void Initialize(int waveNumber, float healthMultiplier, float speedMultiplier)
    {
        // 3. Calculate Final Health (Piecewise Linear using Inspector values)
        if (isBoss && overrideHealth > 0)
        {
            maxHealth = overrideHealth;
        }
        else
        {
            int wavesActive = Mathf.Max(0, waveNumber - firstWaveUnlocked);
            maxHealth = baseMaxHealth + (wavesActive * hpPerWave);
        }

        // Apply difficulty multiplier
        maxHealth *= healthMultiplier;
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = maxHealth;

        // 4. Apply Speed and Push to Movement
        moveSpeed = baseMoveSpeed * speedMultiplier;
        EnemyMovement movement = GetComponent<EnemyMovement>();
        if (movement != null)
        {
            movement.speed = moveSpeed;
        }

        // 5. Update Health Display UI directly
        UpdateHealthDisplay();
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        pathFollow = GetComponent<PathFollowTest>();
        enemyMovement = GetComponent<EnemyMovement>();

        // Fallback if Initialize() wasn't called (e.g. dropped directly in scene for testing)
        if (maxHealth <= 0f)
        {
            maxHealth = Mathf.Max(1f, baseMaxHealth);
            currentHealth = maxHealth;
        }

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }

        UpdateHealthDisplay();

        if (healthBar != null)
        {
            healthBar.SetActive(false);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - damage);

        if (healthBar != null)
        {
            healthBar.SetActive(true);
        }

        UpdateHealthDisplay();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void UpdateHealthDisplay()
    {
        if (healthSlider != null && maxHealth > 0f)
        {
            healthSlider.value = currentHealth / maxHealth;
        }

        if (enemyHealthText != null)
        {
            enemyHealthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Sfx.Play(deathSound, transform.position); //Play SFX

        if (scriptsToStopOnDeath != null)
        {
            foreach (Behaviour scripts in scriptsToStopOnDeath)
            {
                if (scripts != null)
                {
                    scripts.enabled = false;
                }
            }
        }

        WaveEnemyTracker tracker = GetComponentInParent<WaveEnemyTracker>();

        if (tracker != null)
        {
            tracker.ReportRemoved();
        }
        else
        {
            Debug.LogError($"No wave tracker found for {name}", this);
        }

        if (healthBar != null)
        {
            healthBar.SetActive(false);
        }

        if (pathFollow != null)
        {
            pathFollow.enabled = false;
        }

        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position + Vector3.up, Quaternion.identity);
            Destroy(effect, 5f);
        }

        if (useDeathAnimation && animator != null)
        {
            animator.SetTrigger("Die");
        }

        if (enemyMovement != null)
        {
            enemyMovement.enabled = false;
        }

        Destroy(gameObject, useDeathAnimation ? despawnDelay : 0f);

        EnemyDropCurrency currencyDrop = GetComponentInChildren<EnemyDropCurrency>();

        if (currencyDrop != null)
        {
            currencyDrop.DropItem();
        }
    }

    [ContextMenu("Test/Deal 50 damage")]
    private void TestDamage()
    {
        if (Application.isPlaying)
        {
            TakeDamage(50f);
        }
    }
}