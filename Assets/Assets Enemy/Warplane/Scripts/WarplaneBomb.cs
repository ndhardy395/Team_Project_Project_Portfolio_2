using System.Collections.Generic;
using UnityEngine;

namespace Warplane
{
    /// <summary>
    /// A falling bomb. It keeps its nose along its fall and explodes on the first thing it touches (never
    /// the plane that dropped it). The blast calls TakeDamage(float) on everything within blastRadius, full
    /// damage at the center falling to nothing at the edge, and pushes loose Rigidbodies.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class WarplaneBomb : MonoBehaviour
    {
        public GameObject explosionPrefab;
        [Tooltip("Seconds before it blows up anyway, if it never hits anything.")]
        public float fuseTime = 10f;

        public GameObject Owner { get; private set; }

        Rigidbody rb;
        WarplaneSettings settings;
        float age;
        bool exploded;
        static readonly Collider[] overlap = new Collider[128];

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        /// <summary>Called by WarplaneBombs right after Instantiate.</summary>
        public void Launch(Vector3 velocity, GameObject owner, Collider[] ownerColliders, WarplaneSettings s)
        {
            Owner = owner;
            settings = s;
            rb.linearVelocity = velocity;
            Collider[] mine = GetComponentsInChildren<Collider>();
            foreach (Collider o in ownerColliders)
                foreach (Collider c in mine)
                    if (o != null && c != null) Physics.IgnoreCollision(c, o, true);
        }

        void FixedUpdate()
        {
            age += Time.fixedDeltaTime;
            Vector3 v = rb.linearVelocity;
            if (v.sqrMagnitude > 0.01f)
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(v), 1f - Mathf.Exp(-4f * Time.fixedDeltaTime)));
            if (age > fuseTime) Explode(transform.position);
        }

        void OnCollisionEnter(Collision c)
        {
            if (exploded || (Owner != null && c.transform.IsChildOf(Owner.transform))) return;
            Explode(c.contactCount > 0 ? c.GetContact(0).point : transform.position);
        }

        public void Explode(Vector3 point)
        {
            if (exploded) return;
            exploded = true;
            float radius = settings != null ? settings.blastRadius : 3f;
            float damage = settings != null ? settings.bombDamage : 100f;
            LayerMask mask = settings != null ? settings.bombHitMask : (LayerMask)~0;

            int n = Physics.OverlapSphereNonAlloc(point, radius, overlap, mask, QueryTriggerInteraction.Collide);
            var damaged = new HashSet<GameObject>();
            var pushed = new HashSet<Rigidbody>();
            for (int i = 0; i < n; ++i)
            {
                Collider col = overlap[i];
                if (col == null || col.attachedRigidbody == rb) continue;
                if (Owner != null && col.transform.IsChildOf(Owner.transform)) continue;   // not the plane that dropped it

                // one hit per target, even if it has several colliders
                GameObject target = col.attachedRigidbody != null ? col.attachedRigidbody.gameObject : col.gameObject;
                if (damaged.Add(target))
                {
                    float dist = Vector3.Distance(point, col.bounds.ClosestPoint(point));   // works for every collider type
                    float amount = damage * Mathf.Clamp01(1f - dist / radius);
                    if (amount > 0f) col.SendMessageUpwards("TakeDamage", amount, SendMessageOptions.DontRequireReceiver);
                }
                Rigidbody body = col.attachedRigidbody;
                if (body != null && !body.isKinematic && pushed.Add(body) && settings != null)
                    body.AddExplosionForce(settings.blastPush, point, radius, 0.5f, ForceMode.Impulse);
            }

            if (explosionPrefab != null)
            {
                GameObject fx = Instantiate(explosionPrefab, point, Quaternion.identity);
                SimpleExplosion se = fx.GetComponent<SimpleExplosion>();
                if (se != null) se.size = settings != null ? settings.bombExplosionSize : 0.5f;
            }
            Destroy(gameObject);
        }
    }
}
