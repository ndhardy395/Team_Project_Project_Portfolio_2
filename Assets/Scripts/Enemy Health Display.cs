using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthDisplay : MonoBehaviour, IDamageable
{
    public TMP_Text enemyHealthText;
    public float maxHealth = 100f;
    public GameObject healthBar;
    public Slider healthSlider;

    public string enemyName = "Big Imp";

    [TextArea(2, 5)]
    public string discription = "A large enemy with a high health.";

    public float CurrentHealth => currentHealth;

    public float MaxHealth => maxHealth;

    public bool IsDead => isDead;

    public bool useDeathAnimation = true;

    public GameObject deathEffect;

    public float despawnDelay = 2.5f;

    private float currentHealth;
    private Animator animator;
    private PathFollowTest pathFollow;

    private EnemyMovement enemyMovement;
    private bool isDead;



    void Awake()
    {
        animator = GetComponent<Animator>();
        pathFollow = GetComponent<PathFollowTest>();
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = maxHealth;
        enemyMovement = GetComponent<EnemyMovement>();

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }

        UpdateHealthDisplay();

        if(healthBar != null)
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
        if(healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }

        if(enemyHealthText != null)
        {
            enemyHealthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }

    private void Die() // this is the death animation for the ghost and other enemy.
    {
        isDead = true;

        if (healthBar != null)
        {
            healthBar.SetActive(false);
        }

        if (pathFollow != null)
        {
            pathFollow.enabled = false;
        }

       
        if(deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position + Vector3.up, Quaternion.identity);

            Destroy(effect, 5f);
        }
        
        if(useDeathAnimation && animator != null)
        {
            animator.SetTrigger("Die");
        }

        if(enemyMovement != null)
        {
            enemyMovement.enabled = false;
        }


        Destroy(gameObject, useDeathAnimation ? despawnDelay : 0f);
    }

    [ContextMenu("Test/Deal 50 damage")]

    private void TestDamage()
    {
        if(Application.isPlaying)
        {
            TakeDamage(50f);
        }
    }
    


}
