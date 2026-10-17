using UnityEngine;

namespace Warplane
{
    /// <summary>
    /// Every tunable number for the warplane, in one asset (Warplane/Data/WarplaneSettings.asset, made by
    /// Tools ▸ Warplane ▸ Build Plane). Sizes and speeds are in scene units. Changes work live in Play mode,
    /// except the wingspan: after changing it, run Tools ▸ Warplane ▸ Build Plane again.
    /// The defaults suit a plane about 1.5 units across over a map about 60 × 40 units.
    /// </summary>
    [CreateAssetMenu(menuName = "Warplane/Settings", fileName = "WarplaneSettings")]
    public class WarplaneSettings : ScriptableObject
    {
        [Header("Size (rebuild after changing: Tools ▸ Warplane ▸ Build Plane)")]
        [Tooltip("Wingtip to wingtip, in scene units.")]
        public float wingspan = 1.5f;
        [Tooltip("Tools ▸ Warplane ▸ Add Plane And Route places the plane this high above the ground. " +
                 "In Play it holds whatever height it was placed at.")]
        public float placeHeight = 6f;

        [Header("Flying the route")]
        [Tooltip("Cruise speed (units/s).")]
        public float speed = 6f;
        [Tooltip("Fastest turn (degrees/s). Turn radius = speed / turn rate, about 3.8 units at the defaults.")]
        public float turnRate = 90f;
        [Tooltip("Heading error (degrees) that gets the fastest turn. Smaller = snappier.")]
        public float turnSoftness = 30f;
        [Tooltip("How far it banks in its tightest turn (degrees).")]
        public float maxBank = 50f;
        [Tooltip("How fast it rolls into and out of a bank (degrees/s).")]
        public float rollRate = 120f;
        [Tooltip("How close (units) it gets to a waypoint before heading for the next. It never needs to get closer than its turn radius.")]
        public float waypointRadius = 1.5f;
        [Tooltip("Turns at least this hard (share of the fastest turn) count as tight: it slows down and lowers its flaps.")]
        [Range(0f, 1f)] public float tightTurn = 0.6f;
        [Tooltip("Speed in tight turns, as a share of cruise.")]
        [Range(0.3f, 1f)] public float turnSlowdown = 0.75f;
        [Tooltip("With no route it circles at this share of its fastest turn.")]
        [Range(0f, 1f)] public float idleCircle = 0.4f;

        [Header("Routes in the game")]
        [Tooltip("Show the route each plane flies as a line in the game, at the plane's height.")]
        public bool showRoute = true;
        [Tooltip("Route line width (units).")]
        public float routeLineWidth = 0.06f;
        public Color routeColor = new Color(1f, 1f, 1f, 0.55f);
        [Tooltip("Bends tighter than the plane can turn show in this color (it cuts those corners).")]
        public Color tightColor = new Color(1f, 0.2f, 0.15f, 0.9f);

        [Header("Drawing routes (Warplane Route Drawer)")]
        [Tooltip("Press within this many pixels (on a 1080p screen) of a plane, then drag, to draw its route.")]
        public float pickRadius = 60f;
        [Tooltip("Spacing (units) of the points recorded while drawing.")]
        public float drawSpacing = 0.4f;
        [Tooltip("A line shorter than this (units) is ignored: it was a click, not a route.")]
        public float minRouteLength = 4f;
        [Tooltip("A line ending this close (units) to where it started closes into a loop; otherwise the plane flies it back and forth.")]
        public float closeDistance = 3f;

        [Header("Moving parts")]
        [Tooltip("Deflection (degrees) at full input.")]
        public float aileronAngle = 15f;
        public float elevatorUpAngle = 20f;
        public float elevatorDownAngle = 15f;
        public float rudderAngle = 20f;
        [Tooltip("How fast (degrees/s) the ailerons, elevator and rudder move.")]
        public float surfaceSpeed = 120f;
        [Tooltip("Flap angle (degrees) when lowered.")]
        public float flapAngle = 40f;
        [Tooltip("How fast (degrees/s) the flaps move: slower than the other surfaces, like real flaps.")]
        public float flapSpeed = 30f;
        [Tooltip("Propeller spin (degrees/s) when stopped and at cruise speed.")]
        public float propIdleSpin = 500f;
        public float propCruiseSpin = 2100f;

        [Header("Health and breaking apart")]
        public float maxHealth = 100f;
        [Tooltip("A wingtip breaks off when health falls to this share of max, and the other one at the second.")]
        [Range(0f, 1f)] public float firstTipBreaksAt = 0.667f;
        [Range(0f, 1f)] public float secondTipBreaksAt = 0.333f;
        [Tooltip("Degrees it leans toward a side that has lost its wingtip.")]
        public float tipLean = 10f;
        [Tooltip("Degrees of wobble per missing wingtip.")]
        public float tipWobble = 6f;
        [Tooltip("After it explodes, come back at the start of the route?")]
        public bool respawn = true;
        public float respawnDelay = 4f;

        [Header("Going down (health at 0: a wing or the tail tears off)")]
        [Tooltip("Seconds it can spiral before it explodes anyway, if it hasn't hit anything.")]
        public float doomedMaxTime = 4f;
        [Tooltip("Seconds for the spiral to wind up to full rate.")]
        public float doomedSpinUpTime = 0.4f;
        [Tooltip("Wing gone: roll rate (degrees/s) toward the missing wing.")]
        public float doomedWingRollRate = 300f;
        [Tooltip("Tail gone: the nose tucks under at this rate (degrees/s) while it tumbles.")]
        public float doomedTailPitchRate = 150f;
        public float doomedTailRollRate = 100f;
        [Tooltip("Explosion size when it hits the ground.")]
        public float crashExplosionSize = 0.6f;
        [Tooltip("Seconds before a broken-off piece disappears. 0 = no debris.")]
        public float debrisLifetime = 4f;
        [Tooltip("Speed (units/s) a broken-off piece is thrown off.")]
        public float debrisPush = 1.5f;
        [Tooltip("Tumble (rad/s) of a broken-off piece.")]
        public float debrisSpin = 6f;
        public float debrisDrag = 0.5f;

        [Header("Guns (fire with WarplaneGuns.FireAt)")]
        [Tooltip("Rounds per second across all six guns.")]
        public float fireRate = 20f;
        public float bulletSpeed = 60f;
        public float bulletDamage = 5f;
        [Tooltip("Seconds before a bullet that hit nothing disappears (range = speed × this).")]
        public float bulletLifetime = 1.5f;
        [Tooltip("Random spread (degrees) around the aim.")]
        public float spreadDegrees = 0.6f;
        [Tooltip("What bullets can hit. Leave on Everything, or pick your enemy layer.")]
        public LayerMask bulletHitMask = ~0;
        [Tooltip("Tracer streak length and thickness (units).")]
        public float tracerLength = 0.6f;
        public float tracerWidth = 0.03f;

        [Header("Gun heat")]
        [Tooltip("Seconds of continuous fire from cold guns to an overheat.")]
        public float overheatSeconds = 5f;
        [Tooltip("Seconds for fully hot guns to cool all the way down once they stop firing.")]
        public float coolSeconds = 3f;
        [Tooltip("Pause (s) after the last shot before the guns start cooling.")]
        public float coolDelay = 0.2f;
        [Tooltip("Overheated guns stay locked until they've cooled to this (0.5 = half: about 1.5 s).")]
        [Range(0f, 1f)] public float unlockHeat = 0.5f;

        [Header("Hot barrels")]
        [Tooltip("Barrel color by heat, cold on the left to overheated on the right. Color = how the hot metal looks; " +
                 "alpha = how much of it shows (0 = plain metal) and how strong the glow at each muzzle is.")]
        public Gradient barrelHeat = DefaultBarrelHeat();
        [Tooltip("Size (units) of the soft glow at each muzzle at full heat. The barrels are tiny; this is what you see.")]
        public float glowSize = 0.3f;
        [Range(0f, 1f)] public float glowAlpha = 0.85f;
        [Tooltip("The glow sits this far (units) toward the camera so the wing doesn't cut it off.")]
        public float glowTowardCamera = 0.05f;
        [Tooltip("Smoke from overheated barrels: how long it lasts (s), its width (units) at the barrels and where it fades.")]
        public float smokeSeconds = 0.5f;
        public float smokeWidthStart = 0.02f;
        public float smokeWidthEnd = 0.15f;
        [Range(0f, 1f)] public float smokeAlpha = 0.35f;

        [Header("Overheat warning (on screen, next to the plane)")]
        [Tooltip("A thin arc beside the plane that fills with heat, white → orange → red.")]
        public bool showHeatArc = true;
        [Tooltip("Arc distance from the plane's center (pixels on a 1080p screen).")]
        public float heatArcRadius = 34f;
        public string overheatText = "OVERHEAT";
        [Tooltip("Height (pixels on a 1080p screen) of the OVERHEAT text above the plane's center.")]
        public float overheatTextHeight = 44f;

        [Header("Bombs (drop with WarplaneBombs.Drop)")]
        [Tooltip("Seconds until an empty hardpoint has a new bomb.")]
        public float bombReloadTime = 3f;
        public float blastRadius = 3f;
        [Tooltip("Damage at the center of the blast, falling off to 0 at the edge.")]
        public float bombDamage = 100f;
        [Tooltip("What the blast can damage. Leave on Everything, or pick your enemy layer.")]
        public LayerMask bombHitMask = ~0;
        [Tooltip("Push given to loose Rigidbodies in the blast.")]
        public float blastPush = 20f;
        [Tooltip("Downward push (units/s) when a bomb is released.")]
        public float bombEjectSpeed = 0.3f;
        public float bombExplosionSize = 0.5f;

        [Header("Wake wisps")]
        [Tooltip("One knob for the wisps' size: scales both widths. 1 = default.")]
        public float wispSize = 1f;
        [Tooltip("How long (s) the wake lasts. Its length is speed × this.")]
        public float wispLength = 0.8f;
        [Range(0f, 1f)] public float wispOpacity = 0.3f;
        [Tooltip("Width (units) where a wisp leaves the plane and where it fades out, before wispSize.")]
        public float wispWidthStart = 0.05f;
        public float wispWidthEnd = 0.35f;
        [Tooltip("Share of the wake behind the plane before the wisps show.")]
        [Range(0f, 0.6f)] public float wispFadeIn = 0.15f;

        /// <summary>The tightest circle (radius, units) the plane can fly: in a tight turn it slows to
        /// turnSlowdown × speed and turns at turnRate. Route bends tighter than this get cut.</summary>
        public float TightestTurnRadius => speed * turnSlowdown / Mathf.Max(turnRate * Mathf.Deg2Rad, 0.01f);

        /// <summary>Dark metal until about a quarter heat, then dull red, cherry red, and orange at overheat.</summary>
        static Gradient DefaultBarrelHeat()
        {
            var g = new Gradient();
            g.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.45f, 0.03f, 0f), 0f),
                    new GradientColorKey(new Color(0.75f, 0.06f, 0.02f), 0.55f),
                    new GradientColorKey(new Color(1f, 0.2f, 0.04f), 0.8f),
                    new GradientColorKey(new Color(1f, 0.5f, 0.12f), 1f),
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0f, 0.25f),
                    new GradientAlphaKey(0.7f, 0.6f),
                    new GradientAlphaKey(1f, 1f),
                });
            return g;
        }
    }
}
