using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Warplane
{
    /// <summary>
    /// Lets the player draw a plane's route: press on a plane (or close to it) with the left mouse button,
    /// drag a line over the map, let go. One of these in the scene handles every plane
    /// (Tools ▸ Warplane ▸ Add Route Drawer To Scene).
    ///  - The line is read at the plane's flying height, so the plane flies exactly over what you drew on
    ///    screen (the camera is perspective, and the plane flies above the ground).
    ///  - Bends tighter than the plane can turn show red; it flies them by cutting the corner.
    ///  - End the line close to where it started and it becomes a loop; otherwise the plane flies it back
    ///    and forth. Lines shorter than minRouteLength are ignored.
    /// Other click handlers (like the fireball spawner) can skip clicks meant for a plane by checking
    /// RouteDrawer.IsOverPlane(mouse position) first.
    /// </summary>
    public class RouteDrawer : MonoBehaviour
    {
        /// <summary>The player is drawing a route right now.</summary>
        public static bool IsDrawing { get; private set; }

        WarplaneFlight plane;   // the one whose route is being drawn
        readonly List<Vector3> drawn = new List<Vector3>();
        RouteLine preview;

        // Fast Enter Play Mode keeps statics between Play sessions.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => IsDrawing = false;

        /// <summary>The plane within its pickRadius of this screen position (the closest), or null.</summary>
        public static WarplaneFlight PlaneAt(Vector2 screenPosition)
        {
            Camera view = Camera.main;
            if (view == null) return null;
            float pixels = Screen.height / 1080f;
            WarplaneFlight best = null;
            float bestDist = float.MaxValue;
            foreach (WarplaneFlight f in FindObjectsByType<WarplaneFlight>())
            {
                if (f.settings == null || f.IsDoomed) continue;
                WarplaneHealth h = f.GetComponent<WarplaneHealth>();
                if (h != null && h.IsDown) continue;
                Vector3 sp = view.WorldToScreenPoint(f.transform.position);
                if (sp.z <= 0f) continue;
                float d = Vector2.Distance(screenPosition, sp);
                if (d <= f.settings.pickRadius * pixels && d < bestDist)
                {
                    bestDist = d;
                    best = f;
                }
            }
            return best;
        }

        /// <summary>Would a press here start drawing a route? For other click handlers to check first.</summary>
        public static bool IsOverPlane(Vector2 screenPosition) => PlaneAt(screenPosition) != null;

        void Update()
        {
            Mouse mouse = Mouse.current;
            Camera view = Camera.main;
            if (mouse == null || view == null) return;
            Vector2 at = mouse.position.ReadValue();

            if (plane == null)
            {
                if (mouse.leftButton.wasPressedThisFrame && !OverUI())
                {
                    WarplaneFlight picked = PlaneAt(at);
                    if (picked != null) Begin(picked, view, at);
                }
                return;
            }
            if (mouse.leftButton.isPressed) Extend(view, at);
            else Finish();
        }

        static bool OverUI() => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        void Begin(WarplaneFlight picked, Camera view, Vector2 at)
        {
            plane = picked;
            IsDrawing = true;
            drawn.Clear();
            if (OnFlyingHeight(view, at, out Vector3 p)) drawn.Add(p);
        }

        void Extend(Camera view, Vector2 at)
        {
            if (plane == null || !OnFlyingHeight(view, at, out Vector3 p)) return;
            float spacing = Mathf.Max(plane.settings.drawSpacing, 0.05f);
            if (drawn.Count == 0)
            {
                drawn.Add(p);
                return;
            }
            // Fill in points along a fast mouse movement, so the spacing stays even.
            Vector3 last = drawn[drawn.Count - 1];
            float dist = Vector3.Distance(last, p);
            for (float t = spacing; t <= dist; t += spacing) drawn.Add(Vector3.Lerp(last, p, t / dist));
            ShowPreview();
        }

        void ShowPreview()
        {
            if (preview == null) preview = new GameObject("Route Being Drawn").AddComponent<RouteLine>();
            List<Vector3> smooth = RouteLine.Smooth(drawn, false, 5);
            preview.Show(smooth, WouldClose(smooth, plane.settings), plane.settings);
        }

        static bool WouldClose(List<Vector3> p, WarplaneSettings s)
        {
            return p.Count > 2 && Vector3.Distance(p[0], p[p.Count - 1]) <= s.closeDistance && RouteLine.Length(p) > 2f * s.closeDistance;
        }

        void Finish()
        {
            WarplaneFlight target = plane;
            plane = null;
            IsDrawing = false;
            if (preview != null) preview.Hide();
            if (target == null || target.settings == null) return;
            WarplaneSettings s = target.settings;
            if (RouteLine.Length(drawn) < s.minRouteLength) return;   // a click on the plane, not a route

            List<Vector3> smooth = RouteLine.Smooth(drawn, false, 5);
            bool closed = WouldClose(smooth, s);
            // Waypoints about a unit apart: plenty for the plane to follow, few enough to stay cheap.
            List<Vector3> points = RouteLine.Resample(smooth, Mathf.Max(1f, s.drawSpacing));
            if (closed && points.Count > 2 && Vector3.Distance(points[0], points[points.Count - 1]) < 0.5f)
                points.RemoveAt(points.Count - 1);   // the loop closes itself; no doubled point at the seam

            var go = new GameObject("Drawn Route (" + target.name + ")");
            WarplaneRoute route = go.AddComponent<WarplaneRoute>();
            route.SetPoints(points, closed);
            target.SetRoute(route);
        }

        /// <summary>Where the mouse ray meets the plane's flying height.</summary>
        bool OnFlyingHeight(Camera view, Vector2 screenPosition, out Vector3 point)
        {
            point = Vector3.zero;
            var height = new Plane(Vector3.up, new Vector3(0f, plane.Altitude, 0f));
            Ray ray = view.ScreenPointToRay(screenPosition);
            if (!height.Raycast(ray, out float t)) return false;
            point = ray.GetPoint(t);
            return true;
        }

        void OnDisable()
        {
            plane = null;
            IsDrawing = false;
            if (preview != null) preview.Hide();
        }
    }
}
