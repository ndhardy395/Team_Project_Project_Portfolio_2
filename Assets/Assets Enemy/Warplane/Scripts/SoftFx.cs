using UnityEngine;

namespace Warplane
{
    /// <summary>
    /// Shared soft-edged materials for trails and glows, made in code so there are no texture files.
    /// They use Sprites/Default: unlit, alpha-blended, tinted by each renderer's colors, works in URP and is
    /// always included in builds. Made once and shared by every plane.
    /// </summary>
    public static class SoftFx
    {
        static Material trail, sprite;
        static Sprite glow;

        /// <summary>For TrailRenderers (wake wisps, gun smoke): soft edges and a few faint streaks along the trail.</summary>
        public static Material TrailMaterial
        {
            get
            {
                // "== null" also catches a material Unity has destroyed, which a plain C# null check wouldn't.
                if (trail == null) trail = MakeTrailMaterial();
                return trail;
            }
        }

        /// <summary>For SpriteRenderers, set explicitly: URP may otherwise hand them a 2D-lit sprite material.</summary>
        public static Material SpriteMaterial
        {
            get
            {
                if (sprite == null)
                {
                    Shader shader = SpritesShader();
                    if (shader != null) sprite = new Material(shader) { name = "Soft Sprite" };
                }
                return sprite;
            }
        }

        /// <summary>A round soft glow, 1 unit across: brightest in the middle, fading to nothing at the edge.</summary>
        public static Sprite GlowSprite
        {
            get
            {
                if (glow == null) glow = MakeGlowSprite();
                return glow;
            }
        }

        static Shader SpritesShader()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) Debug.LogWarning("Warplane: Sprites/Default shader not found; wisps, smoke and glows won't be drawn.");
            return shader;
        }

        static Material MakeTrailMaterial()
        {
            Shader shader = SpritesShader();
            if (shader == null) return null;
            const int W = 64, H = 32;   // u runs along the trail, v across it
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { name = "Soft Trail", wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < H; ++y)
            {
                float v = (y + 0.5f) / H;
                float edge = Mathf.Pow(Mathf.Sin(v * Mathf.PI), 1.5f);
                for (int x = 0; x < W; ++x)
                {
                    float u = (x + 0.5f) / W;
                    float streaks = 0.55f + 0.45f * Mathf.PerlinNoise(u * 2f, v * 9f);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, edge * streaks));
                }
            }
            tex.Apply();
            return new Material(shader) { name = "Soft Trail", mainTexture = tex };
        }

        static Sprite MakeGlowSprite()
        {
            const int S = 64;
            var tex = new Texture2D(S, S, TextureFormat.RGBA32, false) { name = "Soft Glow", wrapMode = TextureWrapMode.Clamp };
            float c = (S - 1) * 0.5f;
            for (int y = 0; y < S; ++y)
            {
                for (int x = 0; x < S; ++x)
                {
                    float d = Mathf.Clamp01(new Vector2(x - c, y - c).magnitude / c);   // 0 center, 1 edge
                    float a = (1f - d) * (1f - d);                                       // bright core, long soft falloff
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply();
            // pixelsPerUnit = the texture's width, so the sprite is 1 unit across and scale = size in units
            Sprite s = Sprite.Create(tex, new Rect(0f, 0f, S, S), new Vector2(0.5f, 0.5f), S);
            s.name = "Soft Glow";
            return s;
        }
    }
}
