using UnityEngine;

namespace Warplane
{
    /// <summary>
    /// The gun heat warning, drawn on screen next to the plane (it's a top-down game, so it follows the
    /// plane, not a crosshair): a thin arc beside the plane that fills with heat, white to orange to red,
    /// and a flashing "OVERHEAT" above the plane while the guns are locked. Hidden while the guns are cold.
    /// Look and placement live in WarplaneSettings (Overheat warning).
    /// </summary>
    [RequireComponent(typeof(WarplaneGuns))]
    public class OverheatIndicator : MonoBehaviour
    {
        public WarplaneSettings settings;

        WarplaneGuns guns;
        WarplaneHealth health;
        GUIStyle label;

        void Awake()
        {
            guns = GetComponent<WarplaneGuns>();
            health = GetComponent<WarplaneHealth>();
        }

        void OnGUI()
        {
            if (Event.current.type != EventType.Repaint || settings == null) return;
            if ((guns.Heat <= 0.005f && !guns.Overheated) || (health != null && health.IsDown)) return;
            Camera view = Camera.main;
            if (view == null) return;
            Vector3 sp = view.WorldToScreenPoint(transform.position);
            if (sp.z <= 0f) return;

            // Laid out on a 1080p-tall canvas, scaled to the real screen.
            float scale = Screen.height / 1080f;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float x = sp.x / scale, y = (Screen.height - sp.y) / scale;
            bool flashOn = guns.Overheated && Mathf.Repeat(Time.unscaledTime * 4f, 1f) < 0.5f;

            if (settings.showHeatArc) DrawArc(x, y, settings.heatArcRadius, flashOn ? Color.white : HeatColor(guns.Heat));
            if (guns.Overheated)
            {
                if (label == null)
                {
                    label = new GUIStyle(GUI.skin.label) { fontSize = 18, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                    label.normal.textColor = Color.white;   // GUI.color tints it
                }
                Rect r = new Rect(x - 100f, y - settings.overheatTextHeight - 12f, 200f, 24f);
                GUI.color = new Color(0f, 0f, 0f, 0.6f);   // a dark copy behind it so it reads on any ground
                GUI.Label(new Rect(r.x + 1.5f, r.y + 1.5f, r.width, r.height), settings.overheatText, label);
                GUI.color = flashOn ? Color.white : new Color(1f, 0.25f, 0.2f);
                GUI.Label(r, settings.overheatText, label);
            }
            GUI.color = Color.white;
            GUI.matrix = Matrix4x4.identity;
        }

        /// <summary>A thin arc on the plane's right, filling upward with heat. θ (theta) runs from the bottom
        /// of the arc (-Span/2) to the top (+Span/2), 0 = straight right.</summary>
        void DrawArc(float x, float y, float r, Color fill)
        {
            const int Segments = 16;
            const float Span = 110f;
            for (int pass = 0; pass < 2; ++pass)   // dark edges first, then the track and fill on top
            {
                for (int i = 0; i < Segments; ++i)
                {
                    float t0 = (-Span / 2f + Span * i / Segments) * Mathf.Deg2Rad;
                    float t1 = (-Span / 2f + Span * (i + 1) / Segments) * Mathf.Deg2Rad;
                    var a = new Vector2(x + r * Mathf.Cos(t0), y - r * Mathf.Sin(t0));
                    var b = new Vector2(x + r * Mathf.Cos(t1), y - r * Mathf.Sin(t1));
                    if (pass == 0)
                    {
                        GUI.color = new Color(0f, 0f, 0f, 0.45f);
                        Line(a, b, 5f);
                    }
                    else
                    {
                        GUI.color = (i + 0.5f) / Segments <= guns.Heat ? fill : new Color(1f, 1f, 1f, 0.25f);
                        Line(a, b, 3f);
                    }
                }
            }
        }

        static Color HeatColor(float h)
        {
            var orange = new Color(1f, 0.6f, 0.15f);
            var red = new Color(1f, 0.18f, 0.12f);
            return h < 0.5f ? Color.Lerp(Color.white, orange, h / 0.5f) : Color.Lerp(orange, red, (h - 0.5f) / 0.5f);
        }

        /// <summary>A straight line from a to b in the current GUI.matrix's units.</summary>
        static void Line(Vector2 a, Vector2 b, float thickness)
        {
            Vector2 d = b - a;
            float length = d.magnitude;
            if (length < 0.5f) return;
            Matrix4x4 saved = GUI.matrix;
            GUI.matrix = saved * Matrix4x4.TRS(a, Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg), Vector3.one);
            GUI.DrawTexture(new Rect(0f, -thickness * 0.5f, length, thickness), Texture2D.whiteTexture);
            GUI.matrix = saved;
        }
    }
}
