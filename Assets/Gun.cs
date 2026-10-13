using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Stats (upgrades just change these; everything reads them live)")]
    public float range = 50f;
    public float damage = 50f;
    public float fireRate = 20f;          // shots per second
    public bool splash = false;

    [Header("Parts (drag in from the Hierarchy)")]
    public Transform standPivot;          // turns left/right only: yaw, around world Y
    public Transform barrelPivot;         // child of standPivot; tilts up/down only: pitch, around local X
    public Transform[] firePoints;        // empties at each barrel's muzzle tip, blue (Z) arrow pointing out
    public Bullet bulletPrefab;            // the prefab with Bullet.cs on it

    [Header("Aiming")]
    public float turnSpeed = 180f;        // degrees per second
    public float minElevation = -10f;     // how far the barrels can tilt down
    public float maxElevation = 60f;      // how far the barrels can tilt up
    public float aimTolerance = 5f;       // degrees off-target we still allow firing

    [Header("Targeting")]
    public LayerMask enemyLayer;          // set to your "Enemy" layer in the Inspector
    public float scanInterval = 0.25f;    // seconds between range checks

    Transform target;
    float fireCooldown;
    int nextBarrel;
    readonly Collider[] hits = new Collider[32];   // reused buffer, no per-scan allocation

    void Start()
    {
        // Scan a few times per second instead of every frame.
        InvokeRepeating(nameof(FindTarget), 0f, scanInterval);
    }

    void Update()
    {
        // Unity overloads == so a Destroy()'d enemy compares equal to null,
        // even though the C# reference still exists.
        if (target == null) return;

        // Target may have walked out of range between scans.
        if ((target.position - transform.position).sqrMagnitude > range * range)
        {
            target = null;
            return;
        }

        bool onTarget = Aim();

        fireCooldown -= Time.deltaTime;
        if (onTarget && fireCooldown <= 0f)
        {
            Shoot();
            fireCooldown = 1f / fireRate;
        }
    }

    void FindTarget()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, range, hits, enemyLayer);

        // Placeholder rule: closest enemy to the tower.
        Transform best = null;
        float bestSqr = float.MaxValue;
        for (int i = 0; i < count; ++i)
        {
            float sqr = (hits[i].transform.position - transform.position).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = hits[i].transform;
            }
        }
        target = best;
    }

    // Rotates stand (yaw) and barrels (pitch) toward the target.
    // Returns true when the barrels are pointed close enough to fire.
    bool Aim()
    {
        // --- Stand: yaw only ---
        Vector3 flat = target.position - standPivot.position;
        flat.y = 0f;                                   // ignore height, so the stand never tips
        if (flat.sqrMagnitude < 0.0001f) return false; // target directly above or below

        Quaternion yaw = Quaternion.LookRotation(flat); // rotation whose +Z faces flat
        standPivot.rotation = Quaternion.RotateTowards(
            standPivot.rotation, yaw, turnSpeed * Time.deltaTime);

        // --- Barrels: pitch only, relative to the stand ---
        Vector3 toTarget = target.position - barrelPivot.position;
        float horizontal = new Vector2(toTarget.x, toTarget.z).magnitude;

        float elevation = Mathf.Atan2(toTarget.y, horizontal) * Mathf.Rad2Deg; // + = up
        elevation = Mathf.Clamp(elevation, minElevation, maxElevation);

        // Unity is left-handed: a positive X rotation tips +Z DOWN, so negate to aim up.
        Quaternion tilt = Quaternion.Euler(-elevation, 0f, 0f);
        barrelPivot.localRotation = Quaternion.RotateTowards(
            barrelPivot.localRotation, tilt, turnSpeed * Time.deltaTime);

        return Vector3.Angle(barrelPivot.forward, toTarget) < aimTolerance;
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoints.Length == 0) return;

        Transform muzzle = firePoints[nextBarrel];
        nextBarrel = (nextBarrel + 1) % firePoints.Length;   // alternate barrels

        // Because bulletPrefab is typed as Bullet, Instantiate hands back the Bullet component directly.
        Bullet bullet = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);
        bullet.Init(damage, enemyLayer, target);   // bullet handles its own hit + cleanup
    }

    // Draws the range sphere in the Scene view when the tower is selected.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
