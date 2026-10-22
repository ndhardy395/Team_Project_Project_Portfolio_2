using UnityEngine;

// Put this on the bullet prefab. Its blue (Z) arrow must point out the nose.
// No Collider or Rigidbody needed: it raycasts the distance it's about to move
// each frame, so a fast bullet can't skip through a thin capsule between frames.
public class Bullet : MonoBehaviour
{
    public float speed = 80f;       // units per second
    public float lifetime = 2f;     // seconds before a missed bullet cleans itself up
    public bool homing = true;      // turn off to see why straight shots miss moving targets

    float damage;
    LayerMask hitMask;
    Transform target;

    public void Init(float dmg, LayerMask mask, Transform aimTarget)
    {
        damage = dmg;
        hitMask = mask;
        target = aimTarget;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (homing && target != null)
            transform.forward = (target.position - transform.position).normalized;

        float step = speed * Time.deltaTime;

        // `out` is like passing a reference the function must fill in.
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, step, hitMask))
        {
            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null) damageable.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        transform.position += transform.forward * step;
    }
}
