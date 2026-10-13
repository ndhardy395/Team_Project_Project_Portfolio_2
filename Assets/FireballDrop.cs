using UnityEngine;

public class FireballMovement : MonoBehaviour
{
    public float fallSpeed = 15f;
    public float destroyYLevel = -5f;
    public float hitRadius = 3f;

    void Update()
    {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        // Find the enemy using the "Enemy" tag
        GameObject enemyObj = GameObject.FindWithTag("Enemy");
        if (enemyObj != null)
        {
            float dist = Vector3.Distance(transform.position, enemyObj.transform.position);

            if (dist < hitRadius)
            {
                Debug.Log("HIT REGISTERED!");
                Destroy(enemyObj);
                Destroy(gameObject);
                return;
            }
        }

        if (transform.position.y < destroyYLevel)
        {
            Destroy(gameObject);
        }
    }
}