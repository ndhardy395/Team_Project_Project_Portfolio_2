using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Stats (upgrades just change these; everything reads them live)")]
    public float range = 50f;
    public float damage = 50f;
    public float fireRate = 20f;          // shots per second
    public float splashRadius = 0f;       // 0 = hits one enemy; above 0 = hurts every enemy this close to where the shot lands

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

    [Header("Upgrade (leave Upgrade Prefab empty on the last tier)")]
    public Gun upgradePrefab;             // the next tier's tower prefab, e.g. GunTower 1 -> GunTower 2
    public int upgradeCost = 150;

    [Header("Range ring (shows in-game while this tower is clicked)")]
    public Color rangeColor = new Color(1f, 1f, 1f, 0.8f);
    public float rangeLineWidth = 0.15f;

    Transform target;
    float fireCooldown;
    int nextBarrel;
    readonly Collider[] hits = new Collider[32];   // reused buffer, no per-scan allocation

    IDamageable targetHealth;   // the target's health script, so we can tell when it starts dying

    LineRenderer rangeRing;
    static Material rangeRingMaterial;   // one material shared by every tower's ring

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

        // Drop an enemy that's playing its death animation.
        if (targetHealth != null && targetHealth.IsDead)
        {
            target = null;
            return;
        }

        // Target may have walked out of range between scans.
        if ((target.position - transform.position).sqrMagnitude > range * range)
        {
            target = null;
            return;
        }

        // Towers with no turning parts (like the mortar) skip aiming; the homing bullet does the work.
        bool onTarget = standPivot == null || Aim();

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

        // Rule: shoot whoever is furthest along the path (least distance left to the end).
        Transform best = null;
        IDamageable bestHealth = null;
        float bestRemaining = float.PositiveInfinity;

        for (int i = 0; i < count; ++i)
        {
            IDamageable health = hits[i].GetComponentInParent<IDamageable>();
            if (health == null || health.IsDead) continue;   // not an enemy, or already dying

            IPathProgress path = hits[i].GetComponentInParent<IPathProgress>();
            float remaining = path != null ? path.DistanceRemaining : float.PositiveInfinity;

            if (best == null || remaining < bestRemaining)
            {
                best = hits[i].transform;
                bestHealth = health;
                bestRemaining = remaining;
            }
        }
        target = best;
        targetHealth = bestHealth;
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
        bullet.Init(damage, splashRadius, enemyLayer, target);   // bullet handles its own hit + cleanup
    }

    // The Tower Manu calls this when the player clicks this tower (true) or closes the menu (false).
    public void ShowRange(bool show)
    {
        if (show && rangeRing == null) rangeRing = BuildRangeRing();
        if (rangeRing != null) rangeRing.enabled = show;
    }

    // Gizmos only draw in the Scene view, so the in-game ring is a looped LineRenderer.
    LineRenderer BuildRangeRing()
    {
        if (rangeRingMaterial == null)
            rangeRingMaterial = new Material(Shader.Find("Sprites/Default"));   // unlit, tinted by the line color

        GameObject ring = new GameObject("RangeRing");
        ring.transform.SetParent(transform, false);   // destroyed along with the tower, e.g. on upgrade

        LineRenderer line = ring.AddComponent<LineRenderer>();
        line.useWorldSpace = true;   // range is in world units, so draw the ring in world units too
        line.loop = true;
        line.positionCount = 64;
        line.widthMultiplier = rangeLineWidth;
        line.sharedMaterial = rangeRingMaterial;
        line.startColor = rangeColor;
        line.endColor = rangeColor;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        return line;
    }

    void LateUpdate()
    {
        if (rangeRing == null || !rangeRing.enabled) return;

        // Redrawn every frame it's showing, so a range upgrade resizes the ring right away.
        float step = 2f * Mathf.PI / rangeRing.positionCount;
        for (int i = 0; i < rangeRing.positionCount; ++i)
        {
            Vector3 edge = new Vector3(Mathf.Cos(i * step), 0f, Mathf.Sin(i * step)) * range;
            rangeRing.SetPosition(i, transform.position + edge + Vector3.up * 0.1f);   // lift it off the ground so it doesn't flicker
        }
    }

    // Draws the range sphere in the Scene view when the tower is selected.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);

        if (target != null) 
        { 
            Gizmos.color = Color.yellow; 
            Gizmos.DrawLine(transform.position, target.position); 
        }
    }
}
