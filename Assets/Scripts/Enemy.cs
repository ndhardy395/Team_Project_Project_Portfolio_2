using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Enemy Identity")]
    [SerializeField] private string enemyId = "Spider";

    [Header("Base Stats (Wave 1, Medium 1.0)")]
    [SerializeField] private float baseMaxHealth = 50f;
    [SerializeField] private float baseMoveSpeed = 5f;
    public int currencyReward = 10;

    [Header("Scaling Configuration")]
    [SerializeField] private bool isBoss = false;
    [Tooltip("Manual health override. Set to 0 to use scaling formula instead.")]
    [SerializeField] private float overrideHealth = 0f;
    [Tooltip("Manual speed override. Set to 0 to use difficulty modifier instead.")]
    [SerializeField] private float overrideSpeed = 0f;

    [Header("Runtime Stats")]
    private float currentHealth;
    private float moveSpeed;
    public float maxHealth { get; private set; }

    public bool IsDead => currentHealth <= 0f;

    [Header("Scaling Configuration")]
    [SerializeField] private int firstWaveUnlocked = 1;
    [SerializeField] private float hpPerWave = 4f;

    // Called by your Spawner/Wave Manager immediately after instantiating the enemy
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

        // 5. Update Health Display UI
        EnemyHealthDisplay healthDisplay = GetComponent<EnemyHealthDisplay>();
        if (healthDisplay != null)
        {
            healthDisplay.Initialize(waveNumber, healthMultiplier, speedMultiplier);
        }
        else
        {
            UpdateHealthDisplay();
        }
    }

    private void UpdateHealthDisplay()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fireball"))
        {
            TakeDamage(10f);
            Destroy(other.gameObject);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}