// Assets/Editor/PrototypeBuilder.cs
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class PrototypeBuilder
{
    private const string whiteTexPath = "Assets/Generated/white.png";
    private const string scenePath = "Assets/Scenes/MellstroyArena.unity";
    private const string prefabFolder = "Assets/Prefabs";

    [MenuItem("Tools/Build Mellstroy Arena Scene (Generate)")]
    public static void BuildScene()
    {
        // Create folders
        if (!AssetDatabase.IsValidFolder("Assets/Generated")) AssetDatabase.CreateFolder("Assets", "Generated");
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        if (!AssetDatabase.IsValidFolder(prefabFolder)) AssetDatabase.CreateFolder("Assets", "Prefabs");

        // Create white texture sprite if missing
        if (!File.Exists(whiteTexPath))
        {
            Texture2D tex = new Texture2D(8, 8);
            Color32[] cols = new Color32[8 * 8];
            for (int i = 0; i < cols.Length; i++) cols[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(cols);
            tex.Apply();
            byte[] png = tex.EncodeToPNG();
            File.WriteAllBytes(whiteTexPath, png);
            AssetDatabase.ImportAsset(whiteTexPath);
            var ti = AssetImporter.GetAtPath(whiteTexPath) as TextureImporter;
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.SaveAndReimport();
            }
        }

        // Ensure tags/layers exist
        AddTag("Boxer");
        AddTag("LaserGuy");
        AddTag("Wall");
        AddLayer("Characters");
        AddLayer("Wall");

        // Create new scene
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        GameObject camGO = new GameObject("Main Camera");
        Camera cam = camGO.AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.27f, 0.78f, 0.92f, 1f); // light blue

        // Create Canvas for UI
        GameObject canvasGO = new GameObject("UI_Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Title text
        GameObject title = new GameObject("TitleText");
        title.transform.SetParent(canvasGO.transform, false);
        Text titleText = title.AddComponent<Text>();
        // Use LegacyRuntime.ttf for compatibility with recent Unity versions
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.text = "Boxing Guy vs Exploding Guy";
        titleText.alignment = TextAnchor.UpperCenter;
        titleText.fontSize = 36;
        RectTransform tr = title.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0.5f, 1f);
        tr.anchorMax = new Vector2(0.5f, 1f);
        tr.pivot = new Vector2(0.5f, 1f);
        tr.anchoredPosition = new Vector2(0, -20);
        tr.sizeDelta = new Vector2(800, 60);

        // Bottom Text
        GameObject bottom = new GameObject("BottomText");
        bottom.transform.SetParent(canvasGO.transform, false);
        Text bottomText = bottom.AddComponent<Text>();
        bottomText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bottomText.text = "Like and Subscribe!";
        bottomText.alignment = TextAnchor.LowerCenter;
        bottomText.fontSize = 32;
        RectTransform br = bottom.GetComponent<RectTransform>();
        br.anchorMin = new Vector2(0.5f, 0f);
        br.anchorMax = new Vector2(0.5f, 0f);
        br.pivot = new Vector2(0.5f, 0f);
        br.anchoredPosition = new Vector2(0, 20);
        br.sizeDelta = new Vector2(800, 60);

        // Background (SpriteRenderer) full-screen
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(whiteTexPath);
        if (sprite == null)
        {
            // Try to create a Sprite asset from the PNG if loading as Sprite failed
            Texture2D tex2 = AssetDatabase.LoadAssetAtPath<Texture2D>(whiteTexPath);
            if (tex2 != null)
            {
                Sprite newSprite = Sprite.Create(tex2, new Rect(0, 0, tex2.width, tex2.height), new Vector2(0.5f, 0.5f), 100f);
                string spriteAssetPath = "Assets/Generated/whiteSprite.asset";
                AssetDatabase.CreateAsset(newSprite, spriteAssetPath);
                AssetDatabase.SaveAssets();
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spriteAssetPath);
            }
            else
            {
                Debug.LogWarning("Could not load texture at " + whiteTexPath + " — background sprite will be missing.");
            }
        }

        GameObject bg = new GameObject("Background");
        var sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(0.27f, 0.78f, 0.92f, 1f);
        bg.transform.position = Vector3.zero;
        // scale to camera — guard against null sprite
        if (sr.sprite != null)
        {
            float worldHeight = cam.orthographicSize * 2f;
            float worldWidth = worldHeight * cam.aspect;
            Vector2 spriteSize = sr.sprite.bounds.size;
            if (spriteSize.x > 0 && spriteSize.y > 0)
                bg.transform.localScale = new Vector3(worldWidth / spriteSize.x, worldHeight / spriteSize.y, 1f);
        }

        // Create walls
        CreateWall("Wall_Top", new Vector2(0, cam.orthographicSize - 0.5f), new Vector2(cam.orthographicSize * 2f * cam.aspect, 1f), sr.sprite);
        CreateWall("Wall_Bottom", new Vector2(0, -cam.orthographicSize + 0.5f), new Vector2(cam.orthographicSize * 2f * cam.aspect, 1f), sr.sprite);
        CreateWall("Wall_Left", new Vector2(-cam.orthographicSize * cam.aspect + 0.5f, 0), new Vector2(1f, cam.orthographicSize * 2f), sr.sprite);
        CreateWall("Wall_Right", new Vector2(cam.orthographicSize * cam.aspect - 0.5f, 0), new Vector2(1f, cam.orthographicSize * 2f), sr.sprite);

        // Create Laser prefab
        GameObject laserPrefab = new GameObject("LaserPrefab");
        var lr = laserPrefab.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = Color.red;
        lr.startWidth = lr.endWidth = 0.1f;
        laserPrefab.AddComponent(typeof(LaserBeam2D));

        string laserPrefabPath = prefabFolder + "/LaserPrefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(laserPrefab, laserPrefabPath);
        Object.DestroyImmediate(laserPrefab);

        // Create LaserGay prefab
        GameObject lg = new GameObject("LaserGay");
        var srLG = lg.AddComponent<SpriteRenderer>();
        srLG.sprite = sprite;
        srLG.color = new Color(1f, 0.5f, 0.7f);
        lg.AddComponent<Rigidbody2D>().gravityScale = 0f;
        lg.AddComponent<CircleCollider2D>().radius = 0.5f;
        lg.AddComponent(typeof(LaserGayFighter));
        // create firepoint
        GameObject fp = new GameObject("FirePoint");
        fp.transform.SetParent(lg.transform, false);
        fp.transform.localPosition = new Vector3(0.5f, 0f, 0f);
        lg.tag = "LaserGuy";
        int chLayer = LayerMask.NameToLayer("Characters");
        if (chLayer != -1) lg.layer = chLayer;

        string lgPath = prefabFolder + "/LaserGay.prefab";
        PrefabUtility.SaveAsPrefabAsset(lg, lgPath);
        Object.DestroyImmediate(lg);

        // Create Boxer prefab
        GameObject bx = new GameObject("Boxer");
        var srBX = bx.AddComponent<SpriteRenderer>();
        srBX.sprite = sprite;
        srBX.color = new Color(1f, 0.8f, 0.6f);
        bx.AddComponent<Rigidbody2D>().gravityScale = 0f;
        bx.AddComponent<CircleCollider2D>().radius = 0.5f;
        bx.AddComponent(typeof(BoxerFighter));
        bx.tag = "Boxer";
        if (chLayer != -1) bx.layer = chLayer;

        string bxPath = prefabFolder + "/Boxer.prefab";
        PrefabUtility.SaveAsPrefabAsset(bx, bxPath);
        Object.DestroyImmediate(bx);

        // Create HealthUI prefab (simple)
        GameObject healthUI = new GameObject("HealthUI");
        var rect = healthUI.AddComponent<RectTransform>();
        healthUI.AddComponent<CanvasRenderer>();
        var image = healthUI.AddComponent<Image>();
        image.color = Color.white;
        rect.sizeDelta = new Vector2(60, 20);
        GameObject txtGO = new GameObject("HPText");
        txtGO.transform.SetParent(healthUI.transform, false);
        Text hpText = txtGO.AddComponent<Text>();
        hpText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hpText.text = "100";
        hpText.alignment = TextAnchor.MiddleCenter;
        RectTransform trect = txtGO.GetComponent<RectTransform>();
        trect.anchorMin = Vector2.zero; trect.anchorMax = Vector2.one;
        trect.offsetMin = Vector2.zero; trect.offsetMax = Vector2.zero;

        healthUI.AddComponent(typeof(FloatingHealthUI));

        string healthPath = prefabFolder + "/HealthUI.prefab";
        PrefabUtility.SaveAsPrefabAsset(healthUI, healthPath);
        Object.DestroyImmediate(healthUI);

        // Instantiate two characters in scene
        GameObject lgInstance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(lgPath));
        lgInstance.transform.position = new Vector3(-2f, 0f, 0f);
        GameObject bxInstance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(bxPath));
        bxInstance.transform.position = new Vector3(2f, 0f, 0f);

        // Attach HealthUI instances
        GameObject h1 = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(healthPath));
        h1.transform.SetParent(canvasGO.transform, false);
        var fh1 = h1.GetComponent<FloatingHealthUI>();
        fh1.target = lgInstance.transform;

        GameObject h2 = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(healthPath));
        h2.transform.SetParent(canvasGO.transform, false);
        var fh2 = h2.GetComponent<FloatingHealthUI>();
        fh2.target = bxInstance.transform;

        // Assign manager
        GameObject gm = new GameObject("FightingGameManager");
        var mgr = gm.AddComponent<FightingGameManager>();
        mgr.fighter1 = lgInstance.GetComponent<Fighter>();
        mgr.fighter2 = bxInstance.GetComponent<Fighter>();
        mgr.hpText1 = h1.GetComponentInChildren<Text>();
        mgr.hpText2 = h2.GetComponentInChildren<Text>();

        // Save the scene
        EditorSceneManager.SaveScene(scene, scenePath);

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Mellstroy Arena", "Scene and prefabs generated in Assets/Scenes and Assets/Prefabs. Open Scenes/MellstroyArena to run.", "OK");
    }

    static void CreateWall(string name, Vector2 pos, Vector2 size, Sprite sprite)
    {
        GameObject go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = Color.black;
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        if (sprite != null)
            go.transform.localScale = new Vector3(size.x / sprite.bounds.size.x, size.y / sprite.bounds.size.y, 1f);
        else
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var bc = go.AddComponent<BoxCollider2D>();
        bc.size = Vector2.one;
        go.tag = "Wall";
        int layer = LayerMask.NameToLayer("Wall");
        if (layer != -1) go.layer = layer;
    }

    static void AddTag(string tag)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tagsProp = tagManager.FindProperty("tags");
        bool exists = false;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            var t = tagsProp.GetArrayElementAtIndex(i).stringValue;
            if (t == tag) { exists = true; break; }
        }
        if (!exists)
        {
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            tagManager.ApplyModifiedProperties();
            Debug.Log("Added tag: " + tag);
        }
    }

    static void AddLayer(string layerName)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layersProp = tagManager.FindProperty("layers");
        bool exists = false;
        for (int i = 0; i < layersProp.arraySize; i++)
        {
            var prop = layersProp.GetArrayElementAtIndex(i);
            if (prop != null && prop.stringValue == layerName) { exists = true; break; }
        }
        if (exists) return;
        for (int i = 8; i < layersProp.arraySize; i++)
        {
            var prop = layersProp.GetArrayElementAtIndex(i);
            if (prop != null && string.IsNullOrEmpty(prop.stringValue))
            {
                prop.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                Debug.Log("Added layer: " + layerName);
                return;
            }
        }
        Debug.LogWarning("No free layer slots to add layer: " + layerName);
    }
}
