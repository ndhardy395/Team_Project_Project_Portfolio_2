using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

// A tower that doesn't shoot. It throws mines onto the enemy path inside its range, and each mine
// sits there until an enemy walks up to it, then blows up (see Mine.cs).
// For the Tower Manu to select it, put towerClick and a trigger BoxCollider on the same object.
//
// Each frame (Update): put loaded mines back on the tower once they've reloaded, and if this tower
// has fewer than maxMines out, count down dropInterval and then throw one (TryDrop):
//   FindSpots()  - list every point on the enemy path inside range (only redone when range changes)
//   PickSpot()   - choose a random one of those that no other mine is too close to
//   Instantiate a Mine and Launch() it at that spot; from there the mine handles itself.
// ITower is what lets the Tower Manu, the range ring and upgrades treat this the same as a Gun.
public class MineTower : MonoBehaviour, ITower
{
    [Header("Stats (upgrades just change these; everything reads them live)")]
    public float range = 8f;            // mines only land on the path this close to the tower
    public float damage = 75f;          // each blast hurts every enemy within blastRadius
    public float blastRadius = 2.5f;    // AOE
    public float triggerRadius = 1f;    // an enemy this close to a mine sets it off
    public int maxMines = 3;            // most of this tower's mines on the path at once
    public float dropInterval = 3f;     // seconds between throws, counted only while there's room for another mine
    public float mineSpacing = 2f;      // a new mine won't land closer than this to any other mine//we take this out essentially

    [Header("Parts (drag in from the Hierarchy)")]
    public Mine minePrefab;             // the prefab with Mine.cs on it
    public Transform dropPoint;         // where a throw starts when no loaded mine is showing; the tower's base if empty

    [Header("Loaded mines (optional: ammo that sits on the tower)")]
    public GameObject[] loadedMines;    // one is hidden each throw, since the mine takes off from right where it sat
    public float reloadTime = 1.5f;     // seconds before a thrown one shows up on the tower again

    [Header("Targeting")]
    public LayerMask enemyLayer;        // set to your "Enemy" layer in the Inspector

    [Header("Upgrade (leave Upgrade Prefab empty on the last tier)")]
    public MineTower upgradePrefab;     // the next tier's tower prefab, e.g. MineTower1 -> MineTower2
    public int upgradeCost = 150;

    [Header("Range ring (shows in-game while this tower is clicked)")]
    public Color rangeColor = new Color(1f, 1f, 1f, 0.8f);
    public float rangeLineWidth = 0.15f;

    [Header("Permanent Shop Upgrades")]
    [SerializeField] private string shopTowerId;            // which shop upgrade boosts this tower 


    [Header("The Discription")]
    [SerializeField] private string discriptionTower;
    const float PathSampleStep = 0.5f;  // how finely the path is checked for spots in range

    readonly List<Vector3> spots = new List<Vector3>();     // points on the path within range
    float spotsRange = -1f;                                 // the range those spots were found for, so a range upgrade finds them again
    readonly List<Mine> mines = new List<Mine>();           // this tower's mines that haven't gone off yet
    float dropCooldown;                                     // starts at 0, so a new tower throws its first mine right away
    float[] reloadedAt;                                     // for each loaded mine: Time.time it shows up on the tower again

    LineRenderer rangeRing;

    // For the Tower Manu (see ITower).
    // Written as "ITower.Name" so they only show up when something treats this tower as an ITower;
    // they just hand over the upgrade fields above in the form the Tower Manu expects.
    GameObject ITower.UpgradePrefab => upgradePrefab != null ? upgradePrefab.gameObject : null;
    int ITower.UpgradeCost => upgradeCost;

    void Start()
    {
        damage *= 1f + SaveManager.GetTowerUpgrades(shopTowerId) * 0.2f;   // same permanent shop bonus as Gun: +20% per shop level
        reloadedAt = new float[loadedMines.Length];   // one reload timer per loaded mine on the tower
        TakeOverMines();
    }

    void Update()
    {
        ReloadMines();

        // Count this tower's mines still out there, and stop throwing while it's at maxMines.

        mines.RemoveAll(mine => mine == null);   // Unity counts a mine that blew up (Destroy'd) as null
        if (mines.Count >= maxMines)
        {
            // The clock only runs while there's room, so a mine that goes off is replaced a full interval later.
            dropCooldown = dropInterval;
            return;
        }

        dropCooldown -= Time.deltaTime;
        // Only reset the timer if a mine actually went out; if there was no free spot, try again next frame.
        if (dropCooldown <= 0f && TryDrop())
            dropCooldown = dropInterval;
    }

    // An upgrade puts the next tier exactly where this one stood, and its mines are still on the path.
    // Take them over, so they count toward maxMines and blow up with this tier's stats.
    void TakeOverMines()
    {
        for (int i = 0; i < Mine.All.Count; ++i)
        {
            Mine mine = Mine.All[i];
            // Thrown from (almost) exactly this spot = thrown by the tower this one replaced.
            // sqrMagnitude skips the square root; 0.01 squared distance = within 0.1 units.
            if ((mine.DroppedFrom - transform.position).sqrMagnitude < 0.01f)
            {
                mine.TakeStatsFrom(this);
                mines.Add(mine);
            }
        }
    }

    // Throws one mine at a free spot on the path. False if it couldn't (no prefab, or nowhere free to put it).
    bool TryDrop()
    {
        if (minePrefab == null) return false;
        if (range != spotsRange) FindSpots();   // first throw, or range changed since the spots were found
        if (!PickSpot(out Vector3 spot)) return false;   // no path in range, or every spot already has a mine near it

        // The throw starts from a loaded mine if one is showing, so it looks like that one took off.
        Transform from = TakeLoadedMine();
        if (from == null) from = dropPoint != null ? dropPoint : transform;

        Mine mine = Instantiate(minePrefab, from.position, from.rotation);
        mine.Launch(this, spot);   // the mine flies, waits and blows up on its own
        mines.Add(mine);
        return true;
    }

    // Every point along the enemy path(s) within range, PathSampleStep apart.
    void FindSpots()
    {
        spots.Clear();
        spotsRange = range;

        // The level's enemy path is a Unity Spline (a SplineContainer object in the scene). Check every one in the level.
        foreach (SplineContainer path in FindObjectsByType<SplineContainer>())
        {
            float length = path.CalculateLength();
            if (length <= 0f) continue;

            // Walk the whole path in PathSampleStep-sized steps and keep the points inside range.
            int steps = Mathf.CeilToInt(length / PathSampleStep);
            for (int i = 0; i <= steps; ++i)
            {
                // EvaluatePosition takes 0 (start of the path) to 1 (end) and returns that point in the world.
                Vector3 point = path.EvaluatePosition((float)i / steps);   // the same line EnemyMovement walks along
                if (FlatDistanceSqr(point, transform.position) <= range * range)
                    spots.Add(point);
            }
        }

        if (spots.Count == 0)
            Debug.LogWarning($"{name}: no enemy path within range ({range}), so it has nowhere to put mines.", this);
    }

    // A random spot on the path that's at least mineSpacing from every other mine, including ones still in the air.
    //! We have this turned off - which I prefer. It also would cause problems with limited spacing and mines.
    bool PickSpot(out Vector3 spot)
    {
        spot = default;

        // First count the free spots, then pick a random number in that count and walk to it.
        // That gives every free spot the same chance without building a second list.
        int free = 0;
        foreach (Vector3 s in spots)
            if (IsFree(s)) ++free;
        if (free == 0) return false;

        int pick = Random.Range(0, free);   // 0 to free - 1
        foreach (Vector3 s in spots)
        {
            if (!IsFree(s)) continue;
            if (pick-- == 0)   // counts down once per free spot; hits 0 on the chosen one
            {
                spot = s;
                return true;
            }
        }
        return false;
    }

    // Checks every tower's mines, so two mine towers next to each other don't stack mines on one spot.
    bool IsFree(Vector3 spot)
    {
        for (int i = 0; i < Mine.All.Count; ++i)
            if ((Mine.All[i].LandingPoint - spot).sqrMagnitude < mineSpacing * mineSpacing) return false;
        return true;
    }

    // Hides the first loaded mine still sitting on the tower and hands back where it was. Null if none are showing.
    Transform TakeLoadedMine()
    {
        for (int i = 0; i < loadedMines.Length; ++i)
        {
            GameObject loaded = loadedMines[i];
            if (loaded == null || !loaded.activeSelf) continue;

            loaded.SetActive(false);
            reloadedAt[i] = Time.time + reloadTime;
            return loaded.transform;
        }
        return null;
    }

    // Shows each hidden loaded mine again once its reload time has passed.
    void ReloadMines()
    {
        for (int i = 0; i < loadedMines.Length; ++i)
        {
            GameObject loaded = loadedMines[i];
            if (loaded != null && !loaded.activeSelf && Time.time >= reloadedAt[i])
                loaded.SetActive(true);
        }
    }

    // Range is measured flat along the ground, the same circle the range ring draws.
    static float FlatDistanceSqr(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return dx * dx + dz * dz;
    }

    // The Tower Manu calls this when the player clicks this tower (true) or closes the menu (false).
    public void ShowRange(bool show)
    {
        if (show && rangeRing == null) rangeRing = RangeRing.Build(transform, rangeColor, rangeLineWidth);
        if (rangeRing != null) rangeRing.enabled = show;
    }

    void LateUpdate()
    {
        if (rangeRing != null && rangeRing.enabled) RangeRing.Draw(rangeRing, transform.position, range);
    }

    // Scene view, with the tower selected: its range, and in Play mode the path spots mines can land on.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);

        Gizmos.color = Color.yellow;
        foreach (Vector3 s in spots) Gizmos.DrawSphere(s, 0.1f);
    }
}
