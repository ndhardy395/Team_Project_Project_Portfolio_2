using System.Collections.Generic;
using UnityEngine;

// Put this on the bullet prefab. Its blue (Z) arrow must point out the nose.
// No Collider or Rigidbody needed: it raycasts the distance it's about to move
// each frame, so a fast bullet can't skip through a thin capsule between frames.
public class Bullet : MonoBehaviour
{
    public float speed = 80f;       // units per second
    public float lifetime = 2f;     // seconds before a missed bullet cleans itself up
    public bool homing = true;      // turn off to see why straight shots miss moving targets

    [Header("Mortar (leave at 0 for a straight bullet)")]
    public float arcHeight = 0f;    // how high above the straight line the shot peaks; it flies over enemies and only hits where it lands

    [Header("Effects (optional)")]
    public GameObject explosionPrefab;  // spawned where the shot lands; give it Stop Action: Destroy so it cleans itself up
    public ParticleSystem exhaust;  // a child effect like rocket exhaust; let go of on landing so its smoke fades instead of vanishing

    float damage;
    float splashRadius;             // comes from the Gun, so an upgrade can grow it
    LayerMask hitMask;
    Transform target;

    // Arc flight
    Vector3 launchPoint;
    Vector3 landingPoint;           // follows the target while it's alive, then stays where it was last seen
    float flightTime;
    float timeFlown;

    static readonly Collider[] splashHits = new Collider[64];                     // reused buffer, no per-explosion allocation
    static readonly HashSet<IDamageable> splashDamaged = new HashSet<IDamageable>();  // so an enemy with two colliders isn't hit twice

    public void Init(float dmg, float splash, LayerMask mask, Transform aimTarget)
    {
        damage = dmg;
        splashRadius = splash;
        hitMask = mask;
        target = aimTarget;

        launchPoint = transform.position;
        landingPoint = target != null ? target.position : transform.position + transform.forward * 10f;
        flightTime = Mathf.Max(Vector3.Distance(launchPoint, landingPoint) / speed, 0.05f);

        // Point the nose where the arc heads at launch (the hump climbs at 4 x arcHeight to start),
        // not whichever way the spawn point was turned.
        if (arcHeight > 0f) transform.forward = (landingPoint - launchPoint) + Vector3.up * (4f * arcHeight);

        // An arc always lands (and cleans up in Impact), so only straight shots need the timeout.
        if (arcHeight <= 0f) Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (arcHeight > 0f)
        {
            FlyArc();
            return;
        }

        if (homing && target != null)
            transform.forward = (target.position - transform.position).normalized;

        float step = speed * Time.deltaTime;

        // `out` is like passing a reference the function must fill in.
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, step, hitMask))
        {
            Impact(hit.point, hit.collider.GetComponentInParent<IDamageable>());
            return;
        }

        transform.position += transform.forward * step;
    }

    void FlyArc()
    {
        // Keep landing on the enemy while it's alive; if another tower kills it mid-air, land where it was.
        if (homing && target != null) landingPoint = target.position;

        timeFlown += Time.deltaTime;
        float t = Mathf.Clamp01(timeFlown / flightTime);   // 0 at launch, 1 on landing

        // Straight line from launch to landing, plus a hump that's 0 at both ends and arcHeight in the middle.
        Vector3 next = Vector3.Lerp(launchPoint, landingPoint, t) + Vector3.up * (4f * arcHeight * t * (1f - t));
        if (next != transform.position)
            transform.forward = (next - transform.position).normalized;   // nose follows the curve
        transform.position = next;

        // Landing counts as hitting the target if it's still alive, so an arc with no splash still does damage.
        if (t >= 1f) Impact(landingPoint, target != null ? target.GetComponentInParent<IDamageable>() : null);
    }

    // With no splash, damages what was hit; with splash, damages every enemy within splashRadius of the point.
    void Impact(Vector3 point, IDamageable directHit)
    {
        if (splashRadius > 0f)
        {
            splashDamaged.Clear();
            int count = Physics.OverlapSphereNonAlloc(point, splashRadius, splashHits, hitMask);
            for (int i = 0; i < count; ++i)
            {
                IDamageable enemy = splashHits[i].GetComponentInParent<IDamageable>();
                if (enemy != null && !enemy.IsDead && splashDamaged.Add(enemy))
                    enemy.TakeDamage(damage);
            }
        }
        else if (directHit != null)
        {
            directHit.TakeDamage(damage);
        }

        // Spawned on its own, not as a child, so it keeps playing after this shot is destroyed below.
        if (explosionPrefab != null) Instantiate(explosionPrefab, point, Quaternion.identity);

        if (exhaust != null)
        {
            // Destroying the rocket would delete its smoke mid-air, so the exhaust leaves first and fades out on its own.
            Vector3 ownScale = exhaust.transform.localScale;
            exhaust.transform.SetParent(null, true);
            exhaust.transform.localScale = ownScale;   // with Scaling Mode: Local, keeps the puffs the same size
            ParticleSystem.MainModule main = exhaust.main;
            main.stopAction = ParticleSystemStopAction.Destroy;   // removes itself once the last puff is gone
            exhaust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        Destroy(gameObject);
    }
}
