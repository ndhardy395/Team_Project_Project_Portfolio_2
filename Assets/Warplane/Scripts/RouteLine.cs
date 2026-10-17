using System.Collections.Generic;
using UnityEngine;

namespace Warplane
{
    /// <summary>
    /// Draws a route as a thin line in the game, with bends tighter than the plane can turn in red (it
    /// cuts those corners). Used for the routes planes fly and for the line being drawn. Also holds the
    /// path helpers the drawer uses (smoothing, even spacing, finding the tight bends).
    /// </summary>
    public class RouteLine : MonoBehaviour
    {
        readonly List<LineRenderer> pool = new List<LineRenderer>();
        readonly List<Vector3> run = new List<Vector3>();

        /// <summary>Shows the path: closed = the last point joins back to the first.</summary>
        public void Show(IReadOnlyList<Vector3> points, bool closed, WarplaneSettings s)
        {
            int n = points.Count;
            int segments = n < 2 ? 0 : closed ? n : n - 1;
            bool[] tight = TightPoints(points, closed, s.TightestTurnRadius);
            int used = 0;
            // Split the line into runs of normal and too-tight segments, one LineRenderer per run.
            for (int j = 0; j < segments;)
            {
                bool red = SegmentTight(tight, j, n);
                run.Clear();
                run.Add(points[j]);
                while (j < segments && SegmentTight(tight, j, n) == red)
                {
                    run.Add(points[(j + 1) % n]);
                    ++j;
                }
                LineRenderer lr = Renderer(used++, s);
                lr.positionCount = run.Count;
                lr.SetPositions(run.ToArray());
                lr.startColor = lr.endColor = red ? s.tightColor : s.routeColor;
                lr.enabled = true;
            }
            for (int i = used; i < pool.Count; ++i) pool[i].enabled = false;
        }

        public void Hide()
        {
            foreach (LineRenderer lr in pool) lr.enabled = false;
        }

        static bool SegmentTight(bool[] tight, int j, int n) => tight[j] || tight[(j + 1) % n];

        LineRenderer Renderer(int i, WarplaneSettings s)
        {
            while (pool.Count <= i)
            {
                var go = new GameObject("Route Line");
                go.transform.SetParent(transform, false);
                var lr = go.AddComponent<LineRenderer>();
                lr.sharedMaterial = SoftFx.SpriteMaterial;
                lr.useWorldSpace = true;
                lr.alignment = LineAlignment.View;
                lr.numCapVertices = 2;
                lr.numCornerVertices = 2;
                lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                lr.receiveShadows = false;
                pool.Add(lr);
            }
            pool[i].widthMultiplier = s.routeLineWidth;
            return pool[i];
        }

        // ------------------------------------------------------------------ path helpers (flat: X and Z)
        /// <summary>Which points sit on a bend tighter than minRadius. Each bend is measured with the circle
        /// through the point and its neighbors about 0.8 units either side, so hand jitter doesn't count.</summary>
        public static bool[] TightPoints(IReadOnlyList<Vector3> p, bool closed, float minRadius)
        {
            int n = p.Count;
            var tight = new bool[n];
            if (n < 3) return tight;
            int k = Mathf.Max(1, Mathf.RoundToInt(0.8f / Mathf.Max(AverageSpacing(p), 0.01f)));
            for (int i = 0; i < n; ++i)
            {
                int a = i - k, b = i + k;
                if (closed)
                {
                    a = ((a % n) + n) % n;
                    b %= n;
                }
                else if (a < 0 || b >= n) continue;   // the ends of an open line: no bend to measure
                tight[i] = CircleRadius(p[a], p[i], p[b]) < minRadius;
            }
            return tight;
        }

        /// <summary>Radius of the circle through three points, seen from above (infinite if they're in a line).
        /// R = |a| |b| |c| / (2 |a × b|), with a, b, c the triangle's sides.</summary>
        static float CircleRadius(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            var a = new Vector2(p1.x - p0.x, p1.z - p0.z);
            var b = new Vector2(p2.x - p1.x, p2.z - p1.z);
            var c = new Vector2(p2.x - p0.x, p2.z - p0.z);
            float cross = Mathf.Abs(a.x * b.y - a.y * b.x);
            return cross < 1e-6f ? float.PositiveInfinity : a.magnitude * b.magnitude * c.magnitude / (2f * cross);
        }

        public static float Length(IReadOnlyList<Vector3> p)
        {
            float length = 0f;
            for (int i = 1; i < p.Count; ++i) length += Vector3.Distance(p[i - 1], p[i]);
            return length;
        }

        static float AverageSpacing(IReadOnlyList<Vector3> p) => p.Count > 1 ? Length(p) / (p.Count - 1) : 0f;

        /// <summary>Evens out hand jitter: each point moves to the average of itself and its neighbors
        /// (window points wide). The ends of an open line stay put.</summary>
        public static List<Vector3> Smooth(IReadOnlyList<Vector3> p, bool closed, int window)
        {
            int n = p.Count, half = window / 2;
            var result = new List<Vector3>(n);
            for (int i = 0; i < n; ++i)
            {
                if (!closed && (i < half || i >= n - half))
                {
                    result.Add(p[i]);
                    continue;
                }
                Vector3 sum = Vector3.zero;
                for (int j = -half; j <= half; ++j) sum += p[closed ? ((i + j) % n + n) % n : i + j];
                result.Add(sum / (2 * half + 1));
            }
            return result;
        }

        /// <summary>Points every spacing units along the line, keeping its last point.</summary>
        public static List<Vector3> Resample(IReadOnlyList<Vector3> p, float spacing)
        {
            var result = new List<Vector3>();
            if (p.Count == 0) return result;
            result.Add(p[0]);
            float carried = 0f;   // distance along the line since the last point placed
            for (int i = 1; i < p.Count; ++i)
            {
                Vector3 a = p[i - 1], b = p[i];
                float seg = Vector3.Distance(a, b);
                float t = spacing - carried;
                while (t <= seg)
                {
                    result.Add(Vector3.Lerp(a, b, t / seg));
                    t += spacing;
                }
                carried = seg - (t - spacing);
            }
            if ((result[result.Count - 1] - p[p.Count - 1]).sqrMagnitude > 1e-4f) result.Add(p[p.Count - 1]);
            return result;
        }
    }
}
