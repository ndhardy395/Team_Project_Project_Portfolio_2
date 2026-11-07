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
    public float penetration = 0.5f;  // how far a straight shot keeps flying into the enemy after touching its collider, so the bullet and its smoke end inside the model instead of at the collider's edge

    [Header("Mortar (leave at 0 for a straight bullet)")]
    public float arcHeight = 0f;    // how high above the straight line the shot peaks; it flies over enemies and only hits where it lands

    [Header("Effects (optional)")]
    public GameObject explosionPrefab;  // spawned where the shot lands; give it Stop Action: Destroy so it cleans itself up
    public ParticleSystem exhaust;  // a child effect like rocket exhaust; let go of on landing so its smoke fades instead of vanishing

    float damage;
    float splashRadius;             // comes from the Gun, so an upgrade can grow it
    LayerMask hitMask;
    Transform target;
    IDamageable targetHealth;       // so the shot can tell when its target starts dying
    Collider targetBody;            // straight shots aim at the middle of this, not at the enemy's pivot (often its feet)

    // Arc flight
    Vector3 launchPoint;
    Vector3 landingPoint;           // follows the target while it's alive, then stays where it was last seen
    float flightTime;
    float timeFlown;

    // Straight flight
    float sinkLeft = -1f;           // below 0 while still flying; after a hit, how much further it goes into the enemy
    Vector3 exhaustFrom;            // where the exhaust was at the start of this frame, i.e. how far its smoke has been drawn

    static readonly RaycastHit[] rayHits = new RaycastHit[16];                    // reused buffer for the flight raycast
    static readonly Collider[] splashHits = new Collider[64];                     // reused buffer, no per-explosion allocation
    static readonly HashSet<IDamageable> splashDamaged = new HashSet<IDamageable>();  // so an enemy with two colliders isn't hit twice

    public void Init(float dmg, float splash, LayerMask mask, Transform aimTarget)
    {
        damage = dmg;
        splashRadius = splash;
        hitMask = mask;
        target = aimTarget;
        targetHealth = target != null ? target.GetComponentInParent<IDamageable>() : null;
        targetBody = target != null ? target.GetComponent<Collider>() : null;   // the Gun hands over the transform of the collider it found

        launchPoint = transform.position;
        landingPoint = target != null ? target.position : transform.position + transform.forward * 10f;
        flightTime = Mathf.Max(Vector3.Distance(launchPoint, landingPoint) / speed, 0.05f);

        // Point the nose where the arc heads at launch (the hump climbs at 4 x arcHeight to start),
        // not whichever way the spawn point was turned.
        if (arcHeight > 0f) transform.forward = (landingPoint - launchPoint) + Vector3.up * (4f * arcHeight);

        // An arc always lands (and cleans up in Finish), so only straight shots need the timeout.
        if (arcHeight <= 0f) Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // The particle system has drawn smoke up to here. If the shot ends this frame, Finish fills in the rest.
        if (exhaust != null) exhaustFrom = exhaust.transform.position;

        if (arcHeight > 0f)
        {
            FlyArc();
            return;
        }

        float step = speed * Time.deltaTime;

        if (sinkLeft < 0f)
        {
            // Stop steering once the target is dying, or the shot would circle its body (it flies through bodies, below).
            if (homing && target != null && (targetHealth == null || !targetHealth.IsDead))
            {
                // The middle of its collider is always inside what the raycast can hit, so the shot can't slip under or past it.
                Vector3 aimAt = targetBody != null && targetBody.enabled ? targetBody.bounds.center : target.position;
                transform.forward = (aimAt - transform.position).normalized;
            }

            // `out` is like passing a reference the function must fill in.
            if (FirstLiveHit(step, out RaycastHit hit))
            {
                Impact(hit.point, hit.collider.GetComponentInParent<IDamageable>());   // damage lands the moment it touches
                sinkLeft = Mathf.Max(hit.distance + penetration, 0f);   // then it carries on a little way into the enemy before it's removed
            }
        }

        // Already hit: no more homing or raycasts (it can't hit twice), just fly straight in and go away.
        if (sinkLeft >= 0f)
        {
            float move = Mathf.Min(step, sinkLeft);
            transform.position += transform.forward * move;
            sinkLeft -= move;
            if (sinkLeft <= 0f) Finish();
            return;
        }

        transform.position += transform.forward * step;
    }

    // The closest thing in this frame's path, skipping enemies that are already dead. Those stay around for
    // their death animation with their collider still on, and would otherwise soak up shots for no damage.
    bool FirstLiveHit(float distance, out RaycastHit closest)
    {
        closest = default;
        bool found = false;
        int count = Physics.RaycastNonAlloc(transform.position, transform.forward, rayHits, distance, hitMask);
        for (int i = 0; i < count; ++i)   // these come back in no particular order
        {
            IDamageable enemy = rayHits[i].collider.GetComponentInParent<IDamageable>();
            if (enemy == null || enemy.IsDead)
            {
                continue;
            }// update were only living enemies can consume shot
          
            if (!found || rayHits[i].distance < closest.distance)
            {
                closest = rayHits[i];
                found = true;
            }
        }
        return found;
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
        if (t >= 1f)
        {
            Impact(landingPoint, target != null ? target.GetComponentInParent<IDamageable>() : null);
            Finish();
        }
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

        // Spawned on its own, not as a child, so it keeps playing after this shot is destroyed.
        if (explosionPrefab != null) Instantiate(explosionPrefab, point, Quaternion.identity);
    }

    // Removes the shot once it's done flying.
    void Finish()
    {
        if (exhaust != null)
        {
            FillExhaust();

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

    // A trail emitted by distance (Rate over Distance) only gets drawn when the particle system updates,
    // which happens after this script moves. On the frame the shot is removed that update never comes,
    // so without this the smoke would stop where the exhaust was at the start of the frame.
    void FillExhaust()
    {
        ParticleSystem.EmissionModule emission = exhaust.emission;
        // A Local-space effect rides along with the shot, so there's no trail left behind to finish.
        if (!emission.enabled || exhaust.main.simulationSpace != ParticleSystemSimulationSpace.World) return;

        Vector3 to = exhaust.transform.position;
        int count = Mathf.FloorToInt(Vector3.Distance(exhaustFrom, to) * emission.rateOverDistanceMultiplier);
        ParticleSystem.EmitParams puff = new ParticleSystem.EmitParams();   // anything not set here comes from the effect's own settings
        for (int i = 1; i <= count; ++i)
        {
            puff.position = Vector3.Lerp(exhaustFrom, to, (float)i / count);
            exhaust.Emit(puff, 1);
        }
    }
}
