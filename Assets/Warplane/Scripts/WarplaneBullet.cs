using UnityEngine;

namespace Warplane
{
    /// <summary>
    /// A warplane bullet. It moves itself and raycasts along each frame's path, so it can't skip through
    /// thin targets. On a hit it calls TakeDamage(float) on what it hit (and that object's parents), then
    /// disappears. It never hits the plane that fired it, or the plane's own bombs.
    /// The tracer streak grows from nothing as the bullet leaves the gun, so it never reaches back through the wing.
    /// </summary>
    public class WarplaneBullet : MonoBehaviour
    {
        [Tooltip("The tracer streak: a child stretched along -Z behind the bullet.")]
        public Transform tracer;

        Vector3 velocity;
        Rigidbody owner;
        float damage, lifetime, age, tracerLength, tracerWidth;
        LayerMask hitMask = ~0;
        Renderer tracerRenderer;
        static readonly RaycastHit[] hits = new RaycastHit[16];

        void Awake()
        {
            if (tracer != null) tracerRenderer = tracer.GetComponent<Renderer>();
        }

        /// <summary>Called by WarplaneGuns right after Instantiate.</summary>
        public void Launch(Vector3 initialVelocity, Rigidbody shooter, WarplaneSettings s)
        {
            velocity = initialVelocity;
            owner = shooter;
            damage = s.bulletDamage;
            lifetime = s.bulletLifetime;
            hitMask = s.bulletHitMask;
            tracerLength = s.tracerLength;
            tracerWidth = s.tracerWidth;
            if (velocity.sqrMagnitude > 1e-4f) transform.rotation = Quaternion.LookRotation(velocity);
            UpdateTracer();
        }

        void Update()
        {
            float dt = Time.deltaTime;
            age += dt;
            if (age > lifetime)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 start = transform.position;
            Vector3 step = velocity * dt;
            float dist = step.magnitude;
            if (dist < 1e-6f) return;
            int n = Physics.RaycastNonAlloc(start, step / dist, hits, dist, hitMask, QueryTriggerInteraction.Ignore);
            int best = -1;
            float bestDist = float.MaxValue;
            for (int i = 0; i < n; ++i)
            {
                Rigidbody rb = hits[i].rigidbody;
                if (rb != null && (rb == owner || rb.GetComponent<WarplaneBomb>() != null)) continue;
                if (hits[i].distance < bestDist)
                {
                    bestDist = hits[i].distance;
                    best = i;
                }
            }
            if (best >= 0)
            {
                hits[best].collider.SendMessageUpwards("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                Destroy(gameObject);
                return;
            }
            transform.position = start + step;
            UpdateTracer();
        }

        /// <summary>Only as long as the bullet's lead on the gun, up to tracerLength; none on the frame it's fired.</summary>
        void UpdateTracer()
        {
            if (tracer == null) return;
            float length = Mathf.Min(tracerLength, velocity.magnitude * age);
            if (tracerRenderer != null) tracerRenderer.enabled = length > 0.01f;
            tracer.localScale = new Vector3(tracerWidth, tracerWidth, Mathf.Max(length, 0.001f));
            tracer.localPosition = new Vector3(0f, 0f, -0.5f * tracer.localScale.z);
        }
    }
}
