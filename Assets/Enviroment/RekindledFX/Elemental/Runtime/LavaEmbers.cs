using UnityEngine;

namespace RekindledFX.Elemental
{
    /// <summary>
    /// Hot embers / sparks rising from a lava surface. Sets up the ParticleSystem entirely from code;
    /// place this object above the lava surface (its local XZ plane sits on the surface) and embers drift upward.
    ///
    /// The mote sprite is procedural (soft-dot), drawn with an additive shader -> no external texture needed.
    /// Also previews in edit mode.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(ParticleSystem))]
    [AddComponentMenu("RekindledFX/Elemental/Lava Embers")]
    public class LavaEmbers : MonoBehaviour
    {
        [Header("Area (local XZ - place this object on the lava surface)")]
        [Tooltip("Rectangular area the embers spawn from (X, Z meters).")]
        public Vector2 area = new Vector2(4f, 4f);

        [Header("Ember")]
        [Min(0f)] public float rate = 12f;
        public Vector2 size = new Vector2(0.02f, 0.06f);
        public Vector2 lifetime = new Vector2(1.2f, 3f);
        [Tooltip("Upward drift speed.")]
        [Min(0f)] public float riseSpeed = 0.6f;
        [Tooltip("Organic wobble (Noise module).")]
        [Range(0f, 0.3f)] public float floatiness = 0.05f;

        [Header("Color")]
        [ColorUsage(true, true)] public Color startColor = new Color(3f, 1.2f, 0.2f, 1f); // hot orange HDR
        [Tooltip("Color it shifts to at end of life (cooling).")]
        [ColorUsage(true, true)] public Color endColor = new Color(1f, 0.15f, 0f, 1f);

        [Header("Build Reference")]
        [Tooltip("Reference to the ElementalParticle shader so it is included in player builds " +
                 "(a Shader.Find-only shader can be stripped from builds). Assigned in the prefab; " +
                 "falls back to Shader.Find if left empty.")]
        [SerializeField] Shader particleShader;

        ParticleSystem _ps;
        ParticleSystemRenderer _renderer;
        bool _configured;
        static Material s_material;

#if UNITY_EDITOR
        double _lastEditorTime;
#endif

        void Awake() { Configure(); }
        void Reset() { Configure(); }
        void OnValidate() { _configured = false; if (isActiveAndEnabled) Configure(); }

        void Configure()
        {
            if (_configured && _ps != null) return;
            _ps = GetComponent<ParticleSystem>();
            _renderer = GetComponent<ParticleSystemRenderer>();

            var main = _ps.main;
            main.loop = true;
            main.playOnAwake = Application.isPlaying;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 1200;
            main.startSpeed = 0f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime.x, lifetime.y);
            main.startSize = new ParticleSystem.MinMaxCurve(size.x, size.y);
            main.startColor = startColor;
            main.gravityModifier = 0f;

            var em = _ps.emission;
            em.enabled = true;
            em.rateOverTime = rate;

            // thin box along the surface -> spawn over the area
            var shape = _ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(area.x, 0.01f, area.y);

            // steady upward drift
            var vel = _ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.Local;
            // All three axes must use the SAME MinMaxCurve mode (TwoConstants here),
            // otherwise Unity warns "Particle Velocity curves must all be in the same mode".
            vel.x = new ParticleSystem.MinMaxCurve(0f, 0f);
            vel.y = new ParticleSystem.MinMaxCurve(riseSpeed * 0.7f, riseSpeed * 1.3f);
            vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            var noise = _ps.noise;
            noise.enabled = floatiness > 0f;
            noise.strength = floatiness;
            noise.frequency = 0.4f;
            noise.scrollSpeed = 0.2f;

            // color: startColor -> endColor tint -> transparent
            var col = _ps.colorOverLifetime;
            col.enabled = true;
            Color endLdr = ToLdr(endColor);
            var grad = new Gradient();
            grad.SetKeys(
                new[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.Lerp(Color.white, endLdr, 0.7f), 0.6f),
                    new GradientColorKey(endLdr, 1f)
                },
                new[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.2f),
                    new GradientAlphaKey(1f, 0.6f),
                    new GradientAlphaKey(0f, 1f)
                });
            col.color = grad;

            _renderer.renderMode = ParticleSystemRenderMode.Billboard;
            _renderer.alignment = ParticleSystemRenderSpace.View;
            _renderer.sharedMaterial = GetSharedMaterial(particleShader);

            if (Application.isPlaying) _ps.Play();
            _configured = true;
        }

        void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (!_configured) Configure();
                double now = UnityEditor.EditorApplication.timeSinceStartup;
                float dt = (float)(now - _lastEditorTime);
                _lastEditorTime = now;
                if (dt <= 0f || dt > 0.25f) dt = 0.016f;
                if (_ps != null)
                {
                    _ps.Simulate(dt, false, false, false);
                    UnityEditor.SceneView.RepaintAll();
                }
                return;
            }
#endif
            // Play mode: edit-mode Simulate may have left the PS "paused" -> make sure it's playing.
            if (_ps == null) _ps = GetComponent<ParticleSystem>();
            if (_ps != null && !_ps.isPlaying) _ps.Play();
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.5f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(area.x, 0.02f, area.y));
        }

        static Color ToLdr(Color c)
        {
            float m = Mathf.Max(1f, Mathf.Max(c.r, Mathf.Max(c.g, c.b)));
            return new Color(c.r / m, c.g / m, c.b / m, 1f);
        }

        static Material GetSharedMaterial(Shader preferred)
        {
            if (s_material != null) return s_material;
            // Prefer the serialized shader reference (survives build stripping); fall back to Shader.Find.
            var sh = preferred != null ? preferred : Shader.Find("RekindledFX/Elemental/ElementalParticle");
            if (sh == null) sh = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (sh == null)
            {
                Debug.LogWarning("[LavaEmbers] No suitable particle shader found.");
                return null;
            }
            s_material = new Material(sh) { name = "M_LavaEmber (runtime)" };
            if (s_material.HasProperty("_BaseMap")) s_material.SetTexture("_BaseMap", BuildDotTexture(32));
            if (s_material.HasProperty("_BaseColor")) s_material.SetColor("_BaseColor", Color.white);
            return s_material;
        }

        static Texture2D BuildDotTexture(int s)
        {
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float dx = (x + 0.5f) / s - 0.5f;
                float dy = (y + 0.5f) / s - 0.5f;
                float d = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                float a = Mathf.Pow(Mathf.Clamp01(1f - d), 1.7f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
            tex.Apply();
            return tex;
        }
    }
}
