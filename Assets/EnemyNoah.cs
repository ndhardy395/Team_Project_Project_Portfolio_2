using UnityEngine;

// Test enemy: walks straight along its forward direction, despawns when it
// has walked far enough (made it past the gun) or when its health hits 0.
public class EnemyNoah : MonoBehaviour, IDamageable
{
    public float health = 150f;        // public so you can watch it drop in the Inspector during Play
    public float speed = 50f;
    float distanceLeft = Mathf.Infinity;   // if Init is never called (enemy placed by hand), don't despawn on frame 1

    public void Init(float moveSpeed, float startHealth, float travelDistance)
    {
        speed = moveSpeed;
        health = startHealth;
        distanceLeft = travelDistance;
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
