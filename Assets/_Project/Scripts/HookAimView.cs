using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Mira visual: linha pontilhada da mão/peito até o ponto de impacto e um
    /// retículo em estrela no alvo. A cor do retículo indica a cor da âncora
    /// (vermelho, azul, dourado/neutro) ou cinza quando não há âncora.
    /// </summary>
    public class HookAimView : MonoBehaviour
    {
        private const int MaxDots = 32;
        private const float DotSpacing = 0.45f;

        [SerializeField] private DualHookController controller;
        [SerializeField] private PlayerController2D player;
        [SerializeField] private HookSettings settings;

        private Collider2D playerCollider;
        private SpriteRenderer[] dots;
        private SpriteRenderer reticle;

        private static readonly Color AimColor = new Color(0.25f, 0.95f, 0.90f, 1f);
        private static readonly Color NoTargetColor = new Color(0.62f, 0.68f, 0.74f, 1f);

        private void Awake()
        {
            BuildVisuals();

            if (player != null)
                playerCollider = player.GetComponent<Collider2D>();
        }

        public void Configure(DualHookController c, PlayerController2D p, HookSettings s)
        {
            controller = c;
            player = p;
            settings = s;

            if (player != null)
                playerCollider = player.GetComponent<Collider2D>();
        }

        private void BuildVisuals()
        {
            if (reticle != null)
                return;

            Sprite dotSprite = MakeSolidSprite(Color.white);
            Sprite reticleSprite = MakeCrosshairSprite();

            dots = new SpriteRenderer[MaxDots];
            for (int i = 0; i < MaxDots; i++)
            {
                GameObject go = new GameObject("AimDot_" + i);
                go.transform.SetParent(transform, false);
                go.transform.localScale = Vector3.one * 0.09f;

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = dotSprite;
                sr.color = AimColor;
                sr.sortingOrder = 30;
                sr.enabled = false;
                dots[i] = sr;
            }

            GameObject reticleGO = new GameObject("AimReticle");
            reticleGO.transform.SetParent(transform, false);
            reticleGO.transform.localScale = Vector3.one * 0.75f;

            reticle = reticleGO.AddComponent<SpriteRenderer>();
            reticle.sprite = reticleSprite;
            reticle.color = AimColor;
            reticle.sortingOrder = 31;
        }

        private void LateUpdate()
        {
            if (controller == null || player == null || settings == null || reticle == null)
                return;

            Vector2 origin = (Vector2)player.transform.position;
            Vector2 dir = controller.AimDirection;
            if (dir.sqrMagnitude < 0.001f)
                dir = Vector2.right;

            Vector2 end = origin + dir * settings.range;
            HookColor? targetColor = null;

            RaycastHit2D[] buffer = new RaycastHit2D[16];
            int n = Physics2D.RaycastNonAlloc(origin, dir, buffer, settings.range, settings.hitMask);

            for (int i = 0; i < n; i++)
            {
                Collider2D c = buffer[i].collider;
                if (c == null || c == playerCollider)
                    continue;

                end = buffer[i].point;
                HookAnchor anchor = c.GetComponent<HookAnchor>();
                if (anchor != null)
                    targetColor = anchor.Color;
                break;
            }

            float distance = Vector2.Distance(origin, end);
            int count = Mathf.Clamp(Mathf.FloorToInt(distance / DotSpacing), 0, MaxDots);

            for (int i = 0; i < MaxDots; i++)
            {
                if (i < count)
                {
                    float t = (i + 1f) / (count + 1f);
                    Vector3 pos = Vector3.Lerp(
                        new Vector3(origin.x, origin.y, 0f),
                        new Vector3(end.x, end.y, 0f),
                        t);
                    dots[i].transform.position = pos;
                    dots[i].color = AimColor;
                    dots[i].enabled = true;
                }
                else
                {
                    dots[i].enabled = false;
                }
            }

            reticle.transform.position = new Vector3(end.x, end.y, 0f);
            reticle.color = targetColor.HasValue ? ColorFor(targetColor.Value) : NoTargetColor;

            // Pulso do retículo quando o Pendulum Boost está pronto.
            bool boostReady = controller.IsSwinging && controller.BoostReady;
            float reticleScale = boostReady
                ? 0.75f + Mathf.Sin(Time.time * 12f) * 0.08f
                : 0.75f;
            reticle.transform.localScale = Vector3.one * reticleScale;
        }

        private static Color ColorFor(HookColor color)
        {
            switch (color)
            {
                case HookColor.Red:
                    return new Color(1f, 0.36f, 0.42f, 1f);
                case HookColor.Blue:
                    return new Color(0.30f, 0.72f, 1f, 1f);
                default:
                    return new Color(1f, 0.85f, 0.40f, 1f);
            }
        }

        private static Sprite MakeSolidSprite(Color color)
        {
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }

        private static Sprite MakeCrosshairSprite()
        {
            const int size = 24;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            Color clear = new Color(0f, 0f, 0f, 0f);
            Color white = Color.white;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, clear);
            }

            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);

            // 8 pontas (estrela) com um vão no meio, como na referência.
            for (int a = 0; a < 8; a++)
            {
                float angle = a * Mathf.PI / 4f;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                for (float r = 4.5f; r <= 10f; r += 0.5f)
                {
                    DrawPixel(tex, center + dir * r, white);
                    DrawPixel(tex, center - dir * r, white);
                }
            }

            // Ponto central.
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                    DrawPixel(tex, center + new Vector2(x, y), white);
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static void DrawPixel(Texture2D tex, Vector2 p, Color color)
        {
            int x = Mathf.RoundToInt(p.x);
            int y = Mathf.RoundToInt(p.y);
            if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                tex.SetPixel(x, y, color);
        }
    }
}
