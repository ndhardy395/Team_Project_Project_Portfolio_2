using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FireballMovement : MonoBehaviour
{
    public float fallSpeed = 15f;
    public float destroyYLevel = -5f;
    public float hitRadius = 3f;
    public float damage = 50f;

    public LayerMask groundLayers;
    public LayerMask enemyLayers;

    public GameObject impacteffect;
    public float effectLifeTime = 3f;

    private bool hasExploded;
    private Vector3 targetPosition;
    private Transform projectileRoot;
    private bool hasTarget;

    public void SetTarget(Vector3 position, Transform root)
    {
        targetPosition = position;
        projectileRoot = root;
        hasTarget = true;
    }

    void Update()
    {

        if (hasExploded)
        {
            return;
        }

        if (hasTarget)
        {
        projectileRoot.position = Vector3.MoveTowards(projectileRoot.position, targetPosition, fallSpeed * Time.deltaTime);

            if(Vector3.Distance(projectileRoot.position, targetPosition) <= 0.01f)
            {
                Explode(targetPosition);
            }

            return;
        }
        float moveDistance = fallSpeed * Time.deltaTime;

        // checks the entire movement distance to avoid skippping the ground

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, moveDistance, groundLayers, QueryTriggerInteraction.Ignore))
        {
            Explode(hit.point);
            return;
        }

        transform.position += Vector3.down * moveDistance;

        if (transform.position.y < destroyYLevel)
        {
            Destroy(gameObject);
        }
    }

    private void Explode(Vector3 impactPosition)
    {
        hasExploded = true;

        if (impacteffect != null)

        {
            GameObject effect = Instantiate(impacteffect, impactPosition, Quaternion.identity);
            Destroy(effect, effectLifeTime);
        }

        Collider[] hits = Physics.OverlapSphere(impactPosition, hitRadius, enemyLayers, QueryTriggerInteraction.Ignore);

        // Enemies with several collider should take damage only once.
        HashSet<EnemyHealthDisplay> damgedEnemies = new HashSet<EnemyHealthDisplay>();

        foreach (Collider hit in hits)
        {
            EnemyHealthDisplay enemy = hit.GetComponentInParent<EnemyHealthDisplay>();

            if (enemy != null && !enemy.IsDead && damgedEnemies.Add(enemy))
            {
                enemy.TakeDamage(damage);
            }
        }
        Destroy(projectileRoot != null ? projectileRoot.gameObject : gameObject);

    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }

}



