using UnityEngine;

namespace Warplane
{
    /// <summary>
    /// Soft white wake wisps trailing from the wingtips and behind the tail, like light contrails.
    /// Visual only. Their size, length and opacity live in WarplaneSettings (Wake wisps) and change live:
    /// wispSize is the one knob for how big they are. A wingtip's wisp goes with the wingtip when it breaks off.
    /// </summary>
    public class WakeWisps : MonoBehaviour
    {
        public WarplaneSettings settings;
        [Tooltip("Where the wisps come off the plane: empty objects on the wingtips and tail (set up by Tools ▸ Warplane ▸ Build Plane).")]
        public Transform[] anchors = new Transform[0];

        TrailRenderer[] wisps = new TrailRenderer[0];
        WarplaneHealth health;
        float shownOpacity = -1f, shownFadeIn = -1f, shownStart = -1f, shownEnd = -1f;

        void Awake()
        {
            health = GetComponent<WarplaneHealth>();
            Material material = SoftFx.TrailMaterial;
            if (material == null) return;
            wisps = new TrailRenderer[anchors.Length];
            for (int i = 0; i < anchors.Length; ++i)
            {
                if (anchors[i] == null) continue;
                var w = anchors[i].gameObject.AddComponent<TrailRenderer>();
                w.sharedMaterial = material;
                w.minVertexDistance = 0.1f;
                w.textureMode = LineTextureMode.Stretch;
                w.alignment = LineAlignment.View;
                w.numCapVertices = 2;
                w.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                w.receiveShadows = false;
                wisps[i] = w;
            }
        }

        void LateUpdate()
        {
            if (settings == null) return;
            WarplaneSettings s = settings;
            bool rebuild = s.wispOpacity != shownOpacity || s.wispFadeIn != shownFadeIn ||
                           s.wispWidthStart != shownStart || s.wispWidthEnd != shownEnd;
            Gradient fade = rebuild ? Fade(s) : null;
            AnimationCurve width = rebuild ? AnimationCurve.Linear(0f, s.wispWidthStart, 1f, s.wispWidthEnd) : null;
            bool emitting = health == null || !health.IsDead;
            foreach (TrailRenderer w in wisps)
            {
                if (w == null) continue;
                w.emitting = emitting;
                w.time = s.wispLength;
                w.widthMultiplier = s.wispSize;
                if (rebuild)
                {
                    w.colorGradient = fade;
                    w.widthCurve = width;
                }
            }
            if (rebuild)
            {
                shownOpacity = s.wispOpacity;
                shownFadeIn = s.wispFadeIn;
                shownStart = s.wispWidthStart;
                shownEnd = s.wispWidthEnd;
            }
        }

        /// <summary>Drops the current wake, e.g. after a respawn jumps the plane back to its start.</summary>
        public void Clear()
        {
            foreach (TrailRenderer w in wisps)
                if (w != null) w.Clear();
        }

        /// <summary>White, invisible right behind the plane, strongest at wispFadeIn, gone at the end.</summary>
        static Gradient Fade(WarplaneSettings s)
        {
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(s.wispOpacity, Mathf.Max(s.wispFadeIn, 0.01f)),
                    new GradientAlphaKey(s.wispOpacity * 0.5f, Mathf.Lerp(s.wispFadeIn, 1f, 0.5f)),
                    new GradientAlphaKey(0f, 1f),
                });
            return g;
        }
    }
}
