using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Builds the level select from the whiteboard sketch and the Level Select mockup:
// a small 3D map made from the same environment packs as Path Scene_Mikey (the Mini Village ground, water
// and houses, Meadow Forest trees and rocks, grass-stone pads), with the mockup's UI drawn on top.
//
// Run it from Tools > Skyline Rush > Build Level Select. It:
//   1. makes the Level Data assets (Assets/Scenes/LevelSelect/Levels) if they don't exist yet,
//   2. builds Assets/Scenes/LevelSelect/LevelSelect.prefab from scratch,
//   3. puts one copy of that prefab in the UI scene and saves the scene.
// Running it again throws the old prefab away and rebuilds it, so change layouts here, not by hand in the prefab.
// Level Data assets are only created once, so edits to them (names, scenes, preview pictures) are kept.
//
// Positions are written in mockup pixels (1280x720, 0,0 = top-left) and projected onto the 3D ground,
// so the map lines up with the mockup at 16:9.
public static class LevelSelectBuilder
{
    const string Folder = "Assets/Scenes/LevelSelect/";
    const string LevelsFolder = Folder + "Levels/";
    const string MaterialsFolder = Folder + "Materials/";
    const string PrefabPath = Folder + "LevelSelect.prefab";
    const string UIScenePath = "Assets/Scenes/UI.unity";
    // Mini Village props come from the FBX models, like Path Scene_Mikey uses them; the pack's own prefabs
    // point at materials that render pink (magenta) in URP.
    const string Village = "Assets/Enviroment/Low_Poly_Mini_Village/Art/Meshes/";
    const string Forest = "Assets/Enviroment/Meadow Forest Props/Prefabs/";

    const float MockW = 1280f;
    const float MockH = 720f;
    const float CoastX = 80f;       // the shoreline runs down the left edge of the screen at this x

    const float CameraPitch = 58f;
    const float CameraDistance = 40f;
    const float CameraFov = 42f;

    // ---------- The map layout (mockup pixels) ----------

    class NodeDef
    {
        public string id, file, displayName, label, number, scene;
        public LevelNode.Kind kind;
        public Vector2 pos;
        public string[] requires;
        public int keys;
        public bool givesKey;
        public LevelData data;
        public Vector3 world;
    }

    static NodeDef Level(string id, string file, string name, string number, float x, float y, string scene, params string[] requires) =>
        new NodeDef { id = id, file = file, displayName = name, label = "LEVEL " + number, number = number, scene = scene, kind = LevelNode.Kind.Level, pos = new Vector2(x, y), requires = requires };

    static NodeDef KeyLevel(string id, string file, string letter, float x, float y, string requires) =>
        new NodeDef { id = id, file = file, displayName = "Key Level " + letter, label = "KEY LEVEL " + letter, kind = LevelNode.Kind.Key, pos = new Vector2(x, y), requires = new[] { requires }, givesKey = true };

    // In play order (the NEXT tag goes to the first open, unbeaten normal level).
    // Only level 1 has a scene so far; the rest show "Coming soon" until someone fills in their Level Data.
    // Which levels the first playable locks is set on the Level Data assets (Coming Soon), not here.
    static NodeDef[] Nodes() => new[]
    {
        Level("level_01", "Level01_CoastalChaos", "Coastal Chaos", "1", 190, 390, "Path Scene_Mikey"),
        Level("level_02", "Level02", "Level 2", "2", 420, 180, null, "level_01"),
        Level("level_03", "Level03", "Level 3", "3", 450, 450, null, "level_02"),
        Level("level_04", "Level04", "Level 4", "4", 640, 630, null, "level_03"),
        Level("level_05", "Level05", "Level 5", "5", 660, 380, null, "level_04"),
        Level("level_06", "Level06", "Level 6", "6", 960, 170, null, "level_05"),
        Level("level_07", "Level07", "Level 7", "7", 930, 430, null, "level_06"),
        KeyLevel("key_a", "KeyLevelA", "A", 320, 320, "level_02"),
        KeyLevel("key_b", "KeyLevelB", "B", 560, 230, "level_05"),
        KeyLevel("key_c", "KeyLevelC", "C", 1150, 400, "level_07"),
        new NodeDef { id = "boss", file = "Boss", displayName = "Boss", label = "BOSS LEVEL", kind = LevelNode.Kind.Boss, pos = new Vector2(1110, 590), requires = new[] { "level_07" }, keys = 3 },
    };

    class TrailDef
    {
        public string from, to;
        public bool key;
        public Vector2[] curve;   // cubic Beziers chained end to start: p0, c1, c2, p1, c1, c2, p2, ...
    }

    static TrailDef Trail(string from, string to, bool key, params float[] xy)
    {
        Vector2[] points = new Vector2[xy.Length / 2];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vector2(xy[i * 2], xy[i * 2 + 1]);
        return new TrailDef { from = from, to = to, key = key, curve = points };
    }

    // The dashed paths from the mockup's SVG.
    static TrailDef[] Trails() => new[]
    {
        Trail("level_01", "level_02", false, 190, 390, 150, 280, 220, 130, 330, 125, 390, 122, 415, 150, 420, 180),
        Trail("level_02", "level_03", false, 420, 180, 470, 260, 470, 360, 450, 450),
        Trail("level_03", "level_04", false, 450, 450, 430, 560, 540, 640, 640, 630),
        Trail("level_04", "level_05", false, 640, 630, 760, 615, 760, 430, 660, 380),
        Trail("level_05", "level_06", false, 660, 380, 700, 300, 700, 160, 800, 125, 880, 100, 940, 130, 960, 170),
        Trail("level_06", "level_07", false, 960, 170, 990, 250, 950, 350, 930, 430),
        Trail("level_07", "boss", false, 930, 430, 910, 500, 1040, 560, 1110, 590),
        Trail("level_02", "key_a", true, 420, 180, 390, 230, 340, 270, 320, 320),
        Trail("level_05", "key_b", true, 660, 380, 630, 330, 590, 280, 560, 230),
        Trail("level_07", "key_c", true, 930, 430, 1010, 420, 1090, 410, 1150, 400),
    };

    // ---------- Build ----------

    class Sprites
    {
        public Sprite star, starOutline, lockIcon, lockOpen, key, back, play, close, castle, circle, ringDashed, rounded, chamfer, vignette;
        public Sprite[] faces;
    }

    class Mats
    {
        public Material ground, stone, shore, road, roadLocked, keyRoad, keyRoadLocked;
    }

    static Camera cam;
    static MeshCollider groundCollider;   // only exists while building, so props can be dropped onto the hills
    static Sprites sprites;
    static Dictionary<TMP_FontAsset, Material> textMaterials = new Dictionary<TMP_FontAsset, Material>();
    static TMP_FontAsset bodyFont;
    static TMP_FontAsset displayFont;

    [MenuItem("Tools/Skyline Rush/Build Level Select")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        textMaterials.Clear();
        EnsureFolder(LevelsFolder);
        EnsureFolder(MaterialsFolder);
        LoadArt();
        LoadFonts();
        Mats mats = MakeMaterials();
        NodeDef[] nodes = Nodes();
        TrailDef[] trailDefs = Trails();
        MakeLevelData(nodes);

        Scene scene = EditorSceneManager.OpenScene(UIScenePath, OpenSceneMode.Single);
        foreach (GameObject old in scene.GetRootGameObjects())
        {
            if (old.GetComponent<LevelSelectScreen>() != null)
                Object.DestroyImmediate(old);
        }

        GameObject root = new GameObject("LevelSelect");
        LevelSelectScreen screen = root.AddComponent<LevelSelectScreen>();

        // 3D map
        GameObject world = new GameObject("Map");
        world.transform.SetParent(root.transform, false);
        cam = MakeCamera(world.transform);
        MakeLight(world.transform);
        MakeGround(world.transform, mats);

        Dictionary<string, NodeDef> byId = new Dictionary<string, NodeDef>();
        foreach (NodeDef node in nodes)
        {
            node.world = Ground(node.pos.x, node.pos.y);
            byId[node.id] = node;
        }

        MakeClearings(world.transform, nodes, mats);   // also lifts each stop onto the top of its pad
        MakeCoast(world.transform, mats);
        Transform anchors = Child("Anchors", world.transform);
        Dictionary<string, Transform> anchorById = new Dictionary<string, Transform>();
        foreach (NodeDef node in nodes)
        {
            Transform anchor = Child("Anchor " + node.id, anchors);
            anchor.position = node.world;
            anchorById[node.id] = anchor;
        }

        List<LevelTrail> trails = new List<LevelTrail>();
        List<Vector2> trailSamples = new List<Vector2>();
        Transform trailRoot = Child("Trails", world.transform);
        foreach (TrailDef trail in trailDefs)
        {
            trails.Add(MakeTrail(trailRoot, trail, byId[trail.from], byId[trail.to], mats));
            trailSamples.AddRange(SampleCurve(trail.curve, 60));
        }

        MakeScenery(world.transform, nodes, trailSamples);
        Object.DestroyImmediate(groundCollider);
        groundCollider = null;

        // Screen UI
        Canvas canvas = MakeCanvas(root.transform);
        RectTransform canvasRect = (RectTransform)canvas.transform;
        Image vignette = Paint(Stretch(Rect("Vignette", canvasRect)), sprites.vignette, Color.white);
        vignette.raycastTarget = false;

        RectTransform nodeLayer = Stretch(Rect("Nodes", canvasRect));
        List<LevelNode> levelNodes = new List<LevelNode>();
        foreach (NodeDef node in nodes)
            levelNodes.Add(MakeNode(nodeLayer, node, anchorById[node.id]));

        TopBar bar = MakeTopBar(canvasRect);
        LevelDetailsPopup popup = MakePopup(canvasRect);

        cam.ResetAspect();

        SerializedObject so = new SerializedObject(screen);
        so.FindProperty("world").objectReferenceValue = world;
        so.FindProperty("mapCamera").objectReferenceValue = cam;
        SetArray(so.FindProperty("trails"), trails.ToArray());
        so.FindProperty("roadLit").objectReferenceValue = mats.road;
        so.FindProperty("roadUnlit").objectReferenceValue = mats.roadLocked;
        so.FindProperty("keyRoadLit").objectReferenceValue = mats.keyRoad;
        so.FindProperty("keyRoadUnlit").objectReferenceValue = mats.keyRoadLocked;
        so.FindProperty("canvas").objectReferenceValue = canvas;
        so.FindProperty("nodeLayer").objectReferenceValue = nodeLayer;
        SetArray(so.FindProperty("nodes"), levelNodes.ToArray());
        so.FindProperty("popup").objectReferenceValue = popup;
        so.FindProperty("backButton").objectReferenceValue = bar.back;
        so.FindProperty("shopButton").objectReferenceValue = bar.shop;
        so.FindProperty("starsText").objectReferenceValue = bar.stars;
        so.FindProperty("keysText").objectReferenceValue = bar.keys;
        so.FindProperty("keysForBoss").intValue = byId["boss"].keys;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAssetAndConnect(root, PrefabPath, InteractionMode.AutomatedAction);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = root;
        Debug.Log($"Level select built: {PrefabPath}, placed in {UIScenePath}.");
    }

    // ---------- Assets ----------

    static void EnsureFolder(string path)
    {
        path = path.TrimEnd('/');
        if (AssetDatabase.IsValidFolder(path))
            return;
        int slash = path.LastIndexOf('/');
        AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
    }

    static void LoadArt()
    {
        // Reimport so the sprite settings from LevelSelectArtImporter are applied even if the PNGs came in before it compiled.
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { LevelSelectArtImporter.Folder.TrimEnd('/') }))
            AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid), ImportAssetOptions.ForceUpdate);

        sprites = new Sprites
        {
            star = Art("star"),
            starOutline = Art("star_outline"),
            lockIcon = Art("lock"),
            lockOpen = Art("lock_open"),
            key = Art("key"),
            back = Art("back"),
            play = Art("play"),
            close = Art("close"),
            castle = Art("castle"),
            circle = Art("circle"),
            ringDashed = Art("ring_dashed"),
            rounded = Art("rounded"),
            chamfer = Art("chamfer"),
            vignette = Art("vignette"),
            faces = new[] { Art("face_annoyance"), Art("face_anger"), Art("face_violence"), Art("face_bloodlust") },
        };
    }

    static Sprite Art(string file)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(LevelSelectArtImporter.Folder + file + ".png");
        if (sprite == null)
            Debug.LogWarning($"Level select builder: couldn't load the sprite {file}.png");
        return sprite;
    }

    // The mockup uses Black Ops One (titles, level numbers) and Chakra Petch (everything else).
    // If TextMeshPro font assets with those names are in the project they're used; otherwise TMP's default font.
    static void LoadFonts()
    {
        bodyFont = TMP_Settings.defaultFontAsset;
        displayFont = null;
        foreach (string guid in AssetDatabase.FindAssets("t:TMP_FontAsset"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = System.IO.Path.GetFileNameWithoutExtension(path).Replace(" ", "");
            if (name.Contains("BlackOps"))
                displayFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            else if (name.Contains("ChakraPetch"))
                bodyFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        }
        if (displayFont == null)
            displayFont = bodyFont;
    }

    static Mats MakeMaterials()
    {
        return new Mats
        {
            // Copies, so retiling them for the big map doesn't change Path Scene_Mikey's materials.
            ground = Brighten(CopyMaterial(Village + "Materials/Gnd.mat", "LS_Ground", Vector2.one)),
            stone = CopyMaterial("Assets/Enviroment/HandPainted Ground Textures/Materials/Albedo_GrassStone.mat", "LS_Stone", Vector2.one),
            shore = CopyMaterial("Assets/Enviroment/HandPainted Ground Textures/Materials/Albedo_GrassStone.mat", "LS_Shore", new Vector2(1, 14)),
            road = FlatColor("LS_Road", LevelSelectStyle.Gold),
            roadLocked = FlatColor("LS_RoadLocked", LevelSelectStyle.BossBorder),
            keyRoad = FlatColor("LS_KeyRoad", LevelSelectStyle.Key),
            keyRoadLocked = FlatColor("LS_KeyRoadLocked", LevelSelectStyle.KeyLockedBorder),
        };
    }

    static Material CopyMaterial(string sourcePath, string name, Vector2 tiling)
    {
        string path = MaterialsFolder + name + ".mat";
        Material source = AssetDatabase.LoadAssetAtPath<Material>(sourcePath);
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = source != null ? new Material(source) : new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(material, path);
        }
        else if (source != null)
        {
            material.shader = source.shader;
            material.CopyPropertiesFromMaterial(source);
        }
        if (material.HasProperty("_BaseMap"))
            material.SetTextureScale("_BaseMap", tiling);
        EditorUtility.SetDirty(material);
        return material;
    }

    // The village ground material is tinted 60% gray, which makes the grass look muddy from this high up.
    // Full brightness is too yellow behind the gold NEXT badge and trails, so it's lifted only part way.
    static Material Brighten(Material material)
    {
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", new Color(0.78f, 0.78f, 0.78f));
        return material;
    }

    // Unlit, so the trail colors read exactly like the mockup's dashed lines whatever the lighting.
    static Material FlatColor(string name, Color color)
    {
        string path = MaterialsFolder + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetColor("_BaseColor", color);
        EditorUtility.SetDirty(material);
        return material;
    }

    static void MakeLevelData(NodeDef[] nodes)
    {
        Dictionary<string, NodeDef> byId = new Dictionary<string, NodeDef>();
        List<NodeDef> created = new List<NodeDef>();
        foreach (NodeDef node in nodes)
        {
            byId[node.id] = node;
            string path = LevelsFolder + node.file + ".asset";
            node.data = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (node.data != null)
                continue;

            LevelData data = ScriptableObject.CreateInstance<LevelData>();
            data.levelId = node.id;
            data.displayName = node.displayName;
            data.sceneName = node.scene;
            data.keysRequired = node.keys;
            data.givesKey = node.givesKey;
            AssetDatabase.CreateAsset(data, path);
            node.data = data;
            created.Add(node);
        }

        // Second pass, once every asset exists, so "required levels" can point at each other.
        foreach (NodeDef node in created)
        {
            node.data.requiredLevels = new LevelData[node.requires.Length];
            for (int i = 0; i < node.requires.Length; i++)
                node.data.requiredLevels[i] = byId[node.requires[i]].data;
            EditorUtility.SetDirty(node.data);
        }
        AssetDatabase.SaveAssets();
    }

    // ---------- 3D map ----------

    // Where a mockup pixel lands on the ground, seen through the map camera at 16:9.
    // Hits the hilly ground model while it has its build-time collider, otherwise the flat y = 0 plane.
    static Vector3 Ground(float x, float y)
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(x / MockW, 1f - y / MockH, 0f));
        if (groundCollider != null && groundCollider.Raycast(ray, out RaycastHit hit, 500f))
            return hit.point;
        float distance = -ray.origin.y / Mathf.Min(ray.direction.y, -0.01f);
        Vector3 point = ray.origin + ray.direction * distance;
        point.y = 0f;
        return point;
    }

    // Height of the ground straight below a point.
    static float HeightAt(Vector3 point)
    {
        Ray down = new Ray(new Vector3(point.x, 100f, point.z), Vector3.down);
        if (groundCollider != null && groundCollider.Raycast(down, out RaycastHit hit, 200f))
            return hit.point.y;
        return 0f;
    }

    static Camera MakeCamera(Transform parent)
    {
        GameObject go = new GameObject("Map Camera");
        go.transform.SetParent(parent, false);
        Quaternion rotation = Quaternion.Euler(CameraPitch, 0f, 0f);
        go.transform.SetPositionAndRotation(rotation * new Vector3(0f, 0f, -CameraDistance), rotation);

        Camera camera = go.AddComponent<Camera>();
        camera.fieldOfView = CameraFov;
        camera.nearClipPlane = 0.5f;
        camera.farClipPlane = 200f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = LevelSelectStyle.Background;
        camera.depth = 10;   // draws over the UI scene's own camera while the map is showing
        camera.aspect = MockW / MockH;
        return camera;
    }

    // Same angle as Path Scene_Mikey's sun. Together with the UI scene's own light it matches the level's brightness.
    static void MakeLight(Transform parent)
    {
        GameObject go = new GameObject("Map Light");
        go.transform.SetParent(parent, false);
        go.transform.rotation = new Quaternion(0.40821788f, -0.23456968f, 0.10938163f, 0.8754261f);
        Light light = go.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.95686275f, 0.8392157f);
        light.intensity = 1f;
        light.shadows = LightShadows.Soft;
    }

    // The Mini Village's Ground model: a low-poly grass tile with hills. It's stretched to 160 units across
    // (enough for ultrawide screens) with its hills squashed to gentle bumps, and given a collider for the
    // rest of the build so pads, trails and props can be dropped onto it.
    const float GroundWidth = 160f;
    const float HillSquash = 0.15f;   // the model's hills are up to ~6 units tall; this keeps them under ~1

    static void MakeGround(Transform parent, Mats mats)
    {
        Transform root = Child("Ground", parent);
        root.position = new Vector3(0f, 0f, 5f);
        GameObject model = Instantiate(Village + "Ground.FBX", root);
        if (model == null)
        {
            GameObject plane = Primitive(PrimitiveType.Plane, "Ground", root, mats.ground);
            plane.transform.localScale = new Vector3(GroundWidth / 10f, 1f, GroundWidth / 10f);
            return;
        }

        foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>())
            renderer.sharedMaterial = mats.ground;
        // The model is centered on its pivot with its flat rim at height 0, so only the root needs scaling.
        float width = Measure(model).size.x;
        float spread = GroundWidth / Mathf.Max(width, 0.01f);
        root.localScale = new Vector3(spread, HillSquash, spread);

        MeshFilter mesh = model.GetComponentInChildren<MeshFilter>();
        groundCollider = mesh.gameObject.AddComponent<MeshCollider>();
        groundCollider.sharedMesh = mesh.sharedMesh;
        Physics.SyncTransforms();
    }

    // "Coastal Chaos": water along the left edge, with a stone quay, a boat and some rocks.
    static void MakeCoast(Transform parent, Mats mats)
    {
        Transform coast = Child("Coast", parent);
        Vector3 near = Ground(CoastX, MockH + 80f);
        Vector3 far = Ground(CoastX, -120f);
        near.y = far.y = 0f;   // measure the coastline flat; heights are handled below
        Vector3 along = (far - near).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, along);
        Quaternion rotation = Quaternion.LookRotation(along);
        float length = Vector3.Distance(near, far) + 10f;
        Vector3 middle = (near + far) * 0.5f;

        // The sea sits just above the highest bit of ground along the left edge, so no hill pokes through it.
        float seaLevel = 0f;
        for (float y = -120f; y <= MockH + 80f; y += 40f)
        {
            for (float x = 0f; x <= CoastX + 30f; x += 15f)
                seaLevel = Mathf.Max(seaLevel, Ground(x, y).y);
        }
        seaLevel += 0.05f;

        // A stone quay along the waterline. It's a slab rather than a flat sheet so it reads as solid on the hills.
        GameObject shore = Primitive(PrimitiveType.Cube, "Shore", coast, mats.shore);
        Vector3 quay = middle + right * 0.6f;
        shore.transform.SetPositionAndRotation(new Vector3(quay.x, seaLevel + 0.03f - 0.5f, quay.z), rotation);
        shore.transform.localScale = new Vector3(3.6f, 1f, length);

        // The Mini Village Water model, the same one Path Scene_Mikey uses. Its top is low-poly waves; like the level,
        // it's squashed flat, and it's lifted so the wave troughs (not the crests) sit at sea level.
        const float waterWidth = 50f;
        const float waveSquash = 0.15f;
        Transform waterRoot = Child("Water", coast);
        GameObject water = Instantiate(Village + "Water.FBX", waterRoot);
        if (water != null)
        {
            water.transform.localPosition = Vector3.zero;
            Bounds bounds = Measure(water);
            float troughs = bounds.center.y + bounds.extents.y * 0.72f;   // the waves fill the top ~28% of the model
            water.transform.localPosition = new Vector3(-bounds.center.x, -troughs, -bounds.center.z);
            waterRoot.localScale = new Vector3(waterWidth / Mathf.Max(bounds.size.x, 0.01f), waveSquash, length / Mathf.Max(bounds.size.z, 0.01f));
            Vector3 center = middle - right * (waterWidth * 0.5f);
            waterRoot.SetPositionAndRotation(new Vector3(center.x, seaLevel, center.z), rotation);
        }

        Vector3 OnShore(float x, float y)
        {
            Vector3 point = Ground(x, y);
            point.y = Mathf.Max(point.y, seaLevel + 0.03f);
            return point;
        }

        Vector3 boat = Ground(38, 585);
        Prop(coast, Forest + "NF_Prop_Boat_01.prefab", new Vector3(boat.x, seaLevel - 0.12f, boat.z), 3.6f, false, 35f);
        Prop(coast, Forest + "NF_Prop_Rock_05.prefab", OnShore(92, 300), 1.3f, false, 20f);
        Prop(coast, Forest + "NF_Prop_Rock_12.prefab", OnShore(86, 520), 1.1f, false, 140f);
        Prop(coast, Forest + "NF_Prop_Rock_03.prefab", OnShore(96, 650), 1.4f, false, 75f);
        Prop(coast, Village + "Rock_2.FBX", OnShore(90, 180), 1.0f, false, 200f);
    }

    // A round cobblestone pad under every stop, so the UI badges sit on something.
    // Each pad is a short plinth whose top clears the hill it sits on, and the stop is moved up onto it.
    static void MakeClearings(Transform parent, NodeDef[] nodes, Mats mats)
    {
        Transform clearings = Child("Pads", parent);
        foreach (NodeDef node in nodes)
        {
            float size = node.kind == LevelNode.Kind.Boss ? 5.6f : node.kind == LevelNode.Kind.Key ? 3.2f : 4.2f;
            float top = HeightAt(node.world);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI / 4f;
                top = Mathf.Max(top, HeightAt(node.world + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * size * 0.45f));
            }
            top += 0.04f;

            GameObject disc = Primitive(PrimitiveType.Cylinder, "Pad " + node.id, clearings, mats.stone);
            disc.transform.position = new Vector3(node.world.x, top - 0.3f, node.world.z);
            disc.transform.localScale = new Vector3(size, 0.3f, size);   // a cylinder is 2 units tall, so this is 0.6 deep
            node.world.y = top;
        }
    }

    // How far trails stop short of a stop: just past the edge of its pad.
    static float Clearance(NodeDef node) =>
        node.kind == LevelNode.Kind.Boss ? 2.95f : node.kind == LevelNode.Kind.Key ? 1.75f : 2.25f;

    static LevelTrail MakeTrail(Transform parent, TrailDef def, NodeDef from, NodeDef to, Mats mats)
    {
        GameObject go = new GameObject($"Trail {from.id} > {to.id}");
        go.transform.SetParent(parent, false);
        LevelTrail trail = go.AddComponent<LevelTrail>();
        trail.from = from.data;
        trail.keyBranch = def.key;

        // Walk the curve on the ground and drop a flat dash every so often, leaving gaps around the stops.
        List<Vector3> points = new List<Vector3>();
        foreach (Vector2 p in SampleCurve(def.curve, 48))
            points.Add(Ground(p.x, p.y));
        float[] distance = new float[points.Count];
        for (int i = 1; i < points.Count; i++)
            distance[i] = distance[i - 1] + Vector3.Distance(points[i - 1], points[i]);
        float total = distance[distance.Length - 1];

        float dash = def.key ? 0.55f : 0.9f;
        float gap = def.key ? 0.55f : 0.6f;
        float width = def.key ? 0.32f : 0.44f;
        Material material = def.key ? mats.keyRoadLocked : mats.roadLocked;

        List<Renderer> dashes = new List<Renderer>();
        for (float s = Clearance(from) + dash * 0.5f; s <= total - Clearance(to) - dash * 0.5f; s += dash + gap)
        {
            Vector3 position = PointAt(points, distance, s);
            Vector3 direction = PointAt(points, distance, s + 0.05f) - PointAt(points, distance, s - 0.05f);
            GameObject piece = Primitive(PrimitiveType.Capsule, "Dash", go.transform, material);
            // A capsule lies along its Y axis; turn it flat and point it down the trail, then squash it.
            piece.transform.SetPositionAndRotation(position + Vector3.up * 0.08f,
                Quaternion.LookRotation(direction.normalized) * Quaternion.Euler(90f, 0f, 0f));
            piece.transform.localScale = new Vector3(width, dash * 0.5f, 0.06f);
            Renderer renderer = piece.GetComponent<Renderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            dashes.Add(renderer);
        }
        trail.dashes = dashes.ToArray();
        return trail;
    }

    static Vector3 PointAt(List<Vector3> points, float[] distance, float s)
    {
        s = Mathf.Clamp(s, 0f, distance[distance.Length - 1]);
        for (int i = 1; i < points.Count; i++)
        {
            if (distance[i] >= s)
            {
                float t = Mathf.InverseLerp(distance[i - 1], distance[i], s);
                return Vector3.Lerp(points[i - 1], points[i], t);
            }
        }
        return points[points.Count - 1];
    }

    static List<Vector2> SampleCurve(Vector2[] curve, int stepsPerSegment)
    {
        List<Vector2> samples = new List<Vector2> { curve[0] };
        for (int i = 0; i + 3 < curve.Length; i += 3)
        {
            for (int step = 1; step <= stepsPerSegment; step++)
            {
                float t = step / (float)stepsPerSegment;
                float u = 1f - t;
                samples.Add(u * u * u * curve[i] + 3f * u * u * t * curve[i + 1] + 3f * u * t * t * curve[i + 2] + t * t * t * curve[i + 3]);
            }
        }
        return samples;
    }

    // ---------- Scenery ----------

    // A box in mockup pixels that scattered props must stay out of (the UI badges, labels and hand-placed buildings).
    struct KeepOut
    {
        public Vector2 center, half;
        public KeepOut(float x, float y, float halfW, float halfH) { center = new Vector2(x, y); half = new Vector2(halfW, halfH); }
        public bool Contains(Vector2 p) => Mathf.Abs(p.x - center.x) < half.x && Mathf.Abs(p.y - center.y) < half.y;
    }

    struct Solid
    {
        public Vector3 position;
        public float radius;
    }

    // How one kind of prop gets scattered.
    class Scatter
    {
        public string name;
        public string[] prefabs;
        public int count;
        public float minSize, maxSize;
        public bool byHeight;
        public float trailClear;    // pixels to keep from the trail lines
        public float tallPx;        // roughly how far up the screen the prop reaches, so tall things don't cover a trail or badge
        public float widePx;        // roughly how wide it looks on screen
        public float radius;        // world units other props keep away from it (0 = can touch anything)
        public bool village;        // only in the village corner (top right)
        public bool notVillage;     // never in the village corner
    }

    static bool InVillage(Vector2 p) => p.x > 1010f && p.y < 370f;

    static void MakeScenery(Transform parent, NodeDef[] nodes, List<Vector2> trailSamples)
    {
        Transform scenery = Child("Scenery", parent);
        List<KeepOut> keepOut = new List<KeepOut>();
        List<Solid> solids = new List<Solid>();

        foreach (NodeDef node in nodes)
        {
            if (node.kind == LevelNode.Kind.Level)
                keepOut.Add(new KeepOut(node.pos.x, node.pos.y + 4f, 50f, 56f));     // circle, NEXT tag and stars
            else if (node.kind == LevelNode.Kind.Key)
                keepOut.Add(new KeepOut(node.pos.x, node.pos.y + 10f, 46f, 42f));    // circle and KEY EARNED
            else
                keepOut.Add(new KeepOut(node.pos.x, node.pos.y, 82f, 88f));          // locks, castle and caption
        }

        // Hand-placed landmarks: (prefab, x, y, size, size is height?, yaw)
        void Landmark(string prefab, float x, float y, float size, bool byHeight, float yaw, float tallPx, float widePx)
        {
            GameObject go = Prop(scenery, prefab, Ground(x, y), size, byHeight, yaw);
            if (go == null)
                return;
            keepOut.Add(new KeepOut(x, y - tallPx * 0.5f + 6f, widePx * 0.5f, tallPx * 0.5f + 12f));
            Bounds bounds = Measure(go);
            solids.Add(new Solid { position = bounds.center, radius = Mathf.Max(bounds.extents.x, bounds.extents.z) });
        }

        // The village, top right.
        Landmark(Village + "House_1.FBX", 1078, 232, 4.4f, false, 196f, 70f, 105f);
        Landmark(Village + "House_2.FBX", 1192, 178, 4.4f, false, 170f, 70f, 105f);
        Landmark(Village + "House_3.FBX", 1218, 318, 4.0f, false, 188f, 66f, 95f);
        Landmark(Village + "Well1.FBX", 1132, 300, 1.6f, false, 0f, 30f, 40f);
        Landmark(Village + "Electric_pole.FBX", 1040, 335, 4.0f, true, 180f, 60f, 20f);
        Landmark(Village + "Electric_pole.FBX", 1262, 240, 4.0f, true, 180f, 60f, 20f);
        Landmark(Village + "Wooden_barrel.FBX", 1146, 240, 0.8f, true, 30f, 18f, 18f);
        Landmark(Village + "Wooden_barrel.FBX", 1160, 250, 0.8f, true, 80f, 18f, 18f);
        Landmark(Village + "Caravan_1.FBX", 1240, 470, 3.0f, false, 120f, 40f, 70f);

        // Near level 1: a sign on the road out of town.
        Landmark(Village + "Road_sign1.FBX", 252, 468, 1.6f, true, 160f, 30f, 20f);

        // A camp in the loop between levels 3, 4 and 5.
        Landmark(Forest + "NF_Struct_Tent_01.prefab", 585, 505, 3.0f, false, 200f, 50f, 70f);
        Landmark(Forest + "NF_Prop_Campfire_01.prefab", 622, 530, 1.1f, false, 0f, 16f, 26f);
        Landmark(Forest + "NF_Prop_WoodenCart_01.prefab", 545, 470, 2.0f, false, 60f, 30f, 46f);

        // The boss: a watchtower and cabin on the far side, with a few graves out front.
        Landmark(Forest + "NF_Struct_Watchtower_01.prefab", 1222, 640, 7.5f, true, 200f, 110f, 70f);
        Landmark(Forest + "NF_Struct_Cabin_02.prefab", 985, 668, 4.2f, false, 160f, 60f, 95f);
        Landmark(Forest + "NF_Prop_Campfire_01.prefab", 1060, 694, 1.1f, false, 0f, 16f, 26f);
        Landmark(Forest + "NF_Prop_Gravestone_01.prefab", 1168, 702, 0.9f, true, 190f, 16f, 16f);
        Landmark(Forest + "NF_Prop_Gravestone_03.prefab", 1196, 690, 0.9f, true, 170f, 16f, 16f);

        Scatter[] scatters =
        {
            new Scatter { name = "Trees", prefabs = Paths(Forest, ".prefab", "NF_Prop_Pine_01", "NF_Prop_Pine_02", "NF_Prop_Pine_03", "NF_Prop_Pine_01", "NF_Prop_Oak_02", "NF_Prop_Tree_01", "NF_Prop_Tree_02"),
                count = 34, minSize = 4.2f, maxSize = 5.8f, byHeight = true, trailClear = 24f, tallPx = 62f, widePx = 58f, radius = 1.5f, notVillage = true },
            new Scatter { name = "Village Trees", prefabs = Paths(Village, ".FBX", "Tree_1", "Tree_2", "Tree_3", "Tree_4"),
                count = 7, minSize = 3.5f, maxSize = 4.5f, byHeight = true, trailClear = 24f, tallPx = 52f, widePx = 50f, radius = 1.3f, village = true },
            new Scatter { name = "Bushes", prefabs = Paths(Forest, ".prefab", "NF_Prop_Bush_01", "NF_Prop_Bush_02", "NF_Prop_Bush_03", "NF_Prop_Bush_04"),
                count = 60, minSize = 0.8f, maxSize = 1.3f, trailClear = 16f, tallPx = 14f, widePx = 30f, radius = 0.6f },
            new Scatter { name = "Grass", prefabs = Paths(Village, ".FBX", "Grass_1", "Grass_2", "Grass_3"),
                count = 150, minSize = 0.6f, maxSize = 1.0f, trailClear = 10f, tallPx = 0f, widePx = 0f, radius = 0f },
            new Scatter { name = "Rocks", prefabs = Paths(Forest, ".prefab", "NF_Prop_Rock_01", "NF_Prop_Rock_03", "NF_Prop_Rock_05", "NF_Prop_Rock_08", "NF_Prop_Rock_12", "NF_Prop_Rock_15", "NF_Prop_Rock_19", "NF_Prop_Rock_23"),
                count = 22, minSize = 0.6f, maxSize = 1.4f, trailClear = 14f, tallPx = 8f, widePx = 24f, radius = 0.6f },
            new Scatter { name = "Mushrooms", prefabs = Paths(Forest, ".prefab", "NF_Prop_Mushrooms_01", "NF_Prop_Mushrooms_02", "NF_Prop_Mushrooms_04", "NF_Prop_Mushrooms_06"),
                count = 14, minSize = 0.4f, maxSize = 0.6f, trailClear = 10f, tallPx = 0f, widePx = 0f, radius = 0.3f, notVillage = true },
            new Scatter { name = "Stumps", prefabs = Paths(Forest, ".prefab", "NF_Prop_Stump_01", "NF_Prop_Stump_02", "NF_Prop_TreeStump_01", "NF_Prop_TreeStump_02"),
                count = 8, minSize = 0.7f, maxSize = 1.0f, trailClear = 12f, tallPx = 6f, widePx = 20f, radius = 0.5f, notVillage = true },
        };

        // Fixed seed: the same map every time the builder runs.
        System.Random random = new System.Random(2026);
        foreach (Scatter scatter in scatters)
        {
            Transform group = Child(scatter.name, scenery);
            int placed = 0;
            for (int attempt = 0; attempt < scatter.count * 40 && placed < scatter.count; attempt++)
            {
                Vector2 p = new Vector2((float)random.NextDouble() * (MockW + 40f) - 20f, 40f + (float)random.NextDouble() * (MockH + 10f));
                if (p.x < CoastX + 34f)
                    continue;   // water and shore
                if (scatter.village && !InVillage(p))
                    continue;
                if (scatter.notVillage && InVillage(p))
                    continue;
                if (!ClearOnScreen(p, scatter, keepOut, trailSamples))
                    continue;

                Vector3 position = Ground(p.x, p.y);
                float size = Mathf.Lerp(scatter.minSize, scatter.maxSize, (float)random.NextDouble());
                float footprint = scatter.radius * size / Mathf.Max(scatter.maxSize, 0.01f);
                if (footprint > 0f && Overlaps(position, footprint, solids))
                    continue;

                string prefab = scatter.prefabs[random.Next(scatter.prefabs.Length)];
                if (Prop(group, prefab, position, size, scatter.byHeight, (float)random.NextDouble() * 360f) == null)
                    break;
                if (footprint > 0f)
                    solids.Add(new Solid { position = position, radius = footprint });
                placed++;
            }
        }
    }

    // Checks the spot and the strip of screen the prop will cover above it.
    static bool ClearOnScreen(Vector2 p, Scatter scatter, List<KeepOut> keepOut, List<Vector2> trailSamples)
    {
        float halfWide = scatter.widePx * 0.5f;
        for (float up = 0f; up <= scatter.tallPx; up += 8f)
        {
            for (float side = -halfWide; side <= halfWide; side += Mathf.Max(halfWide, 1f))
            {
                Vector2 q = new Vector2(p.x + side, p.y - up);
                foreach (KeepOut box in keepOut)
                {
                    if (box.Contains(q))
                        return false;
                }
                float clear = up == 0f && side == 0f ? scatter.trailClear : scatter.trailClear * 0.6f;
                foreach (Vector2 s in trailSamples)
                {
                    if ((s - q).sqrMagnitude < clear * clear)
                        return false;
                }
            }
        }
        return true;
    }

    static bool Overlaps(Vector3 position, float radius, List<Solid> solids)
    {
        foreach (Solid solid in solids)
        {
            float min = solid.radius + radius;
            Vector3 offset = solid.position - position;
            offset.y = 0f;
            if (offset.sqrMagnitude < min * min)
                return true;
        }
        return false;
    }

    static string[] Paths(string folder, string extension, params string[] names)
    {
        string[] paths = new string[names.Length];
        for (int i = 0; i < names.Length; i++)
            paths[i] = folder + names[i] + extension;
        return paths;
    }

    // Places a prefab so its footprint is centered on the point and its bottom sits on the ground,
    // scaled so its height (or widest side) is `size` units. The packs were modeled at different scales.
    static GameObject Prop(Transform parent, string prefabPath, Vector3 position, float size, bool byHeight, float yaw)
    {
        GameObject go = Instantiate(prefabPath, parent);
        if (go == null)
            return null;

        // Keep the model's own rotation (FBX imports often stand on a -90 degree X turn) and add the yaw on top.
        Quaternion modelRotation = go.transform.localRotation;
        go.transform.position = Vector3.zero;
        Bounds bounds = Measure(go);
        float current = byHeight ? bounds.size.y : Mathf.Max(bounds.size.x, bounds.size.z);
        if (current > 0.0001f)
            go.transform.localScale *= size / current;

        go.transform.rotation = Quaternion.Euler(0f, yaw, 0f) * modelRotation;
        bounds = Measure(go);
        go.transform.position = new Vector3(position.x - bounds.center.x, position.y - bounds.min.y, position.z - bounds.center.z);
        return go;
    }

    static GameObject Instantiate(string prefabPath, Transform parent)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"Level select builder: missing prefab {prefabPath}");
            return null;
        }
        return (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
    }

    static Bounds Measure(GameObject go)
    {
        Bounds bounds = new Bounds(go.transform.position, Vector3.zero);
        bool any = false;
        foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
        {
            if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer))
                continue;
            if (any)
                bounds.Encapsulate(renderer.bounds);
            else
                bounds = renderer.bounds;
            any = true;
        }
        return bounds;
    }

    static GameObject Primitive(PrimitiveType type, string name, Transform parent, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial = material;
        return go;
    }

    static Transform Child(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    // ---------- Screen UI ----------

    static Canvas MakeCanvas(Transform parent)
    {
        GameObject go = new GameObject("Level Select UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(parent, false);
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;   // above ClickCode's menus
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(MockW, MockH);   // 1 UI unit = 1 mockup pixel at 1280x720
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    static LevelNode MakeNode(RectTransform layer, NodeDef def, Transform anchor)
    {
        RectTransform rect = Center(Rect("Node " + def.id, layer), 0f, 0f, 0f, 0f);
        LevelNode node = rect.gameObject.AddComponent<LevelNode>();
        node.level = def.data;
        node.kind = def.kind;
        node.label = def.label;
        node.number = def.number;
        node.anchor = anchor;
        node.lockSprite = sprites.lockIcon;
        node.lockOpenSprite = sprites.lockOpen;
        node.starSprite = sprites.star;
        node.starOutlineSprite = sprites.starOutline;

        if (def.kind == LevelNode.Kind.Level)
        {
            node.glowOuter = Paint(Center(Rect("Glow Outer", rect), 0f, 0f, 92f, 92f), sprites.circle, WithAlpha(LevelSelectStyle.Gold, 0.08f));
            node.glowInner = Paint(Center(Rect("Glow Inner", rect), 0f, 0f, 78f, 78f), sprites.circle, WithAlpha(LevelSelectStyle.Gold, 0.22f));
            node.ring = Paint(Center(Rect("Ring", rect), 0f, 0f, 56f, 56f), sprites.circle, LevelSelectStyle.Gold);
            node.fill = Paint(Center(Rect("Fill", rect), 0f, 0f, 50f, 50f), sprites.circle, LevelSelectStyle.NodeFill);
            node.icon = Paint(Center(Rect("Lock", rect), 0f, 0f, 18f, 18f), sprites.lockIcon, LevelSelectStyle.LockedIcon);
            node.numberText = Label(Center(Rect("Number", rect), 0f, 1f, 60f, 40f), def.number, 22f, LevelSelectStyle.Text, false, TextAlignmentOptions.Center, 0f, displayFont);

            node.starRow = Center(Rect("Stars", rect), 0f, -43.5f, 66f, 15f);
            node.stars = new Image[4];
            for (int i = 0; i < 4; i++)
                node.stars[i] = Paint(Center(Rect("Star " + (i + 1), node.starRow), -25.5f + i * 17f, 0f, 15f, 15f), sprites.star, LevelSelectStyle.StarEmpty);

            node.nextTag = Center(Rect("Next", rect), 0f, 51f, 54f, 22f);
            Paint(node.nextTag, sprites.rounded, LevelSelectStyle.Gold, 4f);
            Label(Stretch(Rect("Text", node.nextTag)), "NEXT", 12f, LevelSelectStyle.OnGold, true, TextAlignmentOptions.Center, 14f);

            // COMING SOON, in the stars' spot, once the player has earned a level this build doesn't include.
            node.caption = Label(Center(Rect("Caption", rect), 0f, -39f, 140f, 14f), "COMING SOON", 11f, LevelSelectStyle.TextMuted, true, TextAlignmentOptions.Center, 14f);
            node.caption.gameObject.SetActive(false);
        }
        else if (def.kind == LevelNode.Kind.Key)
        {
            node.ring = Paint(Center(Rect("Ring", rect), 0f, 0f, 44f, 44f), sprites.circle, LevelSelectStyle.Key);
            node.dashedRing = Paint(Center(Rect("Dashed Ring", rect), 0f, 0f, 44f, 44f), sprites.ringDashed, LevelSelectStyle.KeyLockedBorder);
            node.fill = Paint(Center(Rect("Fill", rect), 0f, 0f, 40f, 40f), sprites.circle, LevelSelectStyle.KeyFill);
            node.icon = Paint(Center(Rect("Key", rect), 0f, 0f, 20f, 20f), sprites.key, LevelSelectStyle.Key);
            node.caption = Label(Center(Rect("Caption", rect), 0f, -35f, 140f, 14f), "KEY EARNED", 11f, LevelSelectStyle.Key, true, TextAlignmentOptions.Center, 14f);
        }
        else
        {
            node.locks = new Image[def.keys];
            for (int i = 0; i < def.keys; i++)
                node.locks[i] = Paint(Center(Rect("Lock " + (i + 1), rect), (i - (def.keys - 1) * 0.5f) * 28f, 59f, 22f, 22f), sprites.lockIcon, LevelSelectStyle.LockedIcon);
            node.ring = Paint(Center(Rect("Ring", rect), 0f, 0f, 84f, 84f), sprites.rounded, LevelSelectStyle.BossBorder, 14f);
            node.fill = Paint(Center(Rect("Fill", rect), 0f, 0f, 80f, 80f), sprites.rounded, LevelSelectStyle.LockedFill, 12f);
            node.icon = Paint(Center(Rect("Castle", rect), 0f, 0f, 48f, 48f), sprites.castle, LevelSelectStyle.BossIcon);
            node.caption = Label(Center(Rect("Caption", rect), 0f, -56f, 220f, 16f), "BOSS · 0/3 KEYS", 12f, LevelSelectStyle.TextMuted, true, TextAlignmentOptions.Center, 14f);
        }

        node.ring.raycastTarget = true;
        node.button = MakeButton(rect.gameObject, node.ring);
        return node;
    }

    class TopBar
    {
        public Button back, shop;
        public TMP_Text stars, keys;
    }

    static TopBar MakeTopBar(RectTransform canvas)
    {
        TopBar bar = new TopBar();
        RectTransform top = Rect("Top Bar", canvas);
        top.anchorMin = new Vector2(0f, 1f);
        top.anchorMax = new Vector2(1f, 1f);
        top.pivot = new Vector2(0.5f, 1f);
        top.anchoredPosition = Vector2.zero;
        top.sizeDelta = new Vector2(0f, 76f);
        Paint(top, null, WithAlpha(LevelSelectStyle.Background, 0.94f)).raycastTarget = true;

        RectTransform line = Rect("Bottom Line", top);
        line.anchorMin = Vector2.zero;
        line.anchorMax = new Vector2(1f, 0f);
        line.pivot = new Vector2(0.5f, 0f);
        line.sizeDelta = new Vector2(0f, 1f);
        Paint(line, null, LevelSelectStyle.Hex("#1C2939"));

        // Back (left)
        RectTransform back = Corner(Rect("Back", top), false, 24f, 16f, 112f, 44f);
        Image backFill = Box(back, LevelSelectStyle.Hex("#16212E"), LevelSelectStyle.Hex("#36485D"), 8f, 1f, true);
        Paint(At(Rect("Icon", back), 14f, 12f, 20f, 20f), sprites.back, LevelSelectStyle.Text);
        Label(At(Rect("Text", back), 42f, 0f, 64f, 44f), "BACK", 15f, LevelSelectStyle.Text, true, TextAlignmentOptions.MidlineLeft, 10f);
        bar.back = MakeButton(back.gameObject, backFill);

        // Title (center)
        RectTransform title = Rect("Title", top);
        title.anchorMin = title.anchorMax = new Vector2(0.5f, 1f);
        title.pivot = new Vector2(0.5f, 1f);
        title.anchoredPosition = new Vector2(0f, -19f);
        title.sizeDelta = new Vector2(420f, 38f);
        Label(title, "Level Select", 28f, LevelSelectStyle.Text, false, TextAlignmentOptions.Center, 4f, displayFont);

        // Stars, keys and Shop (right)
        RectTransform shop = Corner(Rect("Shop", top), true, 24f, 16f, 92f, 44f);
        Image shopFill = Paint(shop, sprites.chamfer, LevelSelectStyle.Gold, 10f);
        shopFill.raycastTarget = true;
        Label(Stretch(Rect("Text", shop)), "SHOP", 15f, LevelSelectStyle.OnGold, true, TextAlignmentOptions.Center, 12f);
        bar.shop = MakeButton(shop.gameObject, shopFill);

        RectTransform keys = Corner(Rect("Keys", top), true, 126f, 16f, 104f, 44f);
        Box(keys, LevelSelectStyle.LockedFill, LevelSelectStyle.LockedBorder, 22f);
        Paint(At(Rect("Icon", keys), 14f, 12f, 20f, 20f), sprites.key, LevelSelectStyle.Key);
        bar.keys = Label(At(Rect("Count", keys), 42f, 0f, 56f, 44f), "<b>0</b><color=#7F92A8> / 3</color>", 17f, LevelSelectStyle.Text, false, TextAlignmentOptions.MidlineLeft);

        RectTransform stars = Corner(Rect("Stars", top), true, 240f, 16f, 84f, 44f);
        Box(stars, LevelSelectStyle.LockedFill, LevelSelectStyle.LockedBorder, 22f);
        Paint(At(Rect("Icon", stars), 14f, 12f, 20f, 20f), sprites.star, LevelSelectStyle.Star);
        bar.stars = Label(At(Rect("Count", stars), 42f, 0f, 36f, 44f), "<b>0</b>", 17f, LevelSelectStyle.Text, false, TextAlignmentOptions.MidlineLeft);
        return bar;
    }

    static LevelDetailsPopup MakePopup(RectTransform canvas)
    {
        RectTransform root = Stretch(Rect("Level Details", canvas));
        LevelDetailsPopup popup = root.gameObject.AddComponent<LevelDetailsPopup>();
        popup.starSprite = sprites.star;
        popup.starOutlineSprite = sprites.starOutline;

        // The backdrop is a sibling of the dialog, not its parent, so clicks inside the dialog don't bubble up and close it.
        RectTransform backdrop = Stretch(Rect("Backdrop", root));
        Image dim = Paint(backdrop, null, new Color(5f / 255f, 8f / 255f, 12f / 255f, 0.74f));
        dim.raycastTarget = true;
        popup.backdrop = backdrop.gameObject.AddComponent<Button>();
        popup.backdrop.transition = Selectable.Transition.None;

        // 840 x 491, the mockup's dialog, centered.
        RectTransform dialog = Center(Rect("Dialog", root), 0f, 10f, 840f, 491f);
        Box(dialog, LevelSelectStyle.LockedFill, LevelSelectStyle.LockedBorder, 12f, 1f, true);
        Color line = LevelSelectStyle.Hex("#22303F");
        Color card = LevelSelectStyle.Hex("#16212E");

        // Header
        popup.levelLabel = Label(At(Rect("Level", dialog), 28f, 28f, 600f, 16f), "LEVEL 1", 13f, LevelSelectStyle.Gold, true, TextAlignmentOptions.MidlineLeft, 20f);
        popup.title = Label(At(Rect("Title", dialog), 28f, 48f, 700f, 44f), "Coastal Chaos", 40f, LevelSelectStyle.Text, false, TextAlignmentOptions.MidlineLeft, 2f, displayFont);
        RectTransform close = At(Rect("Close", dialog), 768f, 28f, 44f, 44f);
        Image closeFill = Box(close, card, LevelSelectStyle.Hex("#36485D"), 8f, 1f, true);
        Paint(At(Rect("Icon", close), 12f, 12f, 20f, 20f), sprites.close, LevelSelectStyle.Text);
        popup.closeButton = MakeButton(close.gameObject, closeFill);
        Paint(At(Rect("Divider", dialog), 28f, 108f, 784f, 1f), null, line);

        // Left column: preview and records
        RectTransform frame = At(Rect("Preview", dialog), 28f, 131f, 340f, 190f);
        Box(frame, LevelSelectStyle.Hex("#1E3223"), LevelSelectStyle.LockedBorder, 8f);
        popup.preview = Paint(Stretch(Rect("Picture", frame), 1f), null, Color.white);
        popup.noPreview = Label(Stretch(Rect("No Preview", frame)), "No preview picture yet", 13f, LevelSelectStyle.TextMuted, false, TextAlignmentOptions.Center).gameObject;
        RectTransform chip = At(Rect("Chip", frame), 10f, 10f, 76f, 20f);
        Paint(chip, sprites.rounded, WithAlpha(LevelSelectStyle.Background, 0.85f), 4f);
        Label(Stretch(Rect("Text", chip)), "PREVIEW", 11f, LevelSelectStyle.Hex("#C5D1DE"), true, TextAlignmentOptions.Center, 14f);

        RectTransform waveCard = At(Rect("Best Wave", dialog), 28f, 337f, 136f, 74f);
        Box(waveCard, card, line, 8f);
        Label(At(Rect("Label", waveCard), 14f, 12f, 116f, 15f), "BEST WAVE", 12f, LevelSelectStyle.TextMuted, true, TextAlignmentOptions.MidlineLeft, 14f);
        popup.bestWave = Label(At(Rect("Value", waveCard), 14f, 31f, 116f, 32f), "-", 28f, LevelSelectStyle.Text, true, TextAlignmentOptions.MidlineLeft);

        RectTransform clearCard = At(Rect("Best Clear", dialog), 176f, 337f, 192f, 74f);
        Box(clearCard, card, line, 8f);
        popup.bestClearLabel = Label(At(Rect("Label", clearCard), 14f, 12f, 172f, 15f), "BEST CLEAR · NONE YET", 12f, LevelSelectStyle.TextMuted, true, TextAlignmentOptions.MidlineLeft, 14f);
        popup.bestStars = StarRow(clearCard, 14f, 36f, 22f, 4f);

        // Right column: temperaments
        Label(At(Rect("Choose", dialog), 396f, 131f, 416f, 16f), "CHOOSE YOUR TEMPERAMENT", 13f, LevelSelectStyle.TextMuted, true, TextAlignmentOptions.MidlineLeft, 20f);
        Temperament[] order = { Temperament.Annoyance, Temperament.Anger, Temperament.Violence, Temperament.Bloodlust };
        popup.choices = new LevelDetailsPopup.TemperamentChoice[4];
        for (int i = 0; i < 4; i++)
        {
            float x = 396f + (i % 2) * 214f;
            float y = 161f + (i / 2) * 92f;
            Temperament temperament = order[i];
            Image highlight = Paint(At(Rect(temperament + " Highlight", dialog), x - 2f, y - 2f, 206f, 84f), sprites.rounded, LevelSelectStyle.TemperamentColor(temperament), 11f);
            RectTransform cell = At(Rect(temperament.ToString(), dialog), x, y, 202f, 80f);
            Image fill = Box(cell, card, LevelSelectStyle.LockedBorder, 10f, 1f, true, out Image border);
            CanvasGroup group = cell.gameObject.AddComponent<CanvasGroup>();   // faded while the temperament is locked
            Paint(At(Rect("Face", cell), 14f, 17f, 46f, 46f), sprites.faces[i], Color.white);
            Label(At(Rect("Name", cell), 72f, 18f, 124f, 22f), temperament.ToString(), 17f, LevelSelectStyle.Text, true, TextAlignmentOptions.MidlineLeft);
            TMP_Text difficulty = Label(At(Rect("Difficulty", cell), 72f, 42f, 124f, 18f), LevelSelectStyle.Difficulty(temperament), 13f, LevelSelectStyle.TextMuted, false, TextAlignmentOptions.MidlineLeft);
            // Outside the cell, so the fade doesn't dim it.
            Image lockIcon = Paint(At(Rect(temperament + " Lock", dialog), x + 174f, y + 12f, 16f, 16f), sprites.lockIcon, LevelSelectStyle.TextMuted);
            lockIcon.gameObject.SetActive(false);
            popup.choices[i] = new LevelDetailsPopup.TemperamentChoice
            {
                temperament = temperament,
                button = MakeButton(cell.gameObject, fill),
                border = border,
                highlight = highlight,
                group = group,
                difficulty = difficulty,
                lockIcon = lockIcon.gameObject,
            };
        }

        RectTransform earn = At(Rect("Earn", dialog), 396f, 347f, 416f, 42f);
        Box(earn, LevelSelectStyle.Hex("#0F1720"), line, 8f);
        popup.earnText = Label(At(Rect("Text", earn), 14f, 0f, 290f, 42f), "Clear on Violence to earn 3 stars", 14f, LevelSelectStyle.Hex("#C5D1DE"), false, TextAlignmentOptions.MidlineLeft);
        popup.earnStars = StarRow(earn, 313f, 11f, 20f, 3f);

        RectTransform start = At(Rect("Start", dialog), 396f, 403f, 416f, 60f);
        Image startFill = Paint(start, sprites.chamfer, LevelSelectStyle.Gold, 14f);
        startFill.raycastTarget = true;
        popup.startButton = MakeButton(start.gameObject, startFill);
        RectTransform content = Stretch(Rect("Content", start));
        HorizontalLayoutGroup row = content.gameObject.AddComponent<HorizontalLayoutGroup>();
        row.childAlignment = TextAnchor.MiddleCenter;
        row.spacing = 12f;
        row.childControlWidth = true;
        row.childControlHeight = false;
        row.childForceExpandWidth = false;
        row.childForceExpandHeight = false;
        RectTransform play = Rect("Icon", content);
        play.sizeDelta = new Vector2(20f, 20f);
        Paint(play, sprites.play, LevelSelectStyle.OnGold);
        LayoutElement playSize = play.gameObject.AddComponent<LayoutElement>();
        playSize.preferredWidth = 20f;
        playSize.preferredHeight = 20f;
        popup.startIcon = play.gameObject;
        RectTransform startText = Rect("Text", content);
        startText.sizeDelta = new Vector2(120f, 30f);
        popup.startLabel = Label(startText, "START", 22f, LevelSelectStyle.OnGold, true, TextAlignmentOptions.Center, 14f);

        root.gameObject.SetActive(false);
        return popup;
    }

    static Image[] StarRow(RectTransform parent, float x, float y, float size, float gap)
    {
        Image[] stars = new Image[4];
        for (int i = 0; i < 4; i++)
            stars[i] = Paint(At(Rect("Star " + (i + 1), parent), x + i * (size + gap), y, size, size), sprites.star, LevelSelectStyle.StarEmpty);
        return stars;
    }

    // ---------- UI helpers ----------

    static RectTransform Rect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    // Mockup-style placement: (x, y) is the top-left corner, measured down and right from the parent's top-left.
    static RectTransform At(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
        return rect;
    }

    // Pinned to the parent's top-left (or top-right) corner, `x` in from that side.
    static RectTransform Corner(RectTransform rect, bool right, float x, float y, float width, float height)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(right ? 1f : 0f, 1f);
        rect.pivot = new Vector2(right ? 1f : 0f, 1f);
        rect.anchoredPosition = new Vector2(right ? -x : x, -y);
        rect.sizeDelta = new Vector2(width, height);
        return rect;
    }

    // Centered on the parent's middle; y goes up.
    static RectTransform Center(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
        return rect;
    }

    static RectTransform Stretch(RectTransform rect, float inset = 0f)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
        return rect;
    }

    // radius > 0 makes a 9-sliced rounded (or chamfered) shape whose corners are `radius` UI units.
    static Image Paint(RectTransform rect, Sprite sprite, Color color, float radius = 0f)
    {
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        if (radius > 0f)
        {
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 16f / radius;   // the sliced sprites have 16px corners
        }
        return image;
    }

    // A rounded box with a border: the border color is the outer shape and the fill sits inside it.
    // Returns the fill (handy as a button's target).
    static Image Box(RectTransform rect, Color fill, Color border, float radius, float borderWidth = 1f, bool clickable = false)
    {
        return Box(rect, fill, border, radius, borderWidth, clickable, out _);
    }

    static Image Box(RectTransform rect, Color fill, Color border, float radius, float borderWidth, bool clickable, out Image edge)
    {
        edge = Paint(rect, sprites.rounded, border, radius);
        edge.raycastTarget = clickable;
        return Paint(Stretch(Rect("Fill", rect), borderWidth), sprites.rounded, fill, Mathf.Max(radius - borderWidth, 1f));
    }

    static TextMeshProUGUI Label(RectTransform rect, string text, float size, Color color, bool bold, TextAlignmentOptions align, float spacing = 0f, TMP_FontAsset font = null)
    {
        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = font != null ? font : bodyFont;
        label.fontSharedMaterial = TextMaterial(label.font);
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        label.alignment = align;
        label.characterSpacing = spacing;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Overflow;
        label.richText = true;
        label.raycastTarget = false;
        return label;
    }

    // TMP's default font material in this project was restyled (red face, outline, underlay) for other menus.
    // The level select gets its own plain copy, so its text colors come out as set and the other menus keep their look.
    static Material TextMaterial(TMP_FontAsset font)
    {
        if (textMaterials.TryGetValue(font, out Material cached) && cached != null)
            return cached;

        string path = MaterialsFolder + "LS_Text_" + font.name.Replace(" ", "") + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(font.material);
            AssetDatabase.CreateAsset(material, path);
        }
        else
        {
            material.shader = font.material.shader;
            material.CopyPropertiesFromMaterial(font.material);
        }
        material.SetColor("_FaceColor", Color.white);
        material.SetFloat("_FaceDilate", 0f);
        material.SetFloat("_OutlineWidth", 0f);
        material.SetColor("_UnderlayColor", Color.clear);
        material.DisableKeyword("OUTLINE_ON");
        material.DisableKeyword("UNDERLAY_ON");
        material.DisableKeyword("UNDERLAY_INNER");
        material.DisableKeyword("GLOW_ON");
        material.DisableKeyword("BEVEL_ON");
        EditorUtility.SetDirty(material);
        textMaterials[font] = material;
        return material;
    }

    static Button MakeButton(GameObject go, Graphic target)
    {
        Button button = go.AddComponent<Button>();
        button.targetGraphic = target;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.86f, 0.86f, 0.86f);
        colors.pressedColor = new Color(0.72f, 0.72f, 0.72f);
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(1f, 1f, 1f, 0.45f);
        button.colors = colors;
        return button;
    }

    static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    static void SetArray<T>(SerializedProperty property, T[] values) where T : Object
    {
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }
}
