#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace TwinHook.Editor
{
    /// <summary>
    /// Configura o kit CraftPix: importa os tiles limpos, cria Tile assets,
    /// uma palette e uma cena de exemplo jogÃ¡vel.
    /// Menu: Pindorama/CraftPix/Configurar tudo.
    /// </summary>
    public static class CraftPixSetupFinal
    {
        private const string ArtRoot = "Assets/_Project/Art/CraftPix";
        private const string TileRoot = ArtRoot + "/Generated/Tiles";
        private const string PalettePath = ArtRoot + "/Generated/CraftPixPalette.prefab";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/PlayerCraftPixDemo.prefab";
        private const string ScenePath = "Assets/_Project/Scenes/CraftPixDemo.unity";

        private static readonly string[] Themes =
            { "gray", "green", "blue", "purple", "tan", "red" };
        private static readonly string[] TileNames =
            { "tl", "t", "tr", "l", "c", "r", "bl", "b", "br", "single" };

        private static readonly Dictionary<string, Dictionary<string, Tile>> tiles =
            new Dictionary<string, Dictionary<string, Tile>>();

        [MenuItem("Pindorama/CraftPix/Configurar tudo (v2)")]
        public static void ConfigureEverything()
        {
            ConfigureTextureImporters();
            CreateTileAssets();
            CreatePalette();
            CreatePlayerPrefab();
            CreateDemoScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("CraftPix: configuraÃ§Ã£o concluÃ­da. Abra Assets/_Project/Scenes/CraftPixDemo.unity");
        }

        private static void ConfigureTextureImporters()
        {
            foreach (string theme in Themes)
                ConfigureTileset(ArtRoot + "/Generated/Tilesets/craftpix_" + theme + ".png", theme);

            string objects = ArtRoot + "/Objects";
            ConfigureObjectSheet(objects + "/Door.png", "Door", 64, 96, 6, 1);
            ConfigureObjectSheet(objects + "/Door2.png", "Door2", 64, 96, 6, 1);
            ConfigureObjectSheet(objects + "/Chest.png", "Chest", 64, 64, 8, 1);
            ConfigureObjectSheet(objects + "/Lever.png", "Lever", 64, 64, 3, 1);
            foreach (string f in new[]
            {
                "Button", "Button2", "Spikes1", "Spikes1_2", "Spikes2",
                "Spikes2_2", "Spikes3", "Spikes3_2", "Target", "TrainingDummy"
            })
            {
                string path = objects + "/" + f + ".png";
                if (File.Exists(path))
                    ConfigureSingle(path);
            }

            foreach (string f in Directory.GetFiles(ArtRoot + "/Pipes", "*.png"))
                ConfigureSingle(f.Replace('\\', '/'));
        }

        private static void ConfigureTileset(string path, string theme)
        {
            if (!File.Exists(path))
            {
                Debug.LogError("CraftPix: tileset nÃ£o encontrado: " + path);
                return;
            }

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return;

            ApplyBaseImporter(importer, true);
            importer.maxTextureSize = 256;
            List<SpriteMetaData> metas = new List<SpriteMetaData>();
            for (int i = 0; i < TileNames.Length; i++)
            {
                int col = i % 8;
                int row = i / 8;
                metas.Add(new SpriteMetaData
                {
                    name = theme + "_" + TileNames[i],
                    rect = new Rect(col * 32, 256 - (row + 1) * 32, 32, 32),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                });
            }
            ApplySpriteRects(importer, metas);
            importer.SaveAndReimport();
        }

        private static void ApplySpriteRects(TextureImporter importer,
                                             List<SpriteMetaData> metas)
        {
            SpriteDataProviderFactories factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider provider =
                factory.GetSpriteEditorDataProviderFromObject(importer);
            if (provider == null)
                return;

            provider.InitSpriteEditorDataProvider();
            SpriteRect[] rects = new SpriteRect[metas.Count];
            for (int i = 0; i < metas.Count; i++)
            {
                SpriteMetaData meta = metas[i];
                rects[i] = new SpriteRect
                {
                    name = meta.name,
                    rect = meta.rect,
                    alignment = (SpriteAlignment)meta.alignment,
                    pivot = meta.pivot,
                    border = meta.border,
                    spriteID = GUID.Generate()
                };
            }
            provider.SetSpriteRects(rects);
            provider.Apply();
        }

        private static void ConfigureObjectSheet(string path, string prefix, int cellW, int cellH,
                                                  int count, int rows)
        {
            if (!File.Exists(path))
                return;
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return;

            ApplyBaseImporter(importer, true);
            List<SpriteMetaData> metas = new List<SpriteMetaData>();
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < count; col++)
                {
                    metas.Add(new SpriteMetaData
                    {
                        name = prefix + "_" + (row * count + col),
                        rect = new Rect(col * cellW, row * cellH, cellW, cellH),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    });
                }
            }
            ApplySpriteRects(importer, metas);
            importer.SaveAndReimport();
        }

        private static void ConfigureSingle(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return;
            ApplyBaseImporter(importer, false);
            importer.SaveAndReimport();
        }

        private static void ApplyBaseImporter(TextureImporter importer, bool multiple)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = multiple
                ? SpriteImportMode.Multiple
                : SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 2048;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
        }

        private static void CreateTileAssets()
        {
            if (AssetDatabase.IsValidFolder(TileRoot))
                AssetDatabase.DeleteAsset(TileRoot);
            AssetDatabase.CreateFolder(ArtRoot + "/Generated", "Tiles");

            tiles.Clear();
            foreach (string theme in Themes)
            {
                string sheetPath = ArtRoot + "/Generated/Tilesets/craftpix_" + theme + ".png";
                Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath)
                    .OfType<Sprite>().ToArray();
                Dictionary<string, Sprite> byName = new Dictionary<string, Sprite>();
                foreach (Sprite s in sprites)
                    byName[s.name] = s;

                Dictionary<string, Tile> themeTiles = new Dictionary<string, Tile>();
                foreach (string n in TileNames)
                {
                    string spriteName = theme + "_" + n;
                    if (!byName.TryGetValue(spriteName, out Sprite sprite))
                    {
                        Debug.LogWarning("CraftPix: sprite ausente " + spriteName);
                        continue;
                    }

                    Tile tile = ScriptableObject.CreateInstance<Tile>();
                    tile.sprite = sprite;
                    tile.color = Color.white;
                    tile.colliderType = Tile.ColliderType.Grid;
                    string assetPath = TileRoot + "/tile_" + theme + "_" + n + ".asset";
                    AssetDatabase.CreateAsset(tile, assetPath);
                    themeTiles[n] = tile;
                }
                tiles[theme] = themeTiles;
            }
            AssetDatabase.SaveAssets();
        }

        private static void CreatePalette()
        {
            GameObject root = new GameObject("CraftPixPalette");
            Grid grid = root.AddComponent<Grid>();
            grid.cellSize = Vector3.one;

            GameObject layer = new GameObject("Layer");
            layer.transform.SetParent(root.transform);
            Tilemap tilemap = layer.AddComponent<Tilemap>();
            TilemapRenderer renderer = layer.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = 0;

            for (int themeIndex = 0; themeIndex < Themes.Length; themeIndex++)
            {
                string theme = Themes[themeIndex];
                for (int i = 0; i < TileNames.Length; i++)
                {
                    if (tiles.TryGetValue(theme, out Dictionary<string, Tile> dict) &&
                        dict.TryGetValue(TileNames[i], out Tile tile))
                    {
                        tilemap.SetTile(new Vector3Int(themeIndex, i, 0), tile);
                    }
                }
            }

            Directory.CreateDirectory(Path.GetDirectoryName(PalettePath));
            PrefabUtility.SaveAsPrefabAsset(root, PalettePath);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.ImportAsset(PalettePath);

            // O Tile Palette procura um subasset GridPalette dentro do prefab.
            GridPalette gridPalette = ScriptableObject.CreateInstance<GridPalette>();
            gridPalette.cellSizing = GridPalette.CellSizing.Automatic;
            gridPalette.hideFlags = HideFlags.None;
            AssetDatabase.AddObjectToAsset(gridPalette, PalettePath);
            AssetDatabase.ImportAsset(PalettePath);
            AssetDatabase.SaveAssets();
            Debug.Log("CraftPix palette subassets: " +
                AssetDatabase.LoadAllAssetsAtPath(PalettePath).Length);
        }

        private static void CreatePlayerPrefab()
        {
            Directory.CreateDirectory("Assets/_Project/Prefabs");
            Scene level = EditorSceneManager.OpenScene(
                "Assets/_Project/Scenes/Level02.unity", OpenSceneMode.Additive);
            PlayerController2D player = UnityEngine.Object.FindAnyObjectByType<PlayerController2D>();
            if (player == null)
            {
                Debug.LogError("CraftPix: player da Level02 nÃ£o encontrado.");
                EditorSceneManager.CloseScene(level, true);
                return;
            }

            PrefabUtility.SaveAsPrefabAsset(player.gameObject, PlayerPrefabPath);
            EditorSceneManager.CloseScene(level, true);
            AssetDatabase.SaveAssets();
        }

        private static void CreateDemoScene()
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 9.5f;
            cam.backgroundColor = new Color(0.035f, 0.045f, 0.09f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();
            CameraFollow follow = camGo.AddComponent<CameraFollow>();

            // Grid and tilemaps
            GameObject gridGo = new GameObject("Grid");
            Grid grid = gridGo.AddComponent<Grid>();
            grid.cellSize = Vector3.one;

            Tilemap background = CreateTilemap(gridGo.transform, "Background", -20, null, false);
            Tilemap common = CreateTilemap(gridGo.transform, "Common", 0, "Default", true);
            Tilemap climbable = CreateTilemap(gridGo.transform, "Climbable", 1, "ClimbableWall", true);
            Tilemap hazardVisual = CreateTilemap(gridGo.transform, "Hazard", 2, null, false);

            background.color = new Color(0.24f, 0.17f, 0.34f, 1f);
            common.color = new Color(0.82f, 0.84f, 0.9f, 1f);
            climbable.color = new Color(0.78f, 1f, 0.88f, 1f);
            hazardVisual.color = new Color(1f, 0.7f, 0.65f, 1f);

            Fill(background, -16, -14, 16, 8, "purple", "c");

            // Ground and side walls.
            Fill(common, -14, -5, 14, -5, "gray", "c");
            Fill(common, -14, -4, 14, -4, "gray", "t");
            Fill(common, -14, -3, -14, 4, "gray", "l");
            Fill(common, 14, -3, 14, 4, "gray", "r");
            Set(common, -14, 5, "gray", "tl");
            Set(common, 14, 5, "gray", "tr");

            // Platforms.
            Platform(common, -10, 1, 6, "gray");
            Platform(common, -1, 2, 6, "gray");
            Platform(common, -7, 5, 4, "gray");

            // Climbable green wall.
            Fill(climbable, 7, -3, 7, 3, "green", "c");
            Set(climbable, 7, 4, "green", "t");
            Set(climbable, 7, -4, "green", "b");

            // Red visual strip under spikes.
            Fill(hazardVisual, 1, -3, 3, -3, "red", "t");

            // Climbable wall hook anchor.
            int climbLayer = LayerMask.NameToLayer("ClimbableWall");
            if (climbLayer >= 0)
                climbable.gameObject.layer = climbLayer;
            HookAnchor anchor = climbable.gameObject.AddComponent<HookAnchor>();
            anchor.Configure(HookColor.Neutral, HookAnchorMotion.Swing);
            Transform anchorPoint = new GameObject("AnchorPoint").transform;
            anchorPoint.SetParent(climbable.transform);
            anchorPoint.position = new Vector3(4.5f, 0.5f, 0f);
            SerializedObject anchorSo = new SerializedObject(anchor);
            SerializedProperty attachProp = anchorSo.FindProperty("attachPoint");
            if (attachProp != null)
            {
                attachProp.objectReferenceValue = anchorPoint;
                anchorSo.ApplyModifiedPropertiesWithoutUndo();
            }

            // Player.
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            if (playerPrefab == null)
            {
                Debug.LogError("CraftPix: player prefab nÃ£o existe. Rode a configuraÃ§Ã£o de novo.");
                return;
            }
            GameObject playerGo = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
            playerGo.name = "Player";
            playerGo.transform.position = new Vector3(-10f, -2.5f, 0f);
            PlayerController2D player = playerGo.GetComponent<PlayerController2D>();
            DualHookController hooks = playerGo.GetComponent<DualHookController>();
            follow.Configure(playerGo.transform);
            SerializedObject followSo = new SerializedObject(follow);
            SerializedProperty sizeProp = followSo.FindProperty("orthographicSize");
            if (sizeProp != null)
            {
                sizeProp.floatValue = 9.5f;
                followSo.ApplyModifiedPropertiesWithoutUndo();
            }

            // Level / respawn.
            GameObject spawnGo = new GameObject("Spawn");
            spawnGo.transform.position = new Vector3(-10f, -2.5f, 0f);
            GameObject levelGo = new GameObject("Level");
            LevelController level = levelGo.AddComponent<LevelController>();
            level.Configure(spawnGo.transform, player, hooks, null);

            // HUD.
            GameObject hudGo = new GameObject("HUD");
            HookHud hud = hudGo.AddComponent<HookHud>();
            hud.Configure(player, hooks, hooks != null ? hooks.RedHand : null,
                hooks != null ? hooks.BlueHand : null);

            // Props.
            CreateProp("Door", ArtRoot + "/Objects/Door.png", "Door_0",
                new Vector3(-8f, -2.5f, 0f), 3, null);
            CreateProp("Button", ArtRoot + "/Objects/Button.png", null,
                new Vector3(2f, -3.5f, 0f), 3, null);
            CreateProp("Chest", ArtRoot + "/Objects/Chest.png", "Chest_0",
                new Vector3(-2f, -3.5f, 0f), 3, null);
            CreateProp("Target", ArtRoot + "/Objects/Target.png", null,
                new Vector3(-4f, 1.2f, 0f), 3, null);
            CreateProp("TrainingDummy", ArtRoot + "/Objects/TrainingDummy.png", null,
                new Vector3(-6f, -2.6f, 0f), 3, null);
            CreateProp("Pipe01", ArtRoot + "/Pipes/Pipe_01.png", null,
                new Vector3(-12f, -3.5f, 0f), -10, null);
            CreateProp("Pipe05", ArtRoot + "/Pipes/Pipe_05.png", null,
                new Vector3(9f, -3.5f, 0f), -10, null);

            // Spikes.
            for (int x = 1; x <= 3; x++)
                CreateSpike(x, level);

            // Kill zone.
            GameObject killGo = new GameObject("KillZone");
            killGo.transform.position = new Vector3(0f, -8f, 0f);
            BoxCollider2D killCol = killGo.AddComponent<BoxCollider2D>();
            killCol.isTrigger = true;
            killCol.size = new Vector2(40f, 2f);
            LethalHazard kill = killGo.AddComponent<LethalHazard>();
            kill.Configure(level);
            SetPrivateEnum(kill, "damageType", (int)DamageType.Lethal);
            SetPrivateFloat(kill, "hitCooldown", 0.5f);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuild(ScenePath);
            Debug.Log("CraftPix: cena de exemplo criada em " + ScenePath);
        }

        private static Tilemap CreateTilemap(Transform parent, string name, int sorting,
                                             string layerName, bool collider)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            if (!string.IsNullOrEmpty(layerName))
            {
                int layer = LayerMask.NameToLayer(layerName);
                if (layer >= 0)
                    go.layer = layer;
            }

            Tilemap map = go.AddComponent<Tilemap>();
            TilemapRenderer renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = sorting;

            if (collider)
            {
                // Collider por tile: mais simples e confiÃ¡vel para o protÃ³tipo.
                // CompositeCollider2D fica para quando o mapa estiver fechado.
                go.AddComponent<TilemapCollider2D>();
            }
            return map;
        }

        private static void Fill(Tilemap map, int x0, int y0, int x1, int y1,
                                 string theme, string tileName)
        {
            for (int x = x0; x <= x1; x++)
                for (int y = y0; y <= y1; y++)
                    Set(map, x, y, theme, tileName);
        }

        private static void Set(Tilemap map, int x, int y, string theme, string tileName)
        {
            if (tiles.TryGetValue(theme, out Dictionary<string, Tile> dict) &&
                dict.TryGetValue(tileName, out Tile tile))
            {
                map.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        private static void Platform(Tilemap map, int x0, int y, int width, string theme)
        {
            for (int i = 0; i < width; i++)
            {
                string n = i == 0 ? "tl" : i == width - 1 ? "tr" : "t";
                Set(map, x0 + i, y, theme, n);
                string below = i == 0 ? "l" : i == width - 1 ? "r" : "c";
                Set(map, x0 + i, y - 1, theme, below);
            }
        }

        private static GameObject CreateProp(string name, string texturePath, string spriteName,
                                             Vector3 pos, int sorting, Transform parent)
        {
            if (!File.Exists(texturePath))
                return null;
            Sprite sprite = null;
            if (string.IsNullOrEmpty(spriteName))
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
            else
                sprite = AssetDatabase.LoadAllAssetsAtPath(texturePath)
                    .OfType<Sprite>().FirstOrDefault(s => s.name == spriteName);
            if (sprite == null)
            {
                Debug.LogWarning("CraftPix: sprite nÃ£o encontrado " + texturePath + " / " + spriteName);
                return null;
            }

            GameObject go = new GameObject(name);
            go.transform.position = pos;
            if (parent != null)
                go.transform.SetParent(parent);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sorting;
            return go;
        }

        private static void CreateSpike(int x, LevelController level)
        {
            GameObject spike = CreateProp("Spike_" + x, ArtRoot + "/Objects/Spikes1.png",
                null, new Vector3(x, -3.5f, 0f), 3, null);
            if (spike == null)
                return;
            BoxCollider2D col = spike.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.9f, 0.35f);
            col.offset = new Vector2(0f, -0.3f);
            SpriteRenderer spikeRenderer = spike.GetComponent<SpriteRenderer>();
            if (spikeRenderer != null)
                spikeRenderer.color = new Color(1f, 0.42f, 0.38f, 1f);
            LethalHazard hazard = spike.AddComponent<LethalHazard>();
            hazard.Configure(level);
            SetPrivateEnum(hazard, "damageType", (int)DamageType.NonLethal);
        }

        private static void SetPrivateEnum(UnityEngine.Object target, string field, int value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p != null)
            {
                p.enumValueIndex = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetPrivateFloat(UnityEngine.Object target, string field, float value)
        {
            SerializedObject so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(field);
            if (p != null)
            {
                p.floatValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void AddSceneToBuild(string path)
        {
            List<EditorBuildSettingsScene> scenes =
                EditorBuildSettings.scenes.ToList();
            if (scenes.All(s => s.path != path))
            {
                scenes.Add(new EditorBuildSettingsScene(path, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
        }
    }
}
#endif

