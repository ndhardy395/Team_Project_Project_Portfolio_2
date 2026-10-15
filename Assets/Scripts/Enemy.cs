using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 50f;
    private float currentHealth;
    public float moveSpeed = 5f;
    public int currencyReward = 10;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth; //sets max health once
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fireball")) //Fireball SPECIFICALLY. This will change
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
        Destroy(gameObject); //boom, gone!
    }
}
