using System.Collections.Generic;
using UnityEngine;

namespace Warplane
{
    /// <summary>
    /// Dependency-free explosion: a few low-poly fireball and smoke spheres that pop, drift and shrink away,
    /// plus a light flash. Works in every render pipeline (no particle shaders). Whoever spawns it sets size.
    /// </summary>
    public class SimpleExplosion : MonoBehaviour
    {
        public Mesh sphereMesh;
        public Material fireMaterial;
        public Material smokeMaterial;
        [Tooltip("Overall scale (units).")]
        public float size = 1f;
        public float duration = 1.4f;
        public int firePuffs = 6;
        public int smokePuffs = 5;
        public bool flashLight = true;
        public Color lightColor = new Color(1f, 0.62f, 0.25f);

        class Puff
        {
            public Transform t;
            public Vector3 velocity;
            public float maxScale, delay, life;
        }

        readonly List<Puff> puffs = new List<Puff>();
        Light flash;
        float age;
        float flashStart;

        void Start()
        {
            if (sphereMesh != null)
            {
                for (int i = 0; i < firePuffs; ++i)
                    puffs.Add(MakePuff(fireMaterial, Random.Range(0.9f, 1.6f), 0f, duration * Random.Range(0.35f, 0.6f), 7f));
                for (int i = 0; i < smokePuffs; ++i)
                    puffs.Add(MakePuff(smokeMaterial, Random.Range(1.4f, 2.4f), duration * Random.Range(0.08f, 0.2f), duration * Random.Range(0.6f, 0.85f), 3f));
            }
            if (flashLight)
            {
                var go = new GameObject("Flash");
                go.transform.SetParent(transform, false);
                flash = go.AddComponent<Light>();
                flash.type = LightType.Point;
                flash.color = lightColor;
                flash.range = 18f * size;
                flashStart = 6f;
                flash.intensity = flashStart;
                flash.shadows = LightShadows.None;
            }
        }

        Puff MakePuff(Material mat, float scale, float delay, float life, float speed)
        {
            if (mat == null) mat = fireMaterial;
            var go = new GameObject("Puff");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Random.insideUnitSphere * 0.4f * size;
            go.transform.localScale = Vector3.zero;
            go.AddComponent<MeshFilter>().sharedMesh = sphereMesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            Vector3 dir = Random.insideUnitSphere;
            dir.y = Mathf.Abs(dir.y) + 0.3f;
            return new Puff { t = go.transform, velocity = dir.normalized * speed * size, maxScale = scale * size * 2f, delay = delay, life = life };
        }

        void Update()
        {
            age += Time.deltaTime;
            foreach (Puff p in puffs)
            {
                float t = (age - p.delay) / p.life;
                if (t < 0f) continue;
                if (t > 1f)
                {
                    if (p.t.gameObject.activeSelf) p.t.gameObject.SetActive(false);
                    continue;
                }
                // pop out fast, then shrink away
                float s = t < 0.25f ? Mathf.SmoothStep(0f, 1f, t / 0.25f) : Mathf.Lerp(1f, 0f, (t - 0.25f) / 0.75f);
                p.t.localScale = Vector3.one * (p.maxScale * s);
                p.t.position += p.velocity * Time.deltaTime;
                p.velocity *= 1f - 2.5f * Time.deltaTime;
                p.velocity += Vector3.up * (2f * size * Time.deltaTime);
            }
            if (flash != null)
                flash.intensity = Mathf.Lerp(flashStart, 0f, age / (duration * 0.4f));
            if (age > duration)
                Destroy(gameObject);
        }
    }
}
