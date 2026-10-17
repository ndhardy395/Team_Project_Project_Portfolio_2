using UnityEngine;

namespace Warplane
{
    /// <summary>
    /// Flies the warplane along its WarplaneRoute, holding the height it was placed at: around and around a
    /// closed route, back and forth along an open one. It turns toward the next waypoint, banking into the
    /// turn, and in tight turns slows down and lowers its flaps. The steering values it uses (RollInput,
    /// PitchInput, YawInput, FlapsDown) also move the control surfaces. With no route it circles where it is.
    /// SetRoute switches it to another route (e.g. one the player drew); it joins at the nearest point.
    ///  - Missing wingtips make it lean toward the damaged side and wobble.
    ///  - Shot down (a wing or the tail gone, see WarplaneHealth): it spirals and falls until it hits the
    ///    ground (anything solid) and explodes, or explodes after doomedMaxTime.
    /// Your scripts can read Velocity (e.g. to lead targets) and set ForceFlaps.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class WarplaneFlight : MonoBehaviour
    {
        public WarplaneSettings settings;
        [Tooltip("The loop it flies. Empty = the nearest Warplane Route in the scene.")]
        public WarplaneRoute route;

        /// <summary>Current speed (units/s).</summary>
        public float Speed { get; private set; }
        /// <summary>Current velocity (units/s): level while flying, falling once shot down.</summary>
        public Vector3 Velocity { get; private set; }
        /// <summary>Steering, -1..1: roll (+ right), pitch (+ nose up), yaw (+ right). Drives the control surfaces.</summary>
        public float RollInput { get; private set; }
        public float PitchInput { get; private set; }
        public float YawInput { get; private set; }
        /// <summary>The flaps are down: in tight turns, or while ForceFlaps is on.</summary>
        public bool FlapsDown { get; private set; }
        /// <summary>Set true to hold the flaps down (e.g. on a bombing run).</summary>
        public bool ForceFlaps { get; set; }
        /// <summary>The waypoint it's heading for.</summary>
        public int TargetWaypoint { get; private set; }
        /// <summary>A wing or the tail is gone and it's going down.</summary>
        public bool IsDoomed => structure != null && structure.IsDoomed;
        /// <summary>The height it flies at (where it was placed).</summary>
        public float Altitude => altitude;
        public WarplaneRoute Route => route;

        Rigidbody rb;
        WarplaneHealth health;
        PlaneStructure structure;
        Vector3 startPosition;
        Quaternion startRotation;
        float altitude, heading, bank, doomTimer;
        int direction = 1;   // along an open route: +1 forward, -1 back
        float wobbleSeed;
        static readonly RaycastHit[] hits = new RaycastHit[16];

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;   // moved by this script; the colliders are there to be shot at
            rb.useGravity = false;
            health = GetComponent<WarplaneHealth>();
            structure = GetComponent<PlaneStructure>();
            wobbleSeed = Random.value * 100f;
        }

        void Start()
        {
            startPosition = transform.position;
            startRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            if (route == null) route = NearestRoute();
            ResetToStart();
            if (route != null) route.ShowLine(settings, altitude);
        }

        /// <summary>Fly this route from now on, joining it at the nearest point. The old route's line goes
        /// away if no other plane flies it, and a drawn one is deleted.</summary>
        public void SetRoute(WarplaneRoute newRoute)
        {
            WarplaneRoute old = route;
            route = newRoute;
            JoinRoute();
            if (route != null) route.ShowLine(settings, altitude);
            if (old == null || old == route) return;
            foreach (WarplaneFlight f in FindObjectsByType<WarplaneFlight>())
                if (f != this && f.route == old) return;   // someone else still flies it
            if (old.IsDrawn) Destroy(old.gameObject);
            else old.HideLine();
        }

        /// <summary>Heads for the nearest waypoint; along an open route, in the direction it's already flying.</summary>
        void JoinRoute()
        {
            direction = 1;
            if (route == null || route.Count == 0) return;
            int i = route.Nearest(transform.position);
            if (!route.closed && route.Count >= 2)
            {
                Vector3 fwd = Quaternion.Euler(0f, heading, 0f) * Vector3.forward;
                Vector3 along = route.Point(Mathf.Min(i + 1, route.Count - 1)) - route.Point(Mathf.Max(i - 1, 0));
                direction = Vector3.Dot(fwd, along) >= 0f ? 1 : -1;
            }
            TargetWaypoint = i;
        }

        WarplaneRoute NearestRoute()
        {
            WarplaneRoute best = null;
            float bestDist = float.MaxValue;
            foreach (WarplaneRoute r in FindObjectsByType<WarplaneRoute>())
            {
                if (r.Count == 0) continue;
                float d = (r.Point(r.Nearest(transform.position)) - transform.position).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = r;
                }
            }
            return best;
        }

        /// <summary>Back where it started, level and at cruise speed (used on respawn).</summary>
        public void ResetToStart()
        {
            transform.SetPositionAndRotation(startPosition, startRotation);
            altitude = startPosition.y;
            heading = startRotation.eulerAngles.y;
            bank = 0f;
            doomTimer = 0f;
            Speed = settings != null ? settings.speed : 0f;
            Velocity = transform.forward * Speed;
            RollInput = PitchInput = YawInput = 0f;
            FlapsDown = false;
            JoinRoute();
        }

        void Update()
        {
            if (settings == null || (health != null && health.IsDead)) return;   // exploded, waiting to respawn
            float dt = Time.deltaTime;
            if (IsDoomed) FlyDoomed(dt);
            else FlyRoute(dt);
        }

        void FlyRoute(float dt)
        {
            WarplaneSettings s = settings;
            Vector3 pos = transform.position;
            Vector3 fwd = Quaternion.Euler(0f, heading, 0f) * Vector3.forward;

            // ---- steer for the next waypoint (or circle with no route)
            float turn = s.idleCircle;
            if (route != null && route.Count >= 2)
            {
                // A plane can't hit a point inside its turning circle, so it doesn't try: close enough, or
                // close and already behind it, counts as reached. Drawn routes have points every unit or so,
                // so it skips ahead to the first one out of reach and chases that.
                float turnRadius = Speed / Mathf.Max(s.turnRate * Mathf.Deg2Rad, 0.01f);
                float reach = Mathf.Max(s.waypointRadius, turnRadius);
                Vector3 to = Vector3.zero;
                for (int guard = 0; guard <= route.Count; ++guard)
                {
                    to = route.Point(TargetWaypoint) - pos;
                    to.y = 0f;
                    bool reached = to.magnitude < reach || (to.magnitude < 2f * reach && Vector3.Dot(to, fwd) < 0f);
                    if (!reached) break;
                    TargetWaypoint = route.Advance(TargetWaypoint, ref direction);
                }
                float error = Vector3.SignedAngle(fwd, to, Vector3.up);   // + = the waypoint is to the right
                turn = Mathf.Clamp(error / Mathf.Max(s.turnSoftness, 1f), -1f, 1f);
            }

            // ---- speed and flaps: tight turns slow it down and lower the flaps
            FlapsDown = ForceFlaps || Mathf.Abs(turn) >= s.tightTurn;
            float targetSpeed = s.speed * (Mathf.Abs(turn) >= s.tightTurn ? s.turnSlowdown : 1f);
            Speed = Mathf.MoveTowards(Speed, targetSpeed, s.speed * dt);

            // ---- turn, and bank into it. Unity: rolling right is a negative turn around the nose (Z).
            heading += turn * s.turnRate * dt;
            int left = structure != null ? structure.TipsLost(-1) : 0;
            int right = structure != null ? structure.TipsLost(1) : 0;
            float lean = (right - left) * s.tipLean;
            float wobble = (left + right) * s.tipWobble * (Mathf.PerlinNoise(Time.time * 1.7f, wobbleSeed) * 2f - 1f);
            float targetBank = -turn * s.maxBank - lean + wobble;
            float rollStep = targetBank - bank;
            bank = Mathf.MoveTowards(bank, targetBank, s.rollRate * dt);

            // ---- what a pilot's hands would be doing: ailerons while the bank changes, a pull and a
            // little rudder through the turn
            RollInput = Mathf.Clamp(-rollStep / 15f, -1f, 1f);
            PitchInput = Mathf.Abs(turn) * 0.7f;
            YawInput = turn * 0.5f;

            fwd = Quaternion.Euler(0f, heading, 0f) * Vector3.forward;
            Velocity = fwd * Speed;
            Vector3 next = pos + Velocity * dt;
            next.y = altitude;
            transform.SetPositionAndRotation(next, Quaternion.Euler(0f, heading, 0f) * Quaternion.Euler(0f, 0f, bank));
        }

        /// <summary>A wing or the tail is gone: the controls are dead; it spins and falls until it hits something.</summary>
        void FlyDoomed(float dt)
        {
            WarplaneSettings s = settings;
            doomTimer += dt;
            RollInput = PitchInput = YawInput = 0f;
            FlapsDown = false;

            Vector3 pos = transform.position;
            Velocity += Physics.gravity * dt;
            Vector3 next = pos + Velocity * dt;
            if (HitsGround(pos, next, out Vector3 at))
            {
                transform.position = at;
                if (health != null) health.Explode(at);
                return;
            }
            if (doomTimer >= s.doomedMaxTime)
            {
                if (health != null) health.Explode(pos);
                return;
            }

            // Wing gone: roll toward the missing wing. Tail gone: the nose tucks under as it tumbles.
            float windUp = s.doomedSpinUpTime > 0f ? Mathf.Clamp01(doomTimer / s.doomedSpinUpTime) : 1f;
            int side = structure.DoomSide;
            Vector3 spin = structure.FatalPart != null && structure.FatalPart.kind == PartKind.Tail
                ? new Vector3(s.doomedTailPitchRate, 0f, -side * s.doomedTailRollRate)
                : new Vector3(0f, 0f, -side * s.doomedWingRollRate);
            Quaternion rot = transform.rotation * Quaternion.Euler(spin * (windUp * dt));
            // the nose swings toward the way it's falling
            Quaternion toFall = Quaternion.FromToRotation(rot * Vector3.forward, Velocity.normalized);
            rot = Quaternion.Slerp(Quaternion.identity, toFall, 1f - Mathf.Exp(-2f * dt)) * rot;
            transform.SetPositionAndRotation(next, rot);
        }

        /// <summary>Did the fall from a to b go into anything solid? Its own colliders and loose physics
        /// objects (debris, bombs) don't count.</summary>
        bool HitsGround(Vector3 a, Vector3 b, out Vector3 at)
        {
            at = b;
            Vector3 d = b - a;
            float dist = d.magnitude;
            if (dist < 1e-5f) return false;
            int n = Physics.RaycastNonAlloc(a, d / dist, hits, dist, ~0, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue;
            for (int i = 0; i < n; ++i)
            {
                Rigidbody body = hits[i].rigidbody;
                if (body == rb || (body != null && !body.isKinematic)) continue;
                if (hits[i].distance < best)
                {
                    best = hits[i].distance;
                    at = hits[i].point;
                }
            }
            return best < float.MaxValue;
        }
    }
}
