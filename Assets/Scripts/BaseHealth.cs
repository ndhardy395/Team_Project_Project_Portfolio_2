using UnityEngine;

public class BaseHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private GameResultManager gameResultManager;

    private int currentHealth;

    private bool isDestroyed;

    public bool IsAlive => !isDestroyed;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if(isDestroyed || damage <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0,currentHealth - damage);

        Debug.Log($"Base Health: {currentHealth}");

        if(currentHealth == 0)
        {
            isDestroyed = true;
            Debug.Log("Game Over! The base was destroyed.");
            gameResultManager.LoseGame();
        }
        
    }
}
