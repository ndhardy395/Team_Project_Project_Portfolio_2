using UnityEngine;

// Test enemy: walks straight along its forward direction, despawns when it
// has walked far enough (made it past the gun) or when its health hits 0.
public class Enemy : MonoBehaviour
{
    public float health = 150f;        // public so you can watch it drop in the Inspector during Play
    public float speed = 50f;
    float distanceLeft;

    public void Init(float moveSpeed, float startHealth, float travelDistance)
    {
        speed = moveSpeed;
        health = startHealth;
        distanceLeft = travelDistance;//probably redundant, but may need some form of this later - when we have more information
    }

    void Update()
    {
        float step = speed * Time.deltaTime;
        transform.position += transform.forward * step;
        distanceLeft -= step;

        if (distanceLeft <= 0f)
            Destroy(gameObject);        // made it past the gun
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
            Destroy(gameObject);        // the gun got it
    }
}
