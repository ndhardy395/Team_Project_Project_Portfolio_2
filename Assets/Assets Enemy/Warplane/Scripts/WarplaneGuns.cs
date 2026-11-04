using UnityEngine;

namespace Warplane
{
    /// <summary>
    /// The six wing guns, fired by your targeting script: call FireAt(target) every frame you want them
    /// shooting, like holding a trigger; stop calling it and they stop. The bullets fly from the muzzles
    /// straight at the point (no lead, so lead moving targets yourself, e.g. with their velocity).
    ///  - Heat: overheatSeconds of continuous fire overheats them. Then they lock until they've cooled to
    ///    unlockHeat; FireAt does nothing meanwhile. Check CanFire, Heat and Overheated.
    ///  - Bullets damage whatever they hit by calling its TakeDamage(float), the same way your enemies
    ///    already take damage (any script with a TakeDamage(float) method works).
    /// </summary>
    public class WarplaneGuns : MonoBehaviour
    {
        public WarplaneSettings settings;
        public Transform[] muzzles = new Transform[0];
        public WarplaneBullet bulletPrefab;

        /// <summary>0 = cold, 1 = overheated.</summary>
        public float Heat { get; private set; }
        /// <summary>Hit 100% heat; locked until cooled to settings.unlockHeat.</summary>
        public bool Overheated { get; private set; }
        /// <summary>Fired at least one round this frame.</summary>
        public bool IsFiring { get; private set; }
        /// <summary>FireAt would shoot now: not overheated and not shot down.</summary>
        public bool CanFire => !Overheated && (health == null || !health.IsDown);

        Rigidbody body;
        WarplaneHealth health;
        Vector3 aimPoint;
        int requestedFrame = -10;
        float cooldown, lastShot = float.NegativeInfinity;
        int nextMuzzle;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            health = GetComponent<WarplaneHealth>();
        }

        /// <summary>Shoot at this point this frame. Call it every frame you want to keep firing.</summary>
        public void FireAt(Vector3 point)
        {
            aimPoint = point;
            requestedFrame = Time.frameCount;
        }

        public void FireAt(Transform target)
        {
            if (target != null) FireAt(target.position);
        }

        public void ResetHeat()
        {
            Heat = 0f;
            Overheated = false;
        }

        void Update()
        {
            if (settings == null) return;
            WarplaneSettings s = settings;
            float dt = Time.deltaTime;

            // cooling, and unlocking once cool enough
            if (Time.time - lastShot >= s.coolDelay)
                Heat = Mathf.Max(0f, Heat - dt / Mathf.Max(s.coolSeconds, 0.01f));
            if (Overheated && Heat <= s.unlockHeat) Overheated = false;

            // "Still holding the trigger" = FireAt was called this frame or last (your script may run after this one).
            IsFiring = false;
            cooldown -= dt;
            bool wants = requestedFrame >= Time.frameCount - 1;
            if (!wants || !CanFire || bulletPrefab == null || muzzles.Length == 0)
            {
                if (cooldown < 0f) cooldown = 0f;
                return;
            }
            int safety = 0;
            while (cooldown <= 0f && !Overheated && safety++ < 20)   // at low frame rates several rounds may be due
            {
                Shoot(s);
                cooldown += 1f / Mathf.Max(s.fireRate, 0.1f);
            }
        }

        void Shoot(WarplaneSettings s)
        {
            Transform m = muzzles[nextMuzzle % muzzles.Length];
            nextMuzzle = (nextMuzzle + 1) % muzzles.Length;
            if (m == null || !m.gameObject.activeInHierarchy) return;   // its wing is gone

            Vector3 dir = aimPoint - m.position;
            if (dir.sqrMagnitude < 1e-6f) return;
            Quaternion aim = Quaternion.LookRotation(dir) *
                             Quaternion.Euler(Random.Range(-s.spreadDegrees, s.spreadDegrees), Random.Range(-s.spreadDegrees, s.spreadDegrees), 0f);
            WarplaneBullet bullet = Instantiate(bulletPrefab, m.position, aim);
            bullet.Launch(aim * Vector3.forward * s.bulletSpeed, body, s);
            IsFiring = true;
            lastShot = Time.time;

            // each round adds its share, so fireRate × overheatSeconds rounds fill the heat
            Heat += 1f / Mathf.Max(s.fireRate * s.overheatSeconds, 1f);
            if (Heat >= 0.9999f)
            {
                Heat = 1f;
                Overheated = true;
            }
        }
    }
}
