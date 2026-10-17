using System.Collections.Generic;
using UnityEngine;

namespace Warplane
{
    /// <summary>What a breakable piece is.</summary>
    public enum PartKind
    {
        WingTip,   // the outer 20% of a wing: breaks off and the plane flies on
        Wing,      // the rest of a wing: tears off and the plane goes down
        Tail,      // stabilizer + fin: tears off and the plane goes down
    }

    /// <summary>
    /// The warplane's breakable pieces (wingtips, wings, tail). WarplaneHealth decides what breaks;
    /// Break() hides the piece with everything nested on it, switches off its colliders and throws a copy
    /// of it as debris. RestoreAll() puts everything back (respawn).
    /// </summary>
    public class PlaneStructure : MonoBehaviour
    {
        public WarplaneSettings settings;

        public bool IsDoomed { get; private set; }
        /// <summary>The part whose loss brought it down (null while it's flying).</summary>
        public BreakablePart FatalPart { get; private set; }
        /// <summary>Which way the death spiral rolls: +1 right, -1 left.</summary>
        public int DoomSide { get; private set; }
        public IReadOnlyList<BreakablePart> Parts => parts;

        BreakablePart[] parts = new BreakablePart[0];
        Collider[] planeColliders;
        WarplaneFlight flight;

        void Awake()
        {
            parts = GetComponentsInChildren<BreakablePart>(true);
            planeColliders = GetComponentsInChildren<Collider>(true);
            flight = GetComponent<WarplaneFlight>();
        }

        /// <summary>Wingtips missing on one side: +1 right, -1 left.</summary>
        public int TipsLost(int side)
        {
            int n = 0;
            foreach (BreakablePart p in parts)
                if (p.kind == PartKind.WingTip && p.side == side && p.IsBroken) ++n;
            return n;
        }

        /// <summary>Breaks a part off, with everything nested on it. away = which way the debris flies.</summary>
        public void Break(BreakablePart part, Vector3 hitPoint, Vector3 away)
        {
            if (part == null || part.IsBroken) return;
            foreach (BreakablePart p in part.GetComponentsInChildren<BreakablePart>(true)) p.SetBroken(true);
            ThrowDebris(part, away);
            part.gameObject.SetActive(false);

            if (part.IsFatal && !IsDoomed)
            {
                IsDoomed = true;
                FatalPart = part;
                DoomSide = part.side != 0 ? part.side : transform.InverseTransformPoint(hitPoint).x >= 0f ? 1 : -1;
                if (DoomSide == 0) DoomSide = Random.value < 0.5f ? -1 : 1;
            }
        }

        /// <summary>Every part back on, colliders included.</summary>
        public void RestoreAll()
        {
            foreach (BreakablePart p in parts)
            {
                p.SetBroken(false);
                p.gameObject.SetActive(true);
            }
            IsDoomed = false;
            FatalPart = null;
            DoomSide = 0;
        }

        void ThrowDebris(BreakablePart part, Vector3 away)
        {
            if (settings == null || settings.debrisLifetime <= 0f) return;
            Transform src = part.transform;
            GameObject copy = Instantiate(part.gameObject, src.position, src.rotation);
            copy.name = part.name + " (debris)";
            copy.transform.localScale = src.lossyScale;
            // A lifeless copy: no moving parts or logic, no effects (muzzle glows, smoke, wisps).
            foreach (MonoBehaviour b in copy.GetComponentsInChildren<MonoBehaviour>(true)) Destroy(b);
            foreach (TrailRenderer t in copy.GetComponentsInChildren<TrailRenderer>(true)) Destroy(t);
            foreach (SpriteRenderer s in copy.GetComponentsInChildren<SpriteRenderer>(true)) Destroy(s);

            var body = copy.AddComponent<Rigidbody>();
            body.mass = 1f;
            body.linearDamping = settings.debrisDrag;
            body.angularDamping = 0.3f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.linearVelocity = (flight != null ? flight.Velocity : Vector3.zero) + away * settings.debrisPush;
            body.angularVelocity = Random.onUnitSphere * settings.debrisSpin;

            MeshFilter mf = copy.GetComponent<MeshFilter>();
            var box = copy.AddComponent<BoxCollider>();
            if (mf != null && mf.sharedMesh != null)
            {
                box.center = mf.sharedMesh.bounds.center;
                box.size = mf.sharedMesh.bounds.size;
            }
            copy.AddComponent<Debris>().Init(box, planeColliders, 0.15f, settings.debrisLifetime);
        }
    }
}
