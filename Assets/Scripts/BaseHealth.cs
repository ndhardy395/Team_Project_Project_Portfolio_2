using UnityEngine;
using TMPro;

public class BaseHealth : MonoBehaviour
{

    [SerializeField,Min(1)] private int maxHealth = 100;
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
            baseHealthText.text = $"Base Health: {currentHealth}";
        }
    }

    public void TakeDamage(int damage)
    {
        if(isDestroyed || damage <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0,currentHealth - damage);
        UpdateHealthUI();

        if(currentHealth == 0)
        {
            isDestroyed = true;

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
