using System.Collections.Generic;
using UnityEngine;

// What a MineTower throws. It arcs onto its spot on the path and waits there. When an enemy
// comes within triggerRadius it blows up, hurting every enemy within blastRadius.
// No Collider or Rigidbody needed: like the Gun, it looks for enemies with an overlap sphere.
public class Mine : MonoBehaviour
{
    [Header("Throw (from the tower to its spot on the path)")]
    public float speed = 12f;       // units per second
    public float arcHeight = 2f;    // how high above the straight line the throw peaks

    [Header("On the path")]
    public float armDelay = 0.3f;   // seconds after landing before it can go off

    [Header("Effects (optional)")]
    public GameObject explosionPrefab;  // spawned where it goes off; give it Stop Action: Destroy so it cleans itself up

    // Every mine on the path or in the air, so towers can space theirs out
    // and an upgraded tower can take over the old tier's mines.
    static readonly List<Mine> all = new List<Mine>();
    public static IReadOnlyList<Mine> All => all;

    public Vector3 LandingPoint { get; private set; }   // where it sits, or will once it lands
    public Vector3 DroppedFrom { get; private set; }    // the tower's position; an upgrade puts the next tier right there

    // These come from the tower, so an upgraded tower's mines hit harder.
    float damage;
    float blastRadius;
    float triggerRadius;
    LayerMask enemyLayer;

    Vector3 launchPoint;
    float flightTime;
    float timeFlown;
    bool landed;
    float armedAt;                  // Time.time it can go off

    static readonly Collider[] hits = new Collider[64];                          // reused buffer, no per-check allocation
    static readonly HashSet<IDamageable> damaged = new HashSet<IDamageable>();   // so an enemy with two colliders isn't hit twice

    void Awake()
    {
        // Until a tower launches it, it lands right where it is.
        launchPoint = transform.position;
        LandingPoint = transform.position;
    }

    void OnEnable() => all.Add(this);
    void OnDisable() => all.Remove(this);

    public void Launch(MineTower tower, Vector3 spot)
    {
        TakeStatsFrom(tower);
        DroppedFrom = tower.transform.position;
        launchPoint = transform.position;
        LandingPoint = spot;
        flightTime = Mathf.Max(Vector3.Distance(launchPoint, spot) / speed, 0.05f);
    }

    public void TakeStatsFrom(MineTower tower)
    {
        damage = tower.damage;
        blastRadius = tower.blastRadius;
        triggerRadius = tower.triggerRadius;
        enemyLayer = tower.enemyLayer;
    }

    void Update()
    {
        if (!landed)
        {
            Fly();
            return;
        }

        if (Time.time < armedAt) return;

        IDamageable enemy = EnemyInReach();
        if (enemy != null) Explode(enemy);
    }

    void Fly()
    {
        timeFlown += Time.deltaTime;
        float t = flightTime > 0f ? Mathf.Clamp01(timeFlown / flightTime) : 1f;   // 0 at launch, 1 on landing

        // Straight line from the tower to the spot, plus a hump that's 0 at both ends and arcHeight in the middle.
        transform.position = Vector3.Lerp(launchPoint, LandingPoint, t) + Vector3.up * (4f * arcHeight * t * (1f - t));

        if (t >= 1f)
        {
            landed = true;
            armedAt = Time.time + armDelay;
        }
    }

    // The first live enemy close enough to set it off, or null.
    IDamageable EnemyInReach()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, triggerRadius, hits, enemyLayer);
        for (int i = 0; i < count; ++i)
        {
            IDamageable enemy = hits[i].GetComponentInParent<IDamageable>();
            if (enemy != null && !enemy.IsDead) return enemy;   // a dying enemy keeps its collider; don't waste the mine on it
        }
        return null;
    }

    // Damages every enemy within blastRadius, and always the one that set it off.
    void Explode(IDamageable trigger)
    {
        damaged.Clear();
        int count = Physics.OverlapSphereNonAlloc(transform.position, blastRadius, hits, enemyLayer);
        for (int i = 0; i < count; ++i)
        {
            IDamageable enemy = hits[i].GetComponentInParent<IDamageable>();
            if (enemy != null && !enemy.IsDead && damaged.Add(enemy))
                enemy.TakeDamage(damage);
        }

        // A blast smaller than the trigger zone could miss the enemy that stepped on it.
        if (!trigger.IsDead && damaged.Add(trigger))
            trigger.TakeDamage(damage);

        // Spawned on its own, not as a child, so it keeps playing after the mine is gone.
        if (explosionPrefab != null) Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    // Scene view, with a mine selected in Play mode: trigger zone (yellow) and blast (red).
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, blastRadius);
    }
}
