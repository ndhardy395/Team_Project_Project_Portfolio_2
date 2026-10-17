using UnityEngine;

namespace Warplane
{
    /// <summary>Spins the propeller and spinner with the plane's speed; it winds down once the plane is shot down.</summary>
    public class PropellerSpin : MonoBehaviour
    {
        public WarplaneSettings settings;
        [Tooltip("The real P-51's prop turns clockwise as seen from the cockpit.")]
        public bool clockwiseFromCockpit = true;
        public float spinUpRate = 1500f;

        float current;
        WarplaneFlight flight;

        void Awake()
        {
            flight = GetComponentInParent<WarplaneFlight>();
        }

        void Update()
        {
            if (settings == null) return;
            float target = 0f;
            if (flight != null && !flight.IsDoomed)
                target = Mathf.Lerp(settings.propIdleSpin, settings.propCruiseSpin, flight.Speed / Mathf.Max(settings.speed, 0.01f));
            current = Mathf.MoveTowards(current, target, spinUpRate * Time.deltaTime);
            float dir = clockwiseFromCockpit ? -1f : 1f;   // +Z rotation is counter-clockwise seen from behind
            transform.localRotation *= Quaternion.AngleAxis(dir * current * Time.deltaTime, Vector3.forward);
        }
    }
}
