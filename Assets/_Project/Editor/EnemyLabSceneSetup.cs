#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TwinHook;

namespace TwinHook.Editor
{
    /// <summary>
    /// Cria a cena EnemyLab com soldados e torretas dos 3 tipos de interaÃ§Ã£o:
    /// amarelo (estilingue), QTE azul e QTE vermelho.
    /// </summary>
    public static class EnemyLabSceneSetup
    {
        private static Sprite whiteSprite;
        private static HookSettings settings;

        [MenuItem("Pindorama/Criar EnemyLab")]
        public static void CreateScene()
        {
            whiteSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/WhiteSquare.png");
            settings = AssetDatabase.LoadAssetAtPath<HookSettings>("Assets/_Project/Data/HookSettings.asset");

            if (whiteSprite == null || settings == null)
            {
                Debug.LogError("EnemyLab: sprites ou HookSettings nÃ£o encontrados.");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Transform root = new GameObject("EnemyLab").transform;

            Vector2 spawn = new Vector2(-28f, -3.4f);
            HookHand redHand;
            HookHand blueHand;
            DualHookController controller;
            GameObject playerGO = CreatePlayer(root, spawn, out redHand, out blueHand, out controller);
            PlayerController2D player = playerGO.GetComponent<PlayerController2D>();

            CreateCamera(root, player);

            LevelController level = new GameObject("LevelController").AddComponent<LevelController>();
            level.transform.SetParent(root, false);

            GameObject spawnGO = new GameObject("SpawnPoint");
            spawnGO.transform.SetParent(root, false);
            spawnGO.transform.position = new Vector3(spawn.x, spawn.y, 0f);
            level.Configure(spawnGO.transform, player, controller, null);

            // ChÃ£o e plataformas.
            CreatePlatform(root, "ChÃ£o", new Vector2(0f, -5f), new Vector2(70f, 1f), new Color(0.28f, 0.31f, 0.38f));
            CreatePlatform(root, "PlataformaA", new Vector2(-20f, -3f), new Vector2(6f, 1f), new Color(0.30f, 0.33f, 0.40f));
            CreatePlatform(root, "PlataformaB", new Vector2(-10f, -3f), new Vector2(6f, 1f), new Color(0.30f, 0.33f, 0.40f));
            CreatePlatform(root, "PlataformaC", new Vector2(0f, -3f), new Vector2(6f, 1f), new Color(0.30f, 0.33f, 0.40f));
            CreatePlatform(root, "PlataformaD", new Vector2(10f, -3f), new Vector2(6f, 1f), new Color(0.30f, 0.33f, 0.40f));
            CreatePlatform(root, "PlataformaE", new Vector2(20f, -3f), new Vector2(6f, 1f), new Color(0.30f, 0.33f, 0.40f));

            CreatePlatform(root, "AltaEsq", new Vector2(-10f, 2.5f), new Vector2(6f, 1f), new Color(0.26f, 0.29f, 0.35f));
            CreatePlatform(root, "AltaDir", new Vector2(10f, 2.5f), new Vector2(6f, 1f), new Color(0.26f, 0.29f, 0.35f));
            CreatePlatform(root, "AltaDir2", new Vector2(20f, 2.5f), new Vector2(6f, 1f), new Color(0.26f, 0.29f, 0.35f));

            CreatePlatform(root, "ParedeEsq", new Vector2(-33f, 2f), new Vector2(1f, 16f), new Color(0.20f, 0.22f, 0.28f));
            CreateClimbableWall(root, "ParedeDireita", new Vector2(33f, 2f), new Vector2(1f, 16f));

            // Ã‚ncoras de teto para deslocamento.
            CreateAnchor(root, "Ancora_1", new Vector2(-24f, 6.5f), HookColor.Red);
            CreateAnchor(root, "Ancora_2", new Vector2(-12f, 6.5f), HookColor.Blue);
            CreateAnchor(root, "Ancora_3", new Vector2(0f, 6.5f), HookColor.Neutral);
            CreateAnchor(root, "Ancora_4", new Vector2(12f, 6.5f), HookColor.Red);
            CreateAnchor(root, "Ancora_5", new Vector2(24f, 6.5f), HookColor.Blue);

            // Soldados: um de cada tipo de interaÃ§Ã£o.
            CreateEnemy(root, "Soldado_Amarelo", new Vector2(-20f, -1.6f),
                EnemyType.Yellow, EnemyArchetype.Soldier, new Color(1f, 0.85f, 0.25f),
                new Vector2(1.2f, 1.8f), true);

            CreateEnemy(root, "Soldado_Azul_QTE", new Vector2(-10f, -1.6f),
                EnemyType.BlueQte, EnemyArchetype.Soldier, new Color(0.30f, 0.60f, 1f),
                new Vector2(1.2f, 1.8f), true);

            CreateEnemy(root, "Soldado_Vermelho_QTE", new Vector2(0f, -1.6f),
                EnemyType.RedQte, EnemyArchetype.Soldier, new Color(0.95f, 0.30f, 0.35f),
                new Vector2(1.2f, 1.8f), true);

            // Torretas: mesma interaÃ§Ã£o, corpo parado e rajadas.
            CreateEnemy(root, "Torreta_Amarela", new Vector2(-10f, 3.7f),
                EnemyType.Yellow, EnemyArchetype.Turret, new Color(1f, 0.85f, 0.25f),
                new Vector2(1.4f, 1.4f), false);

            CreateEnemy(root, "Torreta_Azul_QTE", new Vector2(10f, 3.7f),
                EnemyType.BlueQte, EnemyArchetype.Turret, new Color(0.30f, 0.60f, 1f),
                new Vector2(1.4f, 1.4f), false);

            CreateEnemy(root, "Torreta_Vermelha_QTE", new Vector2(20f, 3.7f),
                EnemyType.RedQte, EnemyArchetype.Turret, new Color(0.95f, 0.30f, 0.35f),
                new Vector2(1.4f, 1.4f), false);

            CreateUi(root, player, controller, redHand, blueHand);

            string scenePath = "Assets/_Project/Scenes/EnemyLab.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(scenePath, true),
                new EditorBuildSettingsScene("Assets/_Project/Scenes/Level02.unity", true),
                new EditorBuildSettingsScene("Assets/_Project/Scenes/Main.unity", true)
            };

            AssetDatabase.SaveAssets();
            Debug.Log("PINDORAMA: EnemyLab.unity criada.");
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
            playerGO.AddComponent<WallClingController>();

            return playerGO;
        }

        private static void CreateCamera(Transform root, PlayerController2D player)
        {
            GameObject camGO = new GameObject("Main Camera");
            camGO.transform.SetParent(root, false);
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(-26f, 0f, -10f);

            Camera cam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
            cam.orthographic = true;
            cam.orthographicSize = 9.5f;
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
            HookHud hookHud = hudGO.AddComponent<HookHud>();
            hookHud.Configure(player, controller, red, blue);
            hudGO.AddComponent<EnemyHud>();

            GameObject aimGO = new GameObject("AimView");
            aimGO.transform.SetParent(root, false);
            HookAimView aim = aimGO.AddComponent<HookAimView>();
            aim.Configure(controller, player, settings);

            GameObject speedGO = new GameObject("SpeedLines");
            speedGO.transform.SetParent(root, false);
            SpeedLinesOverlay speed = speedGO.AddComponent<SpeedLinesOverlay>();
            speed.Configure(player);
        }

        private static EnemyBase CreateEnemy(Transform root, string name, Vector2 position,
                                             EnemyType type, EnemyArchetype archetype,
                                             Color color, Vector2 size, bool patrol)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(position.x, position.y, 0f);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = whiteSprite;
            sr.color = color;
            sr.sortingOrder = 5;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            HookAnchor anchor = go.AddComponent<HookAnchor>();
            HookColor hookColor = type == EnemyType.BlueQte ? HookColor.Blue :
                (type == EnemyType.RedQte ? HookColor.Red : HookColor.Neutral);
            anchor.Configure(hookColor, HookAnchorMotion.Swing);

            EnemyBase enemy = go.AddComponent<EnemyBase>();
            enemy.Configure(type, archetype);

            if (archetype == EnemyArchetype.Turret)
            {
                go.AddComponent<TurretBehavior>();
            }
            else
            {
                go.AddComponent<SoldierBehavior>();
            }

            return enemy;
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

        private static void CreateClimbableWall(Transform root, string name, Vector2 center, Vector2 size)
        {
            GameObject wall = CreatePlatform(root, name, center, size, new Color(0.16f, 0.72f, 0.62f));
            int layer = LayerMask.NameToLayer("ClimbableWall");
            if (layer >= 0)
                wall.layer = layer;

            HookAnchor anchor = wall.AddComponent<HookAnchor>();
            anchor.Configure(HookColor.Neutral, HookAnchorMotion.Swing);
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

        private static Color ColorFor(HookColor color)
        {
            switch (color)
            {
                case HookColor.Red: return new Color(0.95f, 0.30f, 0.36f);
                case HookColor.Blue: return new Color(0.30f, 0.62f, 0.95f);
                default: return new Color(0.95f, 0.82f, 0.35f);
            }
        }
    }
}
#endif
