using UnityEngine;

namespace Warplane
{
    /// <summary>
    /// Hot gun barrels. As WarplaneGuns heats up, the barrels go from dark metal to dull red, cherry and
    /// orange; a soft glow grows at each muzzle (the barrels themselves are tiny on screen); and a thin
    /// smoke trail streams back while the guns are overheated. Colors and sizes live in WarplaneSettings.
    /// The glows and smoke ride on the barrels, so a lost wing takes them along.
    /// </summary>
    [RequireComponent(typeof(WarplaneGuns))]
    public class BarrelGlow : MonoBehaviour
    {
        public WarplaneSettings settings;
        [Tooltip("The gun barrels' renderers, one per wing (unlit material, so their color shows as is).")]
        public Renderer[] barrels = new Renderer[0];

        WarplaneGuns guns;
        MaterialPropertyBlock block;
        Color[] cold;
        SpriteRenderer[] glows;
        Vector3[] glowAt;            // each muzzle in its barrel set's space
        TrailRenderer[] smoke;
        bool tinted;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        void Awake()
        {
            guns = GetComponent<WarplaneGuns>();
            block = new MaterialPropertyBlock();
            cold = new Color[barrels.Length];
            for (int i = 0; i < barrels.Length; ++i)
            {
                Material m = barrels[i] != null ? barrels[i].sharedMaterial : null;
                cold[i] = m == null ? Color.gray : m.HasProperty(BaseColor) ? m.GetColor(BaseColor) : m.color;
            }
            BuildGlows();
            BuildSmoke();
        }

        void BuildGlows()
        {
            Material material = SoftFx.SpriteMaterial;
            int n = guns.muzzles != null && material != null ? guns.muzzles.Length : 0;
            glows = new SpriteRenderer[n];
            glowAt = new Vector3[n];
            for (int k = 0; k < n; ++k)
            {
                Transform muzzle = guns.muzzles[k];
                Renderer nearest = muzzle != null ? Nearest(muzzle.position) : null;
                if (nearest == null) continue;
                var go = new GameObject("Muzzle Glow");
                go.transform.SetParent(nearest.transform, false);
                glowAt[k] = nearest.transform.InverseTransformPoint(muzzle.position);
                go.transform.localPosition = glowAt[k];
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = SoftFx.GlowSprite;
                sr.sharedMaterial = material;
                sr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                sr.receiveShadows = false;
                sr.enabled = false;
                glows[k] = sr;
            }
        }

        Renderer Nearest(Vector3 point)
        {
            Renderer best = null;
            float bestDist = float.MaxValue;
            foreach (Renderer r in barrels)
            {
                if (r == null) continue;
                float d = (r.bounds.center - point).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = r;
                }
            }
            return best;
        }

        void BuildSmoke()
        {
            smoke = new TrailRenderer[barrels.Length];
            Material material = SoftFx.TrailMaterial;
            if (material == null || settings == null) return;
            for (int i = 0; i < barrels.Length; ++i)
            {
                MeshFilter mf = barrels[i] != null ? barrels[i].GetComponent<MeshFilter>() : null;
                if (mf == null || mf.sharedMesh == null) continue;
                Bounds b = mf.sharedMesh.bounds;
                var go = new GameObject("Gun Smoke");
                go.transform.SetParent(barrels[i].transform, false);
                go.transform.localPosition = new Vector3(b.center.x, b.center.y, b.max.z);
                var trail = go.AddComponent<TrailRenderer>();
                trail.sharedMaterial = material;
                trail.time = settings.smokeSeconds;
                trail.minVertexDistance = 0.05f;
                trail.widthCurve = AnimationCurve.Linear(0f, settings.smokeWidthStart, 1f, settings.smokeWidthEnd);
                var fade = new Gradient();
                Color gray = new Color(0.32f, 0.31f, 0.3f);
                fade.SetKeys(new[] { new GradientColorKey(gray, 0f), new GradientColorKey(gray, 1f) },
                             new[] { new GradientAlphaKey(settings.smokeAlpha, 0f), new GradientAlphaKey(0f, 1f) });
                trail.colorGradient = fade;
                trail.textureMode = LineTextureMode.Stretch;
                trail.alignment = LineAlignment.View;
                trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                trail.receiveShadows = false;
                trail.emitting = false;
                smoke[i] = trail;
            }
        }

        // After the plane and the camera have moved, so the glows face the camera this frame.
        void LateUpdate()
        {
            if (settings == null) return;
            WarplaneSettings s = settings;
            Color hot = s.barrelHeat.Evaluate(guns.Heat);   // alpha = how much heat shows
            TintBarrels(hot);

            Camera view = Camera.main;
            bool show = hot.a > 0.01f && view != null;
            for (int k = 0; k < glows.Length; ++k)
            {
                SpriteRenderer sr = glows[k];
                if (sr == null) continue;
                sr.enabled = show;
                if (!show) continue;
                Transform g = sr.transform;
                Vector3 p = g.parent.TransformPoint(glowAt[k]);
                Vector3 eye = view.transform.position;
                g.position = p + (eye - p).normalized * s.glowTowardCamera;
                g.rotation = view.transform.rotation;   // face the camera
                g.localScale = Vector3.one * (s.glowSize * Mathf.Lerp(0.4f, 1f, hot.a));
                sr.color = new Color(hot.r, hot.g, hot.b, hot.a * s.glowAlpha);
            }

            foreach (TrailRenderer t in smoke)
            {
                if (t == null) continue;
                t.emitting = guns.Overheated;
                t.time = s.smokeSeconds;
            }
        }

        void TintBarrels(Color hot)
        {
            if (hot.a <= 0f)
            {
                if (!tinted) return;
                foreach (Renderer r in barrels)
                    if (r != null) r.SetPropertyBlock(null);   // back to the plain material
                tinted = false;
                return;
            }
            for (int i = 0; i < barrels.Length; ++i)
            {
                if (barrels[i] == null) continue;
                Color c = Color.Lerp(cold[i], hot, hot.a);
                c.a = 1f;
                barrels[i].GetPropertyBlock(block);
                block.SetColor(BaseColor, c);   // URP
                block.SetColor(ColorId, c);     // built-in pipeline
                barrels[i].SetPropertyBlock(block);
            }
            tinted = true;
        }
    }
}
