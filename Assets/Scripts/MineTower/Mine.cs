using System.Collections.Generic;
using UnityEngine;

// What a MineTower throws. It arcs onto its spot on the path and waits there. When an enemy
// comes within triggerRadius it blows up, hurting every enemy within blastRadius.
// No Collider or Rigidbody needed: like the Gun, it looks for enemies with an overlap sphere.
//
// The life of a mine, start to finish:
//   1. MineTower Instantiates it at the tower and calls Launch() with a spot on the path.
//   2. Fly() moves it along an arc every frame until it reaches that spot.
//   3. After landing it waits armDelay seconds, so it can't go off mid-air or the instant it lands.
//   4. Every frame after that, EnemyInReach() checks for an enemy within triggerRadius.
//   5. The first time it finds one, Explode() damages everything in blastRadius and destroys the mine.
public class Mine : MonoBehaviour
{
    [Header("Throw (from the tower to its spot on the path)")]
    public float speed = 12f;       // units per second
    public float arcHeight = 2f;    // how high above the straight line the throw peaks

    [Header("On the path")]
    public float armDelay = 0.3f;   // seconds after landing before it can go off

    [Header("Effects (optional)")]
    public GameObject explosionPrefab;  // spawned where it goes off; give it Stop Action: Destroy so it cleans itself up
    public SfxEvent explodeSound;       //SFX

    // Every mine on the path or in the air, so towers can space theirs out
    // and an upgraded tower can take over the old tier's mines.
    // static = one list shared by every mine in the game, not a separate list per mine.
    // Other scripts read it through All, which is read-only, so only Mine itself adds and removes.
    static readonly List<Mine> all = new List<Mine>();
    public static IReadOnlyList<Mine> All => all;

    // "private set": any script can read these, but only Mine can change them.
    public Vector3 LandingPoint { get; private set; }   // where it sits, or will once it lands
    public Vector3 DroppedFrom { get; private set; }    // the tower's position; an upgrade puts the next tier right there

    // These come from the tower, so an upgraded tower's mines hit harder.
    float damage;
    float blastRadius;
    float triggerRadius;
    LayerMask enemyLayer;

    // Flight bookkeeping
    Vector3 launchPoint;            // where the throw started
    float flightTime;               // how many seconds the whole throw takes
    float timeFlown;                // how many seconds it's been in the air so far
    bool landed;                    // false while flying, true once it's sitting on the path
    float armedAt;                  // Time.time it can go off

    // Shared by every mine. That's safe because Unity runs each mine's Update one after another, never at the same time.
    static readonly Collider[] hits = new Collider[64];                          // reused buffer, no per-check allocation
    static readonly HashSet<IDamageable> damaged = new HashSet<IDamageable>();   // so an enemy with two colliders isn't hit twice

    void Awake()
    {
        // Until a tower launches it, it lands right where it is.
        // So a mine dragged straight into a scene (handy for testing) just sits there and works.
        launchPoint = transform.position;
        LandingPoint = transform.position;
    }

    // Join the list when the mine appears, leave it when it's destroyed (Destroy disables it first),
    // so All only ever holds mines that still exist.
    void OnEnable() => all.Add(this);
    void OnDisable() => all.Remove(this);

    // MineTower calls this right after it Instantiates the mine. Copies the tower's stats,
    // remembers which tower threw it, and works out how long the throw to `spot` takes.
    public void Launch(MineTower tower, Vector3 spot)
    {
        TakeStatsFrom(tower);
        DroppedFrom = tower.transform.position;
        launchPoint = transform.position;
        LandingPoint = spot;
        // distance / speed = seconds in the air. Never less than 0.05, so a spot right next to the
        // tower still gets a short throw and Fly() never divides by zero.
        flightTime = Mathf.Max(Vector3.Distance(launchPoint, spot) / speed, 0.05f);
    }

    // Copies the numbers that decide how hard this mine hits. Launch() calls it, and so does an
    // upgraded tower for mines the old tier left on the path (see MineTower.TakeOverMines).
    public void TakeStatsFrom(MineTower tower)
    {
        damage = tower.damage;
        blastRadius = tower.blastRadius;
        triggerRadius = tower.triggerRadius;
        enemyLayer = tower.enemyLayer;
    }

    // Every frame: fly until it lands, then wait out armDelay, then go off as soon as an enemy is close enough.
    void Update()
    {
        if (!landed)
        {
            Fly();
            return;
        }

        if (Time.time < armedAt) return;   // landed, but not armed yet

        IDamageable enemy = EnemyInReach();
        if (enemy != null) Explode(enemy);
    }

    // Moves the mine one frame further along its throw.
    void Fly()
    {
        timeFlown += Time.deltaTime;
        float t = flightTime > 0f ? Mathf.Clamp01(timeFlown / flightTime) : 1f;   // 0 at launch, 1 on landing

        // Straight line from the tower to the spot, plus a hump that's 0 at both ends and arcHeight in the middle.
        // t * (1 - t) is 0 at t = 0 and t = 1 and peaks at 0.25 halfway, so times 4 makes the peak exactly arcHeight.
        transform.position = Vector3.Lerp(launchPoint, LandingPoint, t) + Vector3.up * (4f * arcHeight * t * (1f - t));

        if (t >= 1f)
        {
            landed = true;
            armedAt = Time.time + armDelay;   // start the arming clock
        }
    }

    // The first live enemy close enough to set it off, or null.
    IDamageable EnemyInReach()
    {
        // Fills the reused hits array with every collider on the enemy layer inside the trigger zone
        // and returns how many it found (the rest of the array is old data, so only read up to count).
        int count = Physics.OverlapSphereNonAlloc(transform.position, triggerRadius, hits, enemyLayer);
        for (int i = 0; i < count; ++i)
        {
            // The collider is often on a child (the model); the health script is on the enemy itself, further up.
            IDamageable enemy = hits[i].GetComponentInParent<IDamageable>();
            if (enemy != null && !enemy.IsDead) return enemy;   // a dying enemy keeps its collider; don't waste the mine on it
        }
        return null;
    }

    // Damages every enemy within blastRadius, and always the one that set it off.
    void Explode(IDamageable trigger)
    {
        damaged.Clear();   // the set is shared between mines, so empty out the last explosion's enemies first
        int count = Physics.OverlapSphereNonAlloc(transform.position, blastRadius, hits, enemyLayer);
        for (int i = 0; i < count; ++i)
        {
            IDamageable enemy = hits[i].GetComponentInParent<IDamageable>();
            // damaged.Add returns false if this enemy is already in the set, so each enemy is hurt only once.
            if (enemy != null && !enemy.IsDead && damaged.Add(enemy))
                enemy.TakeDamage(damage);
        }

        // A blast smaller than the trigger zone could miss the enemy that stepped on it.
        if (!trigger.IsDead && damaged.Add(trigger))
            trigger.TakeDamage(damage);

        // Spawned on its own, not as a child, so it keeps playing after the mine is gone.
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        Sfx.Play(explodeSound, transform.position); //Play SFX
        Destroy(gameObject);   // OnDisable takes it off the All list; its tower sees it's gone and later throws a new one
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
