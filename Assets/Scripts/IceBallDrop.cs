using System.Collections.Generic;
using UnityEngine;

public class IceBallDrop : MonoBehaviour
{
    public float fallSpeed = 15f;
    public float destroyYLevel = -5f;
    public float hitRadius = 5f;
    public float damage = 50f;

    public float freezeDuration = 3f;

    public LayerMask groundLayers;
    public LayerMask enemyLayers;

    public GameObject impacteffect;
    public float effectLifeTime = 3f;

    private bool hasFreeze;
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

        if (hasFreeze)
        {
            return;
        }

        if (hasTarget)
        {
            projectileRoot.position = Vector3.MoveTowards(projectileRoot.position, targetPosition, fallSpeed * Time.deltaTime);

            if (Vector3.Distance(projectileRoot.position, targetPosition) <= 0.01f)
            {
                Freeze(targetPosition);
            }

            return;
        }
        float moveDistance = fallSpeed * Time.deltaTime;

        // checks the entire movement distance to avoid skippping the ground

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, moveDistance, groundLayers, QueryTriggerInteraction.Ignore))
        {
            Freeze(hit.point);
            return;
        }

        transform.position += Vector3.down * moveDistance;

        if (transform.position.y < destroyYLevel)
        {
            Destroy(gameObject);
        }
    }

    private void Freeze(Vector3 impactPosition)
    {
        hasFreeze = true;

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

            if (enemy == null || enemy.IsDead || !damgedEnemies.Add(enemy))
            {
                continue;
            }

            // Damage first: enemies with low health die normally
            enemy.TakeDamage(damage);

            // only surviving enemies get frozen.
            if (enemy != null && !enemy.IsDead)
            {
                EnemyMovement movement = enemy.GetComponent<EnemyMovement>();

                if (movement != null)
                {
                    movement.freezeFor(freezeDuration);
                }
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

