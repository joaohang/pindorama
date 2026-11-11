using System.Collections.Generic;
using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Monta toda a sala de teste em código. Cole este componente num GameObject
    /// vazio de uma cena vazia e aperte Play. Não precisa configurar nada no inspector.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Spawn")]
        [SerializeField] private Vector2 spawn = new Vector2(0f, -1.5f);

        [Header("Regra do brace (parar no ar)")]
        [Tooltip("False = qualquer par de ganchos presos para o jogador. " +
                 "True = só para quando pelo menos um dos ganchos está em um puxável " +
                 "(preserva o swing com dois ganchos de parede).")]
        [SerializeField] private bool braceOnlyWithPullable = false;

        private void Awake()
        {
            Physics2D.queriesHitTriggers = true;

            HookSettings settings = ScriptableObject.CreateInstance<HookSettings>();
            settings.range = 8f;
            settings.cooldown = 0.12f;
            settings.failCooldown = 0.18f;
            settings.dualChordWindow = 0.07f;
            settings.minRopeLength = 1.5f;
            settings.maxRopeLength = 10f;
            settings.swingAcceleration = 30f;
            settings.maxGrappleSpeed = 26f;
            settings.attachReelDistance = 1.5f;
            settings.attachReelSpeed = 9f;
            settings.attachImpulse = 3.5f;
            settings.maxStretch = 1.2f;
            settings.hitMask = ~0;

            Camera cam = EnsureCamera();
            EnsureLight();

            // --- Estrutura da sala (corredor longo para testar swing) ---
            CreateStaticBox("Chão", new Vector2(0f, -3f), new Vector2(46f, 1f), new Color(0.28f, 0.31f, 0.38f));
            CreateStaticBox("ParedeEsquerda", new Vector2(-20f, 6f), new Vector2(1f, 18f), new Color(0.22f, 0.24f, 0.30f));
            CreateStaticBox("ParedeDireita", new Vector2(20f, 6f), new Vector2(1f, 18f), new Color(0.22f, 0.24f, 0.30f));

            // --- Corredor de âncoras de swing (estilo homem-aranha) ---
            CreateSwingAnchor("Ancora_12_R", new Vector2(-12f, 5.8f), HookColor.Red);
            CreateSwingAnchor("Ancora_08_B", new Vector2(-8f, 5.8f), HookColor.Blue);
            CreateSwingAnchor("Ancora_04_N", new Vector2(-4f, 5.8f), HookColor.Neutral);
            CreateSwingAnchor("Ancora_00_R", new Vector2(0f, 5.8f), HookColor.Red);
            CreateSwingAnchor("Ancora_04_B", new Vector2(4f, 5.8f), HookColor.Blue);
            CreateSwingAnchor("Ancora_08_N", new Vector2(8f, 5.8f), HookColor.Neutral);
            CreateSwingAnchor("Ancora_12_R", new Vector2(12f, 5.8f), HookColor.Red);

            // Âncora alta: fora do alcance parado (9,3 u); precisa pular para chegar em 8 u.
            // Fica deslocada para não ficar atrás da âncora de (0, 5.8).
            CreateSwingAnchor("AncoraAlta", new Vector2(2.5f, 7.8f), HookColor.Neutral);

            // Âncoras laterais para o disparo espelhado (um pouco acima dos blocos).
            CreateSwingAnchor("LadoEsquerdo_B", new Vector2(-7.2f, 1.2f), HookColor.Blue, 1.2f, 0.8f);
            CreateSwingAnchor("LadoDireito_R", new Vector2(7.2f, 1.2f), HookColor.Red, 1.2f, 0.8f);

            // --- Puxáveis (puxão em passos) ---
            HookPullable redBlock = CreatePullable("BlocoVermelho", new Vector2(3f, -1.6f), new Vector2(1.2f, 1.8f), HookColor.Red);
            HookPullable blueBlock = CreatePullable("BlocoAzul", new Vector2(6f, -1.6f), new Vector2(1.2f, 1.8f), HookColor.Blue);

            // --- Jogador ---
            GameObject playerGO = new GameObject("Player");
            playerGO.transform.position = new Vector3(spawn.x, spawn.y, 0f);

            Rigidbody2D prb = playerGO.AddComponent<Rigidbody2D>();
            prb.gravityScale = 4.5f;
            prb.freezeRotation = true;
            prb.interpolation = RigidbodyInterpolation2D.Interpolate;
            prb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CapsuleCollider2D pcol = playerGO.AddComponent<CapsuleCollider2D>();
            pcol.size = new Vector2(0.8f, 1.8f);
            pcol.direction = CapsuleDirection2D.Vertical;

            PlayerController2D player = playerGO.AddComponent<PlayerController2D>();
            playerGO.AddComponent<PlayerTrail>();

            // Visual do jogador (sprite filho, para não mexer no scale do collider).
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(playerGO.transform, false);
            SpriteRenderer vsr = visual.AddComponent<SpriteRenderer>();
            vsr.sprite = MakeSprite(Color.white);
            visual.transform.localScale = new Vector3(0.8f, 1.8f, 1f);

            PlayerBodyAlignment alignment = playerGO.AddComponent<PlayerBodyAlignment>();
            alignment.Configure(vsr, pcol);

            // Mãos (muzzle) com HookHand + HookRopeView.
            GameObject redMuzzle = new GameObject("HandRed");
            redMuzzle.transform.SetParent(playerGO.transform, false);
            redMuzzle.transform.localPosition = new Vector3(-0.35f, 0.1f, 0f);

            GameObject blueMuzzle = new GameObject("HandBlue");
            blueMuzzle.transform.SetParent(playerGO.transform, false);
            blueMuzzle.transform.localPosition = new Vector3(0.35f, 0.1f, 0f);

            HookHand redHand = redMuzzle.AddComponent<HookHand>();
            HookRopeView redRope = redMuzzle.AddComponent<HookRopeView>();
            redHand.Configure(HookColor.Red, redMuzzle.transform, settings, player, redRope);
            redRope.Configure(redHand, redMuzzle.transform, new Color(0.95f, 0.28f, 0.36f));

            HookHand blueHand = blueMuzzle.AddComponent<HookHand>();
            HookRopeView blueRope = blueMuzzle.AddComponent<HookRopeView>();
            blueHand.Configure(HookColor.Blue, blueMuzzle.transform, settings, player, blueRope);
            blueRope.Configure(blueHand, blueMuzzle.transform, new Color(0.24f, 0.60f, 0.90f));

            DualHookController controller = playerGO.AddComponent<DualHookController>();
            controller.Configure(player, settings, redHand, blueHand);
            controller.SetBraceRule(braceOnlyWithPullable);

            // --- Room reset ---
            GameObject roomGO = new GameObject("RoomController");
            RoomController room = roomGO.AddComponent<RoomController>();
            room.Configure(player, controller, spawn, new HookPullable[] { redBlock, blueBlock });

            // --- Kill zone ---
            GameObject killGO = CreateStaticBox("KillZone", new Vector2(0f, -9f), new Vector2(40f, 2f), new Color(0.6f, 0.1f, 0.1f));
            Collider2D killCol = killGO.GetComponent<Collider2D>();
            killCol.isTrigger = true;
            KillZone kill = killGO.AddComponent<KillZone>();
            kill.Configure(room);

            // --- HUD ---
            GameObject hudGO = new GameObject("HUD");
            HookHud hud = hudGO.AddComponent<HookHud>();
            hud.Configure(player, controller, redHand, blueHand);

            // --- Mira visual (linha pontilhada + retículo) ---
            GameObject aimGO = new GameObject("AimView");
            HookAimView aim = aimGO.AddComponent<HookAimView>();
            aim.Configure(controller, player, settings);

            // --- Speed lines em tela cheia ---
            GameObject speedGO = new GameObject("SpeedLines");
            SpeedLinesOverlay speedLines = speedGO.AddComponent<SpeedLinesOverlay>();
            speedLines.Configure(player);

            // --- Câmera ---
            CameraFollow follow = cam.GetComponent<CameraFollow>();
            if (follow == null)
                follow = cam.gameObject.AddComponent<CameraFollow>();
            follow.Configure(playerGO.transform);
        }

        private Camera EnsureCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }

            // Em 2D a câmera precisa ficar atrás do plano dos sprites (z = -10).
            // Sem isso, uma câmera criada em runtime fica em z = 0 e recorta todos
            // os sprites (o Game view mostra só o skybox).
            cam.transform.position = new Vector3(0f, 1f, -10f);
            cam.orthographic = true;
            cam.orthographicSize = 9.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.09f, 0.12f);
            return cam;
        }

        private void EnsureLight()
        {
            if (FindAnyObjectByType<Light>() == null)
            {
                GameObject go = new GameObject("Light");
                Light light = go.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1f;
            }
        }

        private GameObject CreateStaticBox(string name, Vector2 position, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.position = new Vector3(position.x, position.y, 0f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSprite(color);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            return go;
        }

        private GameObject CreateSwingAnchor(string name, Vector2 position, HookColor color,
                                             float width = 2.5f, float height = 0.5f)
        {
            GameObject go = CreateStaticBox(name, position, new Vector2(width, height), ColorFor(color));
            BoxCollider2D col = go.GetComponent<BoxCollider2D>();
            col.isTrigger = true;

            HookAnchor anchor = go.AddComponent<HookAnchor>();
            anchor.Configure(color, HookAnchorMotion.Swing);
            return go;
        }

        private HookPullable CreatePullable(string name, Vector2 position, Vector2 size, HookColor color)
        {
            GameObject go = CreateStaticBox(name, position, size, ColorFor(color));

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = false;

            HookAnchor anchor = go.AddComponent<HookAnchor>();
            anchor.Configure(color, HookAnchorMotion.Fixed);

            return go.AddComponent<HookPullable>();
        }

        private Sprite MakeSprite(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }

        private static Color ColorFor(HookColor color)
        {
            switch (color)
            {
                case HookColor.Red:
                    return new Color(0.95f, 0.28f, 0.36f);
                case HookColor.Blue:
                    return new Color(0.24f, 0.60f, 0.90f);
                default:
                    return new Color(0.92f, 0.82f, 0.45f);
            }
        }
    }
}
