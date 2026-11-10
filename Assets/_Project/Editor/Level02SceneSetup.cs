#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TwinHook;

namespace TwinHook.Editor
{
    /// <summary>
    /// Cria a cena Level02 com todos os objetos visíveis/editáveis na hierarquia.
    /// Não usa GameBootstrap: plataformas, caixas, laser, âncoras e bola amarela
    /// são objetos reais da cena.
    /// </summary>
    public static class Level02SceneSetup
    {
        private static Sprite whiteSprite;
        private static Sprite circleSprite;
        private static Sprite hatchSprite;
        private static Sprite spikeSprite;
        private static HookSettings settings;

        [MenuItem("Pindorama/Criar Level 02")]
        public static void CreateScene()
        {
            EnsureFolder("Assets/_Project", "Art");
            EnsureFolder("Assets/_Project", "Data");
            EnsureFolder("Assets/_Project", "Scenes");

            whiteSprite = LoadOrCreateSprite("WhiteSquare", 1, 1, 1f, (x, y) => Color.white);
            circleSprite = LoadOrCreateSprite("Circle", 64, 64, 64f, CirclePixel);
            hatchSprite = LoadOrCreateSprite("Hatch", 32, 32, 32f, HatchPixel);
            spikeSprite = LoadOrCreateSprite("Spike", 16, 16, 16f, SpikePixel);
            settings = LoadOrCreateSettings();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Transform root = new GameObject("Level02").transform;

            // ---------- Jogador ----------
            Vector2 spawn = new Vector2(-20f, -3.4f);
            HookHand redHand;
            HookHand blueHand;
            DualHookController controller;
            GameObject playerGO = CreatePlayer(root, spawn, out redHand, out blueHand, out controller);
            PlayerController2D player = playerGO.GetComponent<PlayerController2D>();

            // ---------- Câmera ----------
            CreateCamera(root, player);

            // ---------- Controlador da fase ----------
            LevelController level = new GameObject("LevelController").AddComponent<LevelController>();
            level.transform.SetParent(root, false);

            // ---------- Estrutura ----------
            CreatePlatform(root, "ParedeEsquerda", new Vector2(-24.5f, 2f), new Vector2(1f, 24f), new Color(0.18f, 0.20f, 0.26f));
            CreatePlatform(root, "ParedeDireita", new Vector2(24.5f, 2f), new Vector2(1f, 24f), new Color(0.18f, 0.20f, 0.26f));

            CreatePlatform(root, "PlataformaSpawn", new Vector2(-20f, -5f), new Vector2(7f, 1f), new Color(0.30f, 0.33f, 0.40f));
            CreatePlatform(root, "Plataforma01", new Vector2(-13f, -6f), new Vector2(4f, 1f), new Color(0.30f, 0.33f, 0.40f));
            CreatePlatform(root, "Plataforma02", new Vector2(-5f, -4f), new Vector2(6f, 1f), new Color(0.30f, 0.33f, 0.40f));
            CreatePlatform(root, "Plataforma03", new Vector2(2.5f, -4f), new Vector2(5f, 1f), new Color(0.30f, 0.33f, 0.40f));
            CreatePlatform(root, "Plataforma04", new Vector2(9f, -4f), new Vector2(5f, 1f), new Color(0.30f, 0.33f, 0.40f));
            CreatePlatform(root, "Plataforma05", new Vector2(16f, -6f), new Vector2(4f, 1f), new Color(0.30f, 0.33f, 0.40f));
            CreatePlatform(root, "Plataforma06", new Vector2(20f, -5f), new Vector2(5f, 1f), new Color(0.30f, 0.33f, 0.40f));

            CreatePlatform(root, "PlataformaAltaEsquerda", new Vector2(-21f, 3.8f), new Vector2(7f, 1.2f), new Color(0.28f, 0.30f, 0.36f));
            CreatePlatform(root, "PlataformaAltaDireita", new Vector2(19f, 2.5f), new Vector2(7f, 1.2f), new Color(0.28f, 0.30f, 0.36f));

            // ---------- Corredor de âncoras ----------
            CreateAnchor(root, "Ancora_R_01", new Vector2(-20f, 6.5f), HookColor.Red);
            CreateAnchor(root, "Ancora_B_02", new Vector2(-16f, 6.5f), HookColor.Blue);
            CreateAnchor(root, "Ancora_N_03", new Vector2(-12f, 6.5f), HookColor.Neutral);
            CreateAnchor(root, "Ancora_R_04", new Vector2(-8f, 6.5f), HookColor.Red);
            CreateAnchor(root, "Ancora_B_05", new Vector2(-4f, 6.5f), HookColor.Blue);
            CreateAnchor(root, "Ancora_N_06", new Vector2(0f, 6.5f), HookColor.Neutral);
            CreateAnchor(root, "Ancora_R_07", new Vector2(4f, 6.5f), HookColor.Red);
            CreateAnchor(root, "Ancora_B_08", new Vector2(8f, 6.5f), HookColor.Blue);
            CreateAnchor(root, "Ancora_N_09", new Vector2(12f, 6.5f), HookColor.Neutral);
            CreateAnchor(root, "Ancora_R_10", new Vector2(16f, 6.5f), HookColor.Red);
            CreateAnchor(root, "Ancora_B_11", new Vector2(20f, 6.5f), HookColor.Blue);

            // ---------- Caixas / pits ----------
            BoxIdentity redBox = CreateBox(root, "BOX RED 2", new Vector2(-6f, -2.7f), HookColor.Red, new Color(0.95f, 0.55f, 0.60f));
            BoxIdentity blueBox = CreateBox(root, "BOX BLUE 1", new Vector2(9f, -2.7f), HookColor.Blue, new Color(0.45f, 0.68f, 1f));

            BoxSnapTarget redPit = CreatePit(root, "Pit Red", new Vector2(-3.3f, -3.1f), HookColor.Red);
            BoxSnapTarget bluePit = CreatePit(root, "Pit Blue", new Vector2(4.2f, -3.1f), HookColor.Blue);

            // ---------- Laser ----------
            LaserHazard laser = CreateLaser(root, level, new Vector2(15f, 1.5f), 12f);

            // ---------- Puzzle ----------
            BoxSyncPuzzle puzzle = new GameObject("BoxSyncPuzzle").AddComponent<BoxSyncPuzzle>();
            puzzle.transform.SetParent(root, false);
            puzzle.Configure(new[] { redPit, bluePit }, new[] { redBox, blueBox }, new[] { laser }, 3f);

            // ---------- Spawn / Level ----------
            GameObject spawnGO = new GameObject("SpawnPoint");
            spawnGO.transform.SetParent(root, false);
            spawnGO.transform.position = new Vector3(spawn.x, spawn.y, 0f);
            level.Configure(spawnGO.transform, player, controller, puzzle);

            // ---------- Bola amarela móvel ----------
            CreateMovingBall(root, new Vector2(22f, 6f), 1.5f, 8.5f);

            // ---------- Espinhos ----------
            CreateSpikeStrip(root, level);

            // ---------- HUD / Mira / Speed lines ----------
            CreateUi(root, player, controller, redHand, blueHand);

            // ---------- Build settings ----------
            string scenePath = "Assets/_Project/Scenes/Level02.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(scenePath, true),
                new EditorBuildSettingsScene("Assets/_Project/Scenes/Main.unity", true)
            };

            AssetDatabase.SaveAssets();
            Debug.Log("PINDORAMA: cena Level02.unity criada com elementos visíveis na hierarquia.");
        }

        private static GameObject CreatePlayer(Transform root, Vector2 spawn,
                                               out HookHand redHand, out HookHand blueHand,
                                               out DualHookController controller)
        {
            GameObject playerGO = new GameObject("Player");
            playerGO.transform.SetParent(root, false);
            playerGO.transform.position = new Vector3(spawn.x, spawn.y, 0f);

            Rigidbody2D rb = playerGO.AddComponent<Rigidbody2D>();
            rb.gravityScale = 4.5f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CapsuleCollider2D col = playerGO.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.8f, 1.8f);
            col.direction = CapsuleDirection2D.Vertical;

            PlayerController2D player = playerGO.AddComponent<PlayerController2D>();
            playerGO.AddComponent<PlayerTrail>();

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(playerGO.transform, false);
            SpriteRenderer vsr = visual.AddComponent<SpriteRenderer>();
            vsr.sprite = whiteSprite;
            vsr.color = new Color(0.78f, 0.90f, 1f);
            vsr.sortingOrder = 4;
            visual.transform.localScale = new Vector3(0.8f, 1.8f, 1f);

            PlayerBodyAlignment alignment = playerGO.AddComponent<PlayerBodyAlignment>();
            alignment.Configure(vsr, col);

            GameObject redMuzzle = new GameObject("HandRed");
            redMuzzle.transform.SetParent(playerGO.transform, false);
            redMuzzle.transform.localPosition = new Vector3(-0.35f, 0.1f, 0f);

            GameObject blueMuzzle = new GameObject("HandBlue");
            blueMuzzle.transform.SetParent(playerGO.transform, false);
            blueMuzzle.transform.localPosition = new Vector3(0.35f, 0.1f, 0f);

            redHand = redMuzzle.AddComponent<HookHand>();
            HookRopeView redRope = redMuzzle.AddComponent<HookRopeView>();
            redHand.Configure(HookColor.Red, redMuzzle.transform, settings, player, redRope);
            redRope.Configure(redHand, redMuzzle.transform, new Color(0.95f, 0.28f, 0.36f));

            blueHand = blueMuzzle.AddComponent<HookHand>();
            HookRopeView blueRope = blueMuzzle.AddComponent<HookRopeView>();
            blueHand.Configure(HookColor.Blue, blueMuzzle.transform, settings, player, blueRope);
            blueRope.Configure(blueHand, blueMuzzle.transform, new Color(0.24f, 0.60f, 0.90f));

            controller = playerGO.AddComponent<DualHookController>();
            controller.Configure(player, settings, redHand, blueHand);

            return playerGO;
        }

        private static void CreateCamera(Transform root, PlayerController2D player)
        {
            GameObject camGO = new GameObject("Main Camera");
            camGO.transform.SetParent(root, false);
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(-18f, 0f, -10f);

            Camera cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
            cam.orthographic = true;
            cam.orthographicSize = 10.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.06f, 0.09f);

            CameraFollow follow = camGO.AddComponent<CameraFollow>();
            follow.Configure(player.transform);
        }

        private static void CreateUi(Transform root, PlayerController2D player,
                                     DualHookController controller, HookHand red, HookHand blue)
        {
            GameObject hudGO = new GameObject("HUD");
            hudGO.transform.SetParent(root, false);
            HookHud hud = hudGO.AddComponent<HookHud>();
            hud.Configure(player, controller, red, blue);

            GameObject aimGO = new GameObject("AimView");
            aimGO.transform.SetParent(root, false);
            HookAimView aim = aimGO.AddComponent<HookAimView>();
            aim.Configure(controller, player, settings);

            GameObject speedGO = new GameObject("SpeedLines");
            speedGO.transform.SetParent(root, false);
            SpeedLinesOverlay speed = speedGO.AddComponent<SpeedLinesOverlay>();
            speed.Configure(player);
        }

        private static GameObject CreatePlatform(Transform root, string name, Vector2 center,
                                                 Vector2 size, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(center.x, center.y, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = whiteSprite;
            sr.color = color;
            sr.sortingOrder = 0;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            return go;
        }

        private static void CreateAnchor(Transform root, string name, Vector2 position, HookColor color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(position.x, position.y, 0f);
            go.transform.localScale = new Vector3(2.5f, 0.4f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = whiteSprite;
            sr.color = ColorFor(color);
            sr.sortingOrder = 2;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            col.isTrigger = true;

            HookAnchor anchor = go.AddComponent<HookAnchor>();
            anchor.Configure(color, HookAnchorMotion.Swing);
        }

        private static BoxIdentity CreateBox(Transform root, string name, Vector2 position,
                                             HookColor color, Color tint)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(position.x, position.y, 0f);
            go.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = whiteSprite;
            sr.color = tint;
            sr.sortingOrder = 3;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            HookAnchor anchor = go.AddComponent<HookAnchor>();
            anchor.Configure(color, HookAnchorMotion.Fixed);

            HookPullable pull = go.AddComponent<HookPullable>();
            pull.ConfigurePull(PullAxis.Any, 1.2f, 1.5f, 7f, 0.25f);

            BoxIdentity identity = go.AddComponent<BoxIdentity>();
            identity.Configure(color, name);

            return identity;
        }

        private static BoxSnapTarget CreatePit(Transform root, string name, Vector2 position,
                                               HookColor color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(position.x, position.y, 0f);
            go.transform.localScale = new Vector3(1.4f, 0.8f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = hatchSprite;
            sr.color = new Color(0.65f, 0.70f, 0.76f, 0.85f);
            sr.sortingOrder = 2;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            col.isTrigger = true;

            GameObject snap = new GameObject("SnapPoint");
            snap.transform.SetParent(go.transform, false);
            snap.transform.localPosition = Vector3.zero;

            BoxSnapTarget target = go.AddComponent<BoxSnapTarget>();
            target.Configure(color, snap.transform, 0.18f);

            return target;
        }

        private static LaserHazard CreateLaser(Transform root, LevelController level,
                                               Vector2 center, float height)
        {
            GameObject go = new GameObject("LASER 1");
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(center.x, center.y, 0f);

            GameObject beam = new GameObject("Beam");
            beam.transform.SetParent(go.transform, false);
            beam.transform.localScale = new Vector3(0.14f, height, 1f);
            SpriteRenderer beamSR = beam.AddComponent<SpriteRenderer>();
            beamSR.sprite = whiteSprite;
            beamSR.color = new Color(1f, 0.20f, 0.25f, 1f);
            beamSR.sortingOrder = 6;

            GameObject glow = new GameObject("Glow");
            glow.transform.SetParent(go.transform, false);
            glow.transform.localScale = new Vector3(0.55f, height, 1f);
            SpriteRenderer glowSR = glow.AddComponent<SpriteRenderer>();
            glowSR.sprite = whiteSprite;
            glowSR.color = new Color(1f, 0.20f, 0.25f, 0.25f);
            glowSR.sortingOrder = 5;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.4f, height);
            col.isTrigger = true;

            LaserHazard hazard = go.AddComponent<LaserHazard>();
            hazard.Configure(beamSR, glowSR, col, level, true);

            return hazard;
        }

        private static void CreateMovingBall(Transform root, Vector2 center,
                                             float bottomY, float topY)
        {
            GameObject pointA = new GameObject("BolaPontoA");
            pointA.transform.SetParent(root, false);
            pointA.transform.position = new Vector3(center.x, bottomY, 0f);

            GameObject pointB = new GameObject("BolaPontoB");
            pointB.transform.SetParent(root, false);
            pointB.transform.position = new Vector3(center.x, topY, 0f);

            GameObject rail = new GameObject("TrilhoBola");
            rail.transform.SetParent(root, false);
            rail.transform.position = new Vector3(center.x, (bottomY + topY) * 0.5f, 0f);
            rail.transform.localScale = new Vector3(0.08f, topY - bottomY, 1f);
            SpriteRenderer railSR = rail.AddComponent<SpriteRenderer>();
            railSR.sprite = whiteSprite;
            railSR.color = new Color(0.35f, 0.38f, 0.45f);
            railSR.sortingOrder = 1;

            GameObject ball = new GameObject("Bola Amarela");
            ball.transform.SetParent(root, false);
            ball.transform.position = new Vector3(center.x, center.y, 0f);
            ball.transform.localScale = new Vector3(1.8f, 1.8f, 1f);

            SpriteRenderer sr = ball.AddComponent<SpriteRenderer>();
            sr.sprite = circleSprite;
            sr.color = new Color(1f, 0.86f, 0.25f);
            sr.sortingOrder = 4;

            CircleCollider2D col = ball.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            Rigidbody2D rb = ball.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            HookAnchor anchor = ball.AddComponent<HookAnchor>();
            anchor.Configure(HookColor.Neutral, HookAnchorMotion.Swing);

            MovingAnchor move = ball.AddComponent<MovingAnchor>();
            move.Configure(pointA.transform, pointB.transform, 2.2f);
        }

        private static void CreateSpikeStrip(Transform root, LevelController level)
        {
            GameObject strip = new GameObject("Espinhos");
            strip.transform.SetParent(root, false);
            strip.transform.position = new Vector3(0f, -8.7f, 0f);

            for (float x = -23f; x <= 23f; x += 1f)
            {
                GameObject spike = new GameObject("Spike");
                spike.transform.SetParent(strip.transform, false);
                spike.transform.localPosition = new Vector3(x, 0f, 0f);
                spike.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

                SpriteRenderer sr = spike.AddComponent<SpriteRenderer>();
                sr.sprite = spikeSprite;
                sr.color = new Color(0.55f, 0.16f, 0.18f);
                sr.sortingOrder = 2;
            }

            BoxCollider2D col = strip.AddComponent<BoxCollider2D>();
            col.size = new Vector2(48f, 1.2f);
            col.isTrigger = true;

            LethalHazard hazard = strip.AddComponent<LethalHazard>();
            hazard.Configure(level);
        }

        private static HookSettings LoadOrCreateSettings()
        {
            string path = "Assets/_Project/Data/HookSettings.asset";
            HookSettings asset = AssetDatabase.LoadAssetAtPath<HookSettings>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<HookSettings>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.range = 10f;
            asset.cooldown = 0.12f;
            asset.failCooldown = 0.18f;
            asset.dualChordWindow = 0.07f;
            asset.hitMask = ~0;
            asset.minRopeLength = 1.5f;
            asset.maxRopeLength = 12f;
            asset.swingAcceleration = 30f;
            asset.maxGrappleSpeed = 26f;
            asset.attachReelDistance = 1.5f;
            asset.attachReelSpeed = 9f;
            asset.attachImpulse = 3.5f;
            asset.maxStretch = 1.2f;

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private static Sprite LoadOrCreateSprite(string name, int width, int height,
                                                 float pixelsPerUnit,
                                                 System.Func<int, int, Color> pixel)
        {
            string path = "Assets/_Project/Art/" + name + ".png";

            if (!File.Exists(path))
            {
                Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                        tex.SetPixel(x, y, pixel(x, y));
                }
                tex.Apply();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path);
            }

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }

        private static Color CirclePixel(int x, int y)
        {
            float dx = x - 31.5f;
            float dy = y - 31.5f;
            float distance = Mathf.Sqrt(dx * dx + dy * dy);
            float alpha = Mathf.Clamp01((31f - distance) / 2f);
            return new Color(1f, 1f, 1f, alpha);
        }

        private static Color HatchPixel(int x, int y)
        {
            bool line = ((x + y) % 8) < 3;
            return line ? new Color(1f, 1f, 1f, 0.9f) : new Color(1f, 1f, 1f, 0.08f);
        }

        private static Color SpikePixel(int x, int y)
        {
            // Triângulo apontando para cima.
            float half = 7f;
            float center = 7.5f;
            float width = Mathf.Lerp(half, 0f, y / 15f);
            return Mathf.Abs(x - center) <= width
                ? Color.white
                : new Color(1f, 1f, 1f, 0f);
        }

        private static Color ColorFor(HookColor color)
        {
            switch (color)
            {
                case HookColor.Red:
                    return new Color(0.95f, 0.30f, 0.36f);
                case HookColor.Blue:
                    return new Color(0.30f, 0.62f, 0.95f);
                default:
                    return new Color(0.95f, 0.82f, 0.35f);
            }
        }
    }
}
#endif
