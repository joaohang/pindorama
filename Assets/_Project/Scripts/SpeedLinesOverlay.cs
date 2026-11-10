using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Linhas de velocidade em screen-space (OnGUI) quando o jogador passa do
    /// limiar de velocidade no gancho. Intensidade cresce com a velocidade.
    /// Também desenha o flash rápido do Pendulum Boost.
    /// </summary>
    public class SpeedLinesOverlay : MonoBehaviour
    {
        [SerializeField] private float speedThreshold = 13f;
        [SerializeField] private float maxSpeed = 26f;
        [SerializeField] private int lineCount = 24;

        [SerializeField] private PlayerController2D player;

        private Texture2D white;
        private float flashUntil;

        public void Configure(PlayerController2D p)
        {
            player = p;
        }

        public void Flash()
        {
            flashUntil = Time.time + 0.12f;
        }

        private void Awake()
        {
            white = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            white.SetPixel(0, 0, Color.white);
            white.Apply();
        }

        private void OnGUI()
        {
            if (player == null || Event.current.type != EventType.Repaint)
                return;

            GUI.depth = 1;

            float time = Time.unscaledTime;
            float flash = Mathf.Clamp01((flashUntil - Time.time) / 0.12f);

            if (flash > 0f)
            {
                Color oldColor = GUI.color;
                GUI.color = new Color(0.75f, 0.95f, 1f, 0.18f * flash);
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), white);
                GUI.color = oldColor;
            }

            float speed = player.Speed;
            if (speed <= speedThreshold)
                return;

            float intensity = Mathf.InverseLerp(speedThreshold, maxSpeed, speed);
            Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            float maxRadius = Mathf.Sqrt(center.x * center.x + center.y * center.y);

            for (int i = 0; i < lineCount; i++)
            {
                float noise = Mathf.PerlinNoise(i * 0.37f, time * 0.6f);
                float angle = (i / (float)lineCount) * Mathf.PI * 2f +
                              Mathf.Sin(time * 1.7f + i) * 0.06f;

                float radius = maxRadius * (0.45f + 0.30f * noise);
                float length = maxRadius * (0.10f + 0.25f * intensity) * (0.6f + 0.4f * noise);
                float alpha = 0.05f + 0.20f * intensity;

                Color oldColor = GUI.color;
                Matrix4x4 oldMatrix = GUI.matrix;

                GUI.color = new Color(0.75f, 0.92f, 1f, alpha);
                GUIUtility.RotateAroundPivot(angle * Mathf.Rad2Deg, center);
                GUI.DrawTexture(
                    new Rect(center.x - 0.75f, center.y - radius - length, 1.5f, length),
                    white);

                GUI.matrix = oldMatrix;
                GUI.color = oldColor;
            }
        }
    }
}
