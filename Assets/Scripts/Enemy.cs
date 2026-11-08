using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Base Stats (Wave 1, Medium 1.0)")]
    [SerializeField] private float baseMaxHealth = 50f;
    [SerializeField] private float baseMoveSpeed = 5f;
    public int currencyReward = 10;

    [Header("Scaling Configuration")]
    [SerializeField] private bool isBoss = false;
    [Tooltip("Manual health override. Set to 0 to use exponential scaling formula instead.")]
    [SerializeField] private float overrideHealth = 0f;
    [Tooltip("Manual speed override. Set to 0 to use difficulty modifier instead.")]
    [SerializeField] private float overrideSpeed = 0f;

    [Header("Runtime Stats")]
    private float currentHealth;
    private float moveSpeed;
    public float maxHealth { get; private set; }

    public bool IsDead => currentHealth <= 0f;

    // Called by your Spawner/Wave Manager immediately after instantiating the enemy
    public void Initialize(int waveNumber, float healthMultiplier, float speedMultiplier)
    {
        if (isBoss && overrideHealth > 0)
        {
            maxHealth = overrideHealth;
        }
        else
        {
            // Exponential health scaling: Base * (1 + growthRate)^(wave - 1)
            float healthGrowthRate = 0.10f;
            maxHealth = baseMaxHealth * Mathf.Pow(1f + healthGrowthRate, waveNumber - 1); //uses base health of each enemy.
        }

        // Apply difficulty multiplier to health and speed
        maxHealth *= healthMultiplier;
        currentHealth = maxHealth;

        moveSpeed *= speedMultiplier;

        // Speed calculation
        if (isBoss && overrideSpeed > 0)
        {
            moveSpeed = overrideSpeed;
        }
        else
        {
            moveSpeed = baseMoveSpeed * speedMultiplier;
        }
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