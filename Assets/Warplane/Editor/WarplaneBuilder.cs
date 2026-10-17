using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Warplane
{
    /// <summary>
    /// Tools ▸ Warplane ▸ Build Plane: builds the warplane from Data/warplane_mesh.json (the model, exported
    /// from the Dogfighting project's generator, unpainted): materials for this project's render pipeline,
    /// meshes scaled to WarplaneSettings.wingspan, and the prefabs (Warplane, bullet, bomb, explosion) in
    /// Generated/. Safe to rerun, e.g. after changing the wingspan: assets are updated in place.
    /// Tools ▸ Warplane ▸ Add Plane And Route To Scene: puts a plane and a 4-waypoint loop in the open scene
    /// (and a route drawer, if the scene has none).
    /// Tools ▸ Warplane ▸ Add Route Drawer To Scene: lets the player draw routes (press on a plane and drag).
    /// </summary>
    static class WarplaneBuilder
    {
        const string DataName = "warplane_mesh";

#pragma warning disable 0649   // filled in by JsonUtility
        [Serializable] class KitFile { public MatDef[] materials; public PartDef[] parts; public PartDef[] library; public AnchorDef[] anchors; public ColliderDef[] colliders; }
        [Serializable] class MatDef { public string name; public float[] color; public float metallic, smoothness; public bool unlit; }
        [Serializable] class SubDef { public string mat; public int[] tris; }
        [Serializable]
        class PartDef
        {
            public string name, parent, channel, breakable, role;
            public float[] pivot, pos, nrm, axis;
            public float sign;
            public int side;
            public SubDef[] sub;
        }
        [Serializable] class AnchorDef { public string name; public float[] pos, fwd; }
        [Serializable]
        class ColliderDef
        {
            public string name, type, part;
            public float[] center, size, euler;
            public float radius, height;
            public int direction;
        }
#pragma warning restore 0649

        class Context
        {
            public string root, gen;
            public KitFile data;
            public WarplaneSettings settings;
            public float scale;
            public readonly Dictionary<string, Material> mats = new Dictionary<string, Material>();
            public readonly Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
            public Mesh sphere, cube;
        }

        // ------------------------------------------------------------------ menu
        [MenuItem("Tools/Warplane/Build Plane")]
        static void Build()
        {
            var ctx = new Context { root = FindRoot() };
            if (ctx.root == null) return;
            ctx.gen = ctx.root + "/Generated";
            var text = AssetDatabase.LoadAssetAtPath<TextAsset>(ctx.root + "/Data/" + DataName + ".json");
            ctx.data = JsonUtility.FromJson<KitFile>(text.text);
            ctx.settings = LoadOrCreateSettings(ctx.root);
            float span = ModelSpan(ctx.data);
            ctx.scale = span > 0f ? ctx.settings.wingspan / span : 1f;

            try
            {
                EditorUtility.DisplayProgressBar("Warplane", "Materials...", 0.1f);
                EnsureFolder(ctx.gen + "/Materials");
                EnsureFolder(ctx.gen + "/Meshes");
                EnsureFolder(ctx.gen + "/Prefabs");
                BuiltinMeshes(ctx);
                BuildMaterials(ctx);

                EditorUtility.DisplayProgressBar("Warplane", "Meshes...", 0.35f);
                foreach (PartDef p in ctx.data.parts) ctx.meshes[p.name] = BuildMesh(ctx, p);
                foreach (PartDef p in ctx.data.library)
                    if (p.name == "Bomb") ctx.meshes[p.name] = BuildMesh(ctx, p);

                EditorUtility.DisplayProgressBar("Warplane", "Prefabs...", 0.7f);
                GameObject explosion = SavePrefab(MakeExplosion(ctx), ctx.gen + "/Prefabs/Warplane Explosion.prefab");
                GameObject bullet = SavePrefab(MakeBullet(ctx), ctx.gen + "/Prefabs/Warplane Bullet.prefab");
                GameObject bomb = SavePrefab(MakeBomb(ctx, explosion), ctx.gen + "/Prefabs/Warplane Bomb.prefab");
                GameObject plane = SavePrefab(MakePlane(ctx, explosion, bullet.GetComponent<WarplaneBullet>(), bomb.GetComponent<WarplaneBomb>()),
                                              ctx.gen + "/Prefabs/Warplane.prefab");
                AssetDatabase.SaveAssets();
                Selection.activeObject = plane;
                EditorGUIUtility.PingObject(plane);
                Debug.Log($"Warplane: built {ctx.gen}/Prefabs/Warplane.prefab ({ctx.settings.wingspan:0.##} units across, " +
                          $"{ctx.data.parts.Length} parts, render pipeline: {PipelineName()}). " +
                          "Next: Tools ▸ Warplane ▸ Add Plane And Route To Scene.");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        [MenuItem("Tools/Warplane/Add Plane And Route To Scene")]
        static void AddToScene()
        {
            string root = FindRoot();
            if (root == null) return;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(root + "/Generated/Prefabs/Warplane.prefab");
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("Warplane", "Build the plane first: Tools ▸ Warplane ▸ Build Plane.", "OK");
                return;
            }
            WarplaneSettings settings = LoadOrCreateSettings(root);

            // Centered where the Scene view is looking, placeHeight above the ground there.
            Vector3 c = SceneView.lastActiveSceneView != null ? SceneView.lastActiveSceneView.pivot : Vector3.zero;
            float ground = c.y;
            if (Physics.Raycast(new Vector3(c.x, c.y + 500f, c.z), Vector3.down, out RaycastHit hit, 1000f)) ground = hit.point.y;
            float y = ground + settings.placeHeight;

            var routeGo = new GameObject("Warplane Route");
            routeGo.transform.position = new Vector3(c.x, y, c.z);
            WarplaneRoute route = routeGo.AddComponent<WarplaneRoute>();
            Vector2[] corners = { new Vector2(-12f, 7f), new Vector2(12f, 7f), new Vector2(12f, -7f), new Vector2(-12f, -7f) };
            for (int i = 0; i < corners.Length; ++i)
            {
                var wp = new GameObject("Waypoint " + (i + 1)).transform;
                wp.SetParent(routeGo.transform, false);
                wp.localPosition = new Vector3(corners[i].x, 0f, corners[i].y);
            }

            var plane = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Vector3 start = route.Point(0), toward = route.Point(1) - start;
            toward.y = 0f;
            plane.transform.SetPositionAndRotation(start, Quaternion.LookRotation(toward));
            plane.GetComponent<WarplaneFlight>().route = route;

            Undo.RegisterCreatedObjectUndo(routeGo, "Add Warplane");
            Undo.RegisterCreatedObjectUndo(plane, "Add Warplane");
            AddRouteDrawer();
            EditorSceneManager.MarkSceneDirty(plane.scene);
            Selection.activeGameObject = plane;
            Debug.Log($"Warplane: added a plane and a route to {plane.scene.name}, {settings.placeHeight:0.#} units above the ground. " +
                      "Drag the route's waypoints to reshape the loop, then save the scene.");
        }

        [MenuItem("Tools/Warplane/Add Route Drawer To Scene")]
        static void AddRouteDrawerMenu()
        {
            if (AddRouteDrawer())
                Debug.Log("Warplane: added a Warplane Route Drawer. In Play, press on a plane and drag to draw its route. Save the scene.");
            else
                Debug.Log("Warplane: this scene already has a Warplane Route Drawer.");
        }

        /// <summary>Adds the scene's route drawer if it has none. True if it added one.</summary>
        static bool AddRouteDrawer()
        {
            if (Object.FindAnyObjectByType<RouteDrawer>() != null) return false;
            var go = new GameObject("Warplane Route Drawer");
            go.AddComponent<RouteDrawer>();
            Undo.RegisterCreatedObjectUndo(go, "Add Warplane Route Drawer");
            EditorSceneManager.MarkSceneDirty(go.scene);
            return true;
        }

        // ------------------------------------------------------------------ setup
        static string FindRoot()
        {
            foreach (string guid in AssetDatabase.FindAssets(DataName + " t:TextAsset"))
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(p) == DataName)
                    return Path.GetDirectoryName(Path.GetDirectoryName(p)).Replace('\\', '/');   // <root>/Data/warplane_mesh.json
            }
            EditorUtility.DisplayDialog("Warplane", "Can't find Data/" + DataName + ".json in the Warplane folder.", "OK");
            return null;
        }

        static WarplaneSettings LoadOrCreateSettings(string root)
        {
            string path = root + "/Data/WarplaneSettings.asset";
            var s = AssetDatabase.LoadAssetAtPath<WarplaneSettings>(path);
            if (s != null) return s;
            s = ScriptableObject.CreateInstance<WarplaneSettings>();
            AssetDatabase.CreateAsset(s, path);
            return s;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        /// <summary>The model's wingtip-to-wingtip width in the data's own units.</summary>
        static float ModelSpan(KitFile data)
        {
            float min = float.MaxValue, max = float.MinValue;
            foreach (PartDef p in data.parts)
                for (int i = 0; i < p.pos.Length; i += 3)
                {
                    float x = p.pivot[0] + p.pos[i];
                    min = Mathf.Min(min, x);
                    max = Mathf.Max(max, x);
                }
            return max - min;
        }

        static Vector3 V(float[] a) => a == null || a.Length < 3 ? Vector3.zero : new Vector3(a[0], a[1], a[2]);

        static void BuiltinMeshes(Context ctx)
        {
            var s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ctx.sphere = s.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(s);
            var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ctx.cube = c.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(c);
        }

        // ------------------------------------------------------------------ materials
        enum Pipeline { BuiltIn, URP, HDRP }

        static Pipeline CurrentPipeline()
        {
            RenderPipelineAsset rp = GraphicsSettings.currentRenderPipeline;
            if (rp == null) return Pipeline.BuiltIn;
            string n = rp.GetType().Name;
            return n.Contains("HDRenderPipeline") ? Pipeline.HDRP : Pipeline.URP;
        }

        static string PipelineName() => CurrentPipeline().ToString();

        static Shader FirstShader(params string[] names)
        {
            foreach (string n in names)
            {
                Shader s = Shader.Find(n);
                if (s != null) return s;
            }
            return Shader.Find("Standard");
        }

        static Shader LitShader()
        {
            switch (CurrentPipeline())
            {
                case Pipeline.URP: return FirstShader("Universal Render Pipeline/Lit", "Universal Render Pipeline/Simple Lit");
                case Pipeline.HDRP: return FirstShader("HDRP/Lit");
                default: return FirstShader("Standard");
            }
        }

        static Shader UnlitShader()
        {
            switch (CurrentPipeline())
            {
                case Pipeline.URP: return FirstShader("Universal Render Pipeline/Unlit");
                case Pipeline.HDRP: return FirstShader("HDRP/Unlit");
                default: return FirstShader("Unlit/Color");
            }
        }

        static void BuildMaterials(Context ctx)
        {
            foreach (MatDef d in ctx.data.materials)
                MakeMaterial(ctx, d.name, new Color(d.color[0], d.color[1], d.color[2]), d.metallic, d.smoothness, d.unlit);
            MakeMaterial(ctx, "Tracer", new Color(1f, 0.85f, 0.35f), 0f, 0f, true);
            MakeMaterial(ctx, "Fire", new Color(1f, 0.55f, 0.12f), 0f, 0f, true);
            MakeMaterial(ctx, "Smoke", new Color(0.18f, 0.17f, 0.16f), 0f, 0.1f, false);
        }

        static void MakeMaterial(Context ctx, string name, Color color, float metallic, float smoothness, bool unlit)
        {
            string path = ctx.gen + "/Materials/M_" + name + ".mat";
            Shader shader = unlit ? UnlitShader() : LitShader();
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(shader);
                AssetDatabase.CreateAsset(m, path);
            }
            else if (m.shader != shader)
            {
                m.shader = shader;
            }
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_Color")) m.SetColor("_Color", color);
            if (m.HasProperty("_UnlitColor")) m.SetColor("_UnlitColor", color);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smoothness);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smoothness);
            m.enableInstancing = true;
            EditorUtility.SetDirty(m);
            ctx.mats[name] = m;
        }

        static Material[] PartMaterials(Context ctx, PartDef p)
        {
            var arr = new Material[p.sub.Length];
            for (int i = 0; i < arr.Length; ++i)
                arr[i] = ctx.mats.TryGetValue(p.sub[i].mat, out Material m) ? m : ctx.mats["Silver"];
            return arr;
        }

        // ------------------------------------------------------------------ meshes
        static Mesh BuildMesh(Context ctx, PartDef p)
        {
            string path = ctx.gen + "/Meshes/" + p.name + ".asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            bool isNew = mesh == null;
            if (isNew) mesh = new Mesh();
            mesh.Clear();
            mesh.name = p.name;

            int n = p.pos.Length / 3;
            var verts = new Vector3[n];
            var norms = new Vector3[n];
            for (int i = 0; i < n; ++i)
            {
                verts[i] = new Vector3(p.pos[3 * i], p.pos[3 * i + 1], p.pos[3 * i + 2]) * ctx.scale;
                norms[i] = new Vector3(p.nrm[3 * i], p.nrm[3 * i + 1], p.nrm[3 * i + 2]);
            }
            mesh.indexFormat = n > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            mesh.vertices = verts;
            mesh.normals = norms;
            mesh.subMeshCount = p.sub.Length;
            for (int s = 0; s < p.sub.Length; ++s) mesh.SetTriangles(p.sub[s].tris, s);
            mesh.RecalculateBounds();

            if (isNew) AssetDatabase.CreateAsset(mesh, path);
            else EditorUtility.SetDirty(mesh);
            return mesh;
        }

        static GameObject SavePrefab(GameObject go, string path)
        {
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        static void NoShadows(Renderer r)
        {
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        // ------------------------------------------------------------------ small prefabs
        static GameObject MakeExplosion(Context ctx)
        {
            var go = new GameObject("Warplane Explosion");
            var fx = go.AddComponent<SimpleExplosion>();
            fx.sphereMesh = ctx.sphere;
            fx.fireMaterial = ctx.mats["Fire"];
            fx.smokeMaterial = ctx.mats["Smoke"];
            return go;
        }

        static GameObject MakeBullet(Context ctx)
        {
            var go = new GameObject("Warplane Bullet");
            var b = go.AddComponent<WarplaneBullet>();
            var tracer = new GameObject("Tracer");
            tracer.transform.SetParent(go.transform, false);
            tracer.AddComponent<MeshFilter>().sharedMesh = ctx.cube;
            var mr = tracer.AddComponent<MeshRenderer>();
            mr.sharedMaterial = ctx.mats["Tracer"];
            NoShadows(mr);
            b.tracer = tracer.transform;
            return go;
        }

        static GameObject MakeBomb(Context ctx, GameObject explosion)
        {
            var go = new GameObject("Warplane Bomb");
            Mesh mesh = ctx.meshes["Bomb"];
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterials = PartMaterials(ctx, Library(ctx, "Bomb"));
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.linearDamping = 0f;   // so it falls exactly where WarplaneBombs.PredictLanding says
            rb.angularDamping = 0.5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var col = go.AddComponent<CapsuleCollider>();
            Bounds b = mesh.bounds;
            col.direction = 2;
            col.center = b.center;
            col.radius = Mathf.Max(b.extents.x, b.extents.y);
            col.height = b.size.z;
            go.AddComponent<WarplaneBomb>().explosionPrefab = explosion;
            return go;
        }

        static PartDef Library(Context ctx, string name)
        {
            foreach (PartDef p in ctx.data.library)
                if (p.name == name) return p;
            return null;
        }

        // ------------------------------------------------------------------ the plane
        static GameObject MakePlane(Context ctx, GameObject explosion, WarplaneBullet bullet, WarplaneBomb bomb)
        {
            WarplaneSettings s = ctx.settings;
            float k = ctx.scale;
            var root = new GameObject("Warplane");
            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            var flight = root.AddComponent<WarplaneFlight>();
            var health = root.AddComponent<WarplaneHealth>();
            var structure = root.AddComponent<PlaneStructure>();
            var guns = root.AddComponent<WarplaneGuns>();
            var bombs = root.AddComponent<WarplaneBombs>();
            var glow = root.AddComponent<BarrelGlow>();
            var wisps = root.AddComponent<WakeWisps>();
            var warning = root.AddComponent<OverheatIndicator>();

            // ---- model parts, nested (a wing carries its tip, flap, ailerons and guns)
            var model = new GameObject("Model");
            model.transform.SetParent(root.transform, false);
            var defs = new Dictionary<string, PartDef>();
            var placed = new Dictionary<string, Transform>();
            var breakables = new Dictionary<string, BreakablePart>();
            var barrels = new List<Renderer>();
            foreach (PartDef p in ctx.data.parts)
            {
                defs[p.name] = p;
                bool nested = !string.IsNullOrEmpty(p.parent) && placed.ContainsKey(p.parent);
                var t = new GameObject(p.name).transform;
                t.SetParent(nested ? placed[p.parent] : model.transform, false);
                t.localPosition = (V(p.pivot) - (nested ? V(defs[p.parent].pivot) : Vector3.zero)) * k;
                t.gameObject.AddComponent<MeshFilter>().sharedMesh = ctx.meshes[p.name];
                var mr = t.gameObject.AddComponent<MeshRenderer>();
                mr.sharedMaterials = PartMaterials(ctx, p);
                placed[p.name] = t;

                if (p.name == "Propeller") t.gameObject.AddComponent<PropellerSpin>().settings = s;
                if (!string.IsNullOrEmpty(p.channel))
                {
                    var surface = t.gameObject.AddComponent<MovingSurface>();
                    surface.settings = s;
                    surface.kind = p.channel == "roll" ? MovingSurface.Kind.Aileron
                                 : p.channel == "pitch" ? MovingSurface.Kind.Elevator
                                 : p.channel == "yaw" ? MovingSurface.Kind.Rudder : MovingSurface.Kind.Flap;
                    surface.hingeAxis = V(p.axis).normalized;
                    surface.sign = p.sign;
                }
                if (!string.IsNullOrEmpty(p.breakable))
                {
                    var part = t.gameObject.AddComponent<BreakablePart>();
                    part.kind = p.breakable == "wing" ? PartKind.Wing : p.breakable == "tail" ? PartKind.Tail : PartKind.WingTip;
                    part.side = p.side;
                    breakables[p.name] = part;
                }
                if (p.role == "guns")
                {
                    NoShadows(mr);
                    barrels.Add(mr);
                }
            }

            // ---- colliders (to be shot at), each listed on the part it belongs to
            var colliderRoot = new GameObject("Colliders");
            colliderRoot.transform.SetParent(root.transform, false);
            var partColliders = new Dictionary<string, List<Collider>>();
            foreach (ColliderDef c in ctx.data.colliders)
            {
                var g = new GameObject(c.name);
                g.transform.SetParent(colliderRoot.transform, false);
                g.transform.localPosition = V(c.center) * k;
                g.transform.localRotation = Quaternion.Euler(V(c.euler));
                Collider col;
                if (c.type == "capsule")
                {
                    var cap = g.AddComponent<CapsuleCollider>();
                    cap.radius = c.radius * k;
                    cap.height = c.height * k;
                    cap.direction = c.direction;
                    col = cap;
                }
                else
                {
                    var box = g.AddComponent<BoxCollider>();
                    box.size = V(c.size) * k;
                    col = box;
                }
                if (string.IsNullOrEmpty(c.part)) continue;
                if (!partColliders.ContainsKey(c.part)) partColliders[c.part] = new List<Collider>();
                partColliders[c.part].Add(col);
            }
            foreach (KeyValuePair<string, BreakablePart> kv in breakables)
                kv.Value.colliders = partColliders.TryGetValue(kv.Key, out List<Collider> list) ? list.ToArray() : new Collider[0];

            // ---- gun muzzles and bomb hardpoints (each hardpoint rides on its wing, with its bomb showing)
            var muzzleRoot = new GameObject("Muzzles").transform;
            muzzleRoot.SetParent(root.transform, false);
            var muzzles = new List<Transform>();
            var hardpoints = new List<Transform>();
            var mounted = new List<GameObject>();
            foreach (AnchorDef a in ctx.data.anchors)
            {
                Vector3 at = V(a.pos) * k;   // the root sits at the origin, so plane space = world space here
                if (a.name.StartsWith("Muzzle"))
                {
                    var m = new GameObject(a.name).transform;
                    m.SetParent(muzzleRoot, false);
                    m.SetPositionAndRotation(at, Quaternion.LookRotation(V(a.fwd)));
                    muzzles.Add(m);
                }
                else if (a.name.StartsWith("Hardpoint"))
                {
                    string wing = at.x >= 0f ? "Wing_R" : "Wing_L";
                    var h = new GameObject(a.name).transform;
                    h.SetParent(placed.TryGetValue(wing, out Transform w) ? w : model.transform, false);
                    h.SetPositionAndRotation(at, Quaternion.LookRotation(V(a.fwd)));
                    var vis = new GameObject("MountedBomb");
                    vis.transform.SetParent(h, false);
                    vis.AddComponent<MeshFilter>().sharedMesh = ctx.meshes["Bomb"];
                    vis.AddComponent<MeshRenderer>().sharedMaterials = PartMaterials(ctx, Library(ctx, "Bomb"));
                    hardpoints.Add(h);
                    mounted.Add(vis);
                }
            }

            // ---- wake wisps: off each wingtip's outer trailing corner (riding on the tip), and behind the tail
            var wispAnchors = new List<Transform>();
            foreach (BreakablePart part in breakables.Values)
            {
                Bounds b = part.GetComponent<MeshFilter>().sharedMesh.bounds;
                if (part.kind == PartKind.WingTip)
                {
                    var w = new GameObject("Wisp").transform;
                    w.SetParent(part.transform, false);
                    w.localPosition = new Vector3(part.side > 0 ? b.max.x : b.min.x, b.center.y, b.min.z);
                    wispAnchors.Add(w);
                }
                else if (part.kind == PartKind.Tail)
                {
                    var w = new GameObject("Wisp").transform;
                    w.SetParent(part.transform, false);
                    float backZ = part.transform.TransformPoint(new Vector3(0f, 0f, b.min.z)).z;
                    w.position = new Vector3(0f, 0f, backZ);   // on the fuselage line
                    wispAnchors.Add(w);
                }
            }

            // ---- wire it up
            flight.settings = s;
            health.settings = s;
            health.model = model;
            health.colliders = colliderRoot;
            health.explosionPrefab = explosion;
            structure.settings = s;
            guns.settings = s;
            guns.muzzles = muzzles.ToArray();
            guns.bulletPrefab = bullet;
            bombs.settings = s;
            bombs.bombPrefab = bomb;
            bombs.hardpoints = hardpoints.ToArray();
            bombs.mountedVisuals = mounted.ToArray();
            glow.settings = s;
            glow.barrels = barrels.ToArray();
            wisps.settings = s;
            wisps.anchors = wispAnchors.ToArray();
            warning.settings = s;
            return root;
        }
    }
}
