using UnityEngine;
using TMPro;

public class BaseHealth : MonoBehaviour
{

    [SerializeField,Min(1)] private int maxHealth = 20;
    [SerializeField] private GameResultManager gameResultManager;
    [SerializeField] private TMP_Text baseHealthText;

    private int currentHealth;

    private bool isDestroyed;

    public bool IsAlive => !isDestroyed;

    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = Mathf.Max(1, maxHealth);
        isDestroyed = false;
       UpdateHealthUI();
    }

    public void UpdateHealthUI()
    {
        if(baseHealthText != null)
        {
            baseHealthText.text = $"Lives: {currentHealth}";
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.LogWarning(
    $"Base taking {damage} damage. Health before: {currentHealth}\n" + new System.Diagnostics.StackTrace(true),this);

        if (isDestroyed || damage <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0,currentHealth - damage);
        UpdateHealthUI();

        if(currentHealth == 0)
        {
            isDestroyed = true;

            Debug.Log($"Base reached Zero. Calling loseGame on " + $"{gameResultManager.name}" , gameResultManager);
            if(gameResultManager != null)
            {
                gameResultManager.LoseGame();
            }
            else
            {
                Debug.LogError("Assign Game Result Manager on BaseHealth.", this);
            }
           
        }
        
    }
}
