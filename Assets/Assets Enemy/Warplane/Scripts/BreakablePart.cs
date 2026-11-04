using UnityEngine;

namespace Warplane
{
    /// <summary>
    /// A piece of the warplane that can break off. It sits on the piece's model object, and pieces nested
    /// under it (a wing's tip, flap, ailerons and guns) go with it. Its colliders live under the plane's
    /// Colliders object and are listed here. Set up by Tools ▸ Warplane ▸ Build Plane.
    /// </summary>
    public class BreakablePart : MonoBehaviour
    {
        public PartKind kind = PartKind.WingTip;
        [Tooltip("+1 = right side, -1 = left, 0 = center (the tail).")]
        public int side;
        [Tooltip("Switched off while the part is gone.")]
        public Collider[] colliders = new Collider[0];

        public bool IsBroken { get; private set; }
        /// <summary>Losing it brings the plane down (a whole wing or the tail).</summary>
        public bool IsFatal => kind != PartKind.WingTip;

        public void SetBroken(bool broken)
        {
            IsBroken = broken;
            foreach (Collider c in colliders)
                if (c != null) c.enabled = !broken;
        }
    }
}
