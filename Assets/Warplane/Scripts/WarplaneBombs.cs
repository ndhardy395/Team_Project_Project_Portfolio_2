using System;
using UnityEngine;
using UnityEngine.Events;

namespace Warplane
{
    /// <summary>
    /// Drops bombs from the wing hardpoints, alternating left and right, when your targeting script calls
    /// Drop(). Each hardpoint shows its bomb until it's dropped and gets a new one bombReloadTime later.
    ///  - PredictLanding(out point) says where a bomb dropped right now would land, so your script can drop
    ///    when that point is on (or just ahead of) an enemy.
    ///  - Bombs damage everything in blastRadius by calling its TakeDamage(float), less toward the edge.
    /// </summary>
    public class WarplaneBombs : MonoBehaviour
    {
        public WarplaneSettings settings;
        public WarplaneBomb bombPrefab;
        public Transform[] hardpoints = new Transform[0];
        [Tooltip("The bomb shown on each hardpoint (same order as hardpoints).")]
        public GameObject[] mountedVisuals = new GameObject[0];

        public UnityEvent onBombDropped = new UnityEvent();
        public event Action<WarplaneBomb> BombDropped;

        public int Capacity => hardpoints.Length;
        public int BombsReady
        {
            get
            {
                int n = 0;
                foreach (bool b in loaded) if (b) ++n;
                return n;
            }
        }

        WarplaneFlight flight;
        WarplaneHealth health;
        Collider[] planeColliders;
        bool[] loaded = new bool[0];
        float[] reloadAt = new float[0];
        int next;
        static readonly RaycastHit[] hits = new RaycastHit[16];

        void Awake()
        {
            flight = GetComponent<WarplaneFlight>();
            health = GetComponent<WarplaneHealth>();
            planeColliders = GetComponentsInChildren<Collider>(true);
            loaded = new bool[Capacity];
            reloadAt = new float[Capacity];
            RearmAll();
        }

        void Update()
        {
            for (int i = 0; i < Capacity; ++i)
                if (!loaded[i] && Time.time >= reloadAt[i]) SetLoaded(i, true);
        }

        /// <summary>Drops the next ready bomb. Returns it, or null if none is ready (or the plane is down).</summary>
        public WarplaneBomb Drop()
        {
            if (bombPrefab == null || settings == null || (health != null && health.IsDown)) return null;
            int i = NextReady();
            if (i < 0) return null;

            Transform hp = hardpoints[i];
            WarplaneBomb bomb = Instantiate(bombPrefab, hp.position, hp.rotation);
            bomb.Launch(ReleaseVelocity(), gameObject, planeColliders, settings);
            SetLoaded(i, false);
            reloadAt[i] = Time.time + settings.bombReloadTime;
            next = (i + 1) % Capacity;
            onBombDropped.Invoke();
            if (BombDropped != null) BombDropped(bomb);
            return bomb;
        }

        /// <summary>Where a bomb dropped now would land (first solid thing it would hit). False if none is
        /// ready or it wouldn't hit anything within maxSeconds of falling.</summary>
        public bool PredictLanding(out Vector3 point, float maxSeconds = 5f)
        {
            point = transform.position;
            int i = NextReady();
            if (i < 0 || settings == null) return false;
            Vector3 p = hardpoints[i].position;
            Vector3 v = ReleaseVelocity();
            const float Step = 0.02f;
            for (float t = 0f; t < maxSeconds; t += Step)
            {
                Vector3 nextV = v + Physics.gravity * Step;
                Vector3 nextP = p + (v + nextV) * (0.5f * Step);
                if (SolidHit(p, nextP, out point)) return true;
                p = nextP;
                v = nextV;
            }
            return false;
        }

        public void RearmAll()
        {
            for (int i = 0; i < Capacity; ++i) SetLoaded(i, true);
        }

        Vector3 ReleaseVelocity()
        {
            Vector3 v = flight != null ? flight.Velocity : Vector3.zero;
            return v - transform.up * (settings != null ? settings.bombEjectSpeed : 0f);
        }

        int NextReady()
        {
            for (int k = 0; k < Capacity; ++k)
            {
                int i = (next + k) % Capacity;
                if (loaded[i] && hardpoints[i] != null && hardpoints[i].gameObject.activeInHierarchy) return i;
            }
            return -1;
        }

        bool SolidHit(Vector3 a, Vector3 b, out Vector3 at)
        {
            at = b;
            Vector3 d = b - a;
            float dist = d.magnitude;
            if (dist < 1e-6f) return false;
            int n = Physics.RaycastNonAlloc(a, d / dist, hits, dist, ~0, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue;
            for (int i = 0; i < n; ++i)
            {
                if (hits[i].collider.transform.IsChildOf(transform)) continue;   // the plane itself
                if (hits[i].distance < best)
                {
                    best = hits[i].distance;
                    at = hits[i].point;
                }
            }
            return best < float.MaxValue;
        }

        void SetLoaded(int i, bool value)
        {
            loaded[i] = value;
            if (i < mountedVisuals.Length && mountedVisuals[i] != null) mountedVisuals[i].SetActive(value);
        }

        void OnDrawGizmosSelected()
        {
            if (Application.isPlaying && PredictLanding(out Vector3 p))
            {
                Gizmos.color = new Color(1f, 0.4f, 0f);
                Gizmos.DrawWireSphere(p, settings != null ? settings.blastRadius : 1f);
            }
        }
    }
}
