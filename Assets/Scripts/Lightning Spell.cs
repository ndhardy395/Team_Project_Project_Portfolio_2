using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Net.NetworkInformation;

public class LightningSpell : MonoBehaviour
{
    public float hitRadius = 5f;
    public float damagePerStrick = 25f;

    public int strikeCount = 3;

    public float strikeIntreval = 0.5f;

    public LayerMask enemyLayers;

    public GameObject strikeEffect;
    public float effectLifeTime = 1f;

    private bool hasStarted;

    public void SetTarget(Vector3 position, Transform root)
    {
        if (hasStarted)
        {
            return;
        }

        hasStarted = true;

        Transform spellRoot = root != null ? root : transform;

        // Lightning happens at the target instead of falling

        spellRoot.position = position;

        StartCoroutine(StrikeRoutine(position, spellRoot));
    }

    private IEnumerator StrikeRoutine(Vector3 targetPosition, Transform spellRoot)
    {
        int totalStrike = Mathf.Max(1, strikeCount);

        for (int i = 0; i < totalStrike; i++)
        {
            Strike(targetPosition);

            if(i < totalStrike - 1)
            {
                yield return new WaitForSeconds(Mathf.Max(0.01f, strikeIntreval));
            }
        }

        //remove all spell only after all strikes finished
        Destroy(spellRoot.gameObject);
    }


    private void Strike(Vector3 targetPosition)
    {
        if (strikeEffect != null)
        {
            GameObject effect = Instantiate(strikeEffect, targetPosition, Quaternion.identity);

            // Clean up this strike's visual independently
            Destroy(effect, effectLifeTime);
        }
     

        // Check again each strike becasue enemies can move

        Collider[] hits = Physics.OverlapSphere(targetPosition, hitRadius, enemyLayers, QueryTriggerInteraction.Ignore);

        HashSet<EnemyHealthDisplay> damageEnemies = new HashSet<EnemyHealthDisplay>();

        foreach (Collider hit in hits)
        {
            EnemyHealthDisplay enemy = hit.GetComponentInParent<EnemyHealthDisplay>();

            if (enemy == null || enemy.IsDead || !damageEnemies.Add(enemy))
            {
                continue;
            }

            enemy.TakeDamage(damagePerStrick);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
