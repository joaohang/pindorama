using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Visual do cabo: LineRenderer segmentado (textura de corrente) + uma
    /// segunda linha mais larga e aditiva para o brilho neon. A corda estica
    /// rapidamente ao prender e fica com uma leve oscilação senoidal.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class HookRopeView : MonoBehaviour
    {
        private const int PointCount = 14;

        [SerializeField] private float stretchDuration = 0.12f;
        [SerializeField] private float waveAmplitude = 0.12f;
        [SerializeField] private float waveFrequency = 12f;
        [SerializeField] private float segmentLength = 0.45f;

        private HookHand hand;
        private Transform muzzle;
        private LineRenderer line;
        private LineRenderer glow;
        private Color coreColor = Color.white;

        private void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = PointCount;
            line.startWidth = 0.06f;
            line.endWidth = 0.06f;
            line.numCapVertices = 2;
            line.textureMode = LineTextureMode.Tile;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.material.mainTexture = CreateDashTexture();
            line.sortingOrder = 10;
            line.enabled = false;

            GameObject glowGO = new GameObject("Glow");
            glowGO.transform.SetParent(transform, false);

            glow = glowGO.AddComponent<LineRenderer>();
            glow.useWorldSpace = true;
            glow.positionCount = PointCount;
            glow.startWidth = 0.20f;
            glow.endWidth = 0.20f;
            glow.numCapVertices = 2;
            glow.material = new Material(MakeAdditiveShader());
            glow.sortingOrder = 9;
            glow.enabled = false;
        }

        public void Configure(HookHand handRef, Transform muzzleTransform, Color color)
        {
            hand = handRef;
            muzzle = muzzleTransform;

            coreColor = Color.Lerp(color, Color.white, 0.25f);
            Color glowColor = new Color(color.r, color.g, color.b, 0.28f);

            if (line != null)
            {
                line.startColor = coreColor;
                line.endColor = coreColor;
            }

            if (glow != null)
            {
                glow.startColor = glowColor;
                glow.endColor = glowColor;
            }
        }

        private void LateUpdate()
        {
            if (hand == null || muzzle == null || !hand.IsAttached || hand.Anchor == null)
            {
                if (line != null) line.enabled = false;
                if (glow != null) glow.enabled = false;
                return;
            }

            Vector3 start = muzzle.position;
            Vector3 target = hand.VisualAttachPoint;

            float stretch = Mathf.Clamp01((Time.time - hand.AttachedAt) / stretchDuration);
            Vector3 end = Vector3.Lerp(start, target, stretch);

            Vector3 delta = end - start;
            float length = delta.magnitude;
            if (length < 0.01f)
            {
                line.enabled = false;
                glow.enabled = false;
                return;
            }

            Vector3 direction = delta / length;
            Vector3 normal = new Vector3(-direction.y, direction.x, 0f);

            for (int i = 0; i < PointCount; i++)
            {
                float t = i / (PointCount - 1f);
                Vector3 point = Vector3.Lerp(start, end, t);

                // Oscilação senoidal que desaparece nas pontas (curva elástica).
                float wave = Mathf.Sin(t * Mathf.PI) *
                             waveAmplitude *
                             Mathf.Sin(Time.time * waveFrequency + t * 6f);
                point += normal * wave;

                line.SetPosition(i, point);
                glow.SetPosition(i, point);
            }

            line.material.mainTextureScale = new Vector2(Mathf.Max(1f, length / segmentLength), 1f);

            line.enabled = true;
            glow.enabled = true;
        }

        private static Shader MakeAdditiveShader()
        {
            Shader shader = Shader.Find("Legacy Shaders/Particles/Additive");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");
            return shader;
        }

        private static Texture2D CreateDashTexture()
        {
            const int width = 32;
            const int height = 4;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Point;

            for (int x = 0; x < width; x++)
            {
                // 5 pixels "acesos" e 3 apagados: elos de corrente.
                bool on = (x % 8) < 5;
                Color color = on ? Color.white : new Color(1f, 1f, 1f, 0f);
                for (int y = 0; y < height; y++)
                    texture.SetPixel(x, y, color);
            }

            texture.Apply();
            return texture;
        }
    }
}
