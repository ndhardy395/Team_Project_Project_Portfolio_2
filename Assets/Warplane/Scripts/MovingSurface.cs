using UnityEngine;

namespace Warplane
{
    /// <summary>
    /// An aileron, elevator, rudder or flap that moves with the plane's steering (WarplaneFlight), with
    /// small, quick deflections like a real plane's. Flaps lower in tight turns or while ForceFlaps is on.
    /// Purely visual. The object's pivot sits on the hinge line.
    /// </summary>
    public class MovingSurface : MonoBehaviour
    {
        public enum Kind { Aileron, Elevator, Rudder, Flap }

        public WarplaneSettings settings;
        public Kind kind = Kind.Aileron;
        [Tooltip("Hinge axis in the parent's space.")]
        public Vector3 hingeAxis = Vector3.right;
        [Tooltip("+1 or -1: flips the direction, e.g. the left aileron moves opposite the right one.")]
        public float sign = 1f;

        /// <summary>Current deflection in degrees, before sign.</summary>
        public float Angle { get; private set; }

        WarplaneFlight flight;
        Quaternion restRotation;

        void Awake()
        {
            restRotation = transform.localRotation;
            flight = GetComponentInParent<WarplaneFlight>();
        }

        void LateUpdate()
        {
            if (flight == null || settings == null) return;
            WarplaneSettings s = settings;
            float target, speed = s.surfaceSpeed;
            switch (kind)
            {
                case Kind.Aileron:
                    target = flight.RollInput * s.aileronAngle;
                    break;
                case Kind.Elevator:   // pulling raises the trailing edge; real elevators go further up than down
                    target = flight.PitchInput * (flight.PitchInput > 0f ? s.elevatorUpAngle : s.elevatorDownAngle);
                    break;
                case Kind.Rudder:
                    target = flight.YawInput * s.rudderAngle;
                    break;
                default:              // flaps
                    target = flight.FlapsDown ? s.flapAngle : 0f;
                    speed = s.flapSpeed;
                    break;
            }
            Angle = Mathf.MoveTowards(Angle, target, speed * Time.deltaTime);
            transform.localRotation = Quaternion.AngleAxis(Angle * sign, hingeAxis) * restRotation;
        }
    }
}
