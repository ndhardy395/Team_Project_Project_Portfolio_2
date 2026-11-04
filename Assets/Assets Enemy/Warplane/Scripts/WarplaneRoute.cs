using System.Collections.Generic;
using UnityEngine;

namespace Warplane
{
    /// <summary>
    /// The route a warplane flies. Two kinds:
    ///  - Placed in the editor: this object's children, in order. Drag them around in the Scene view; add
    ///    or remove children to change the shape. Only their X and Z matter: the plane holds the height it
    ///    was placed at.
    ///  - Drawn by the player (Warplane Route Drawer): a list of points, set with SetPoints.
    /// Closed = it loops back to the first waypoint after the last; open = the plane turns around at each
    /// end and flies it back.
    /// </summary>
    public class WarplaneRoute : MonoBehaviour
    {
        [Tooltip("On: loop back to the first waypoint after the last. Off: turn around at each end and fly it back.")]
        public bool closed = true;

        readonly List<Vector3> points = new List<Vector3>();
        RouteLine line;

        /// <summary>Drawn by the player (a list of points) rather than placed in the editor (children).</summary>
        public bool IsDrawn => points.Count > 0;
        public int Count => IsDrawn ? points.Count : transform.childCount;

        /// <summary>Waypoint i: wrapping around a closed route, clamped to the ends of an open one.</summary>
        public Vector3 Point(int i)
        {
            int n = Count;
            i = closed ? ((i % n) + n) % n : Mathf.Clamp(i, 0, n - 1);
            return IsDrawn ? points[i] : transform.GetChild(i).position;
        }

        /// <summary>The waypoint after i, heading dir (+1 forward, -1 back). On an open route dir flips at the ends.</summary>
        public int Advance(int i, ref int dir)
        {
            int n = Count;
            if (closed) return ((i + 1) % n + n) % n;
            if (i + dir < 0 || i + dir >= n) dir = -dir;
            return Mathf.Clamp(i + dir, 0, n - 1);
        }

        /// <summary>The waypoint closest to a position (flat distance).</summary>
        public int Nearest(Vector3 position)
        {
            int best = 0;
            float bestDist = float.MaxValue;
            for (int i = 0; i < Count; ++i)
            {
                Vector3 d = Point(i) - position;
                d.y = 0f;
                if (d.sqrMagnitude < bestDist)
                {
                    bestDist = d.sqrMagnitude;
                    best = i;
                }
            }
            return best;
        }

        /// <summary>Makes this a drawn route through these points (world positions).</summary>
        public void SetPoints(IReadOnlyList<Vector3> newPoints, bool isClosed)
        {
            points.Clear();
            points.AddRange(newPoints);
            closed = isClosed;
        }

        /// <summary>Shows the route as a line in the game (if settings.showRoute is on).</summary>
        public void ShowLine(WarplaneSettings s, float height)
        {
            if (s == null || !s.showRoute || Count < 2)
            {
                HideLine();
                return;
            }
            if (line == null) line = new GameObject("Route Line").AddComponent<RouteLine>();
            line.transform.SetParent(transform, false);
            var shown = new List<Vector3>(Count);
            for (int i = 0; i < Count; ++i)
            {
                Vector3 p = Point(i);
                shown.Add(new Vector3(p.x, height, p.z));   // at the plane's height, where it actually flies
            }
            line.Show(shown, closed, s);
        }

        public void HideLine()
        {
            if (line != null) line.Hide();
        }

        void OnDrawGizmos()
        {
            int n = Count;
            if (n < 2) return;
            Gizmos.color = new Color(0.3f, 0.9f, 1f);
            int segments = closed ? n : n - 1;
            for (int i = 0; i < segments; ++i)
            {
                Vector3 a = Point(i), b = Point(i + 1);
                Gizmos.DrawLine(a, b);
                Vector3 dir = (b - a).normalized;
                if (!IsDrawn && dir.sqrMagnitude > 0f)   // arrowheads: which way it flies
                {
                    Gizmos.DrawLine(b, b + Quaternion.AngleAxis(150f, Vector3.up) * dir * 0.8f);
                    Gizmos.DrawLine(b, b + Quaternion.AngleAxis(-150f, Vector3.up) * dir * 0.8f);
                }
                if (!IsDrawn) Gizmos.DrawWireSphere(a, 0.3f);
            }
        }
    }
}
