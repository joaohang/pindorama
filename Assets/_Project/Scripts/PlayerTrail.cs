using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Rastro (Trail Renderer) que só liga quando a velocidade de tração passa
    /// do limiar. Dá a leitura de "motion blur" sem custo de post-processing.
    /// </summary>
    [RequireComponent(typeof(TrailRenderer))]
    public class PlayerTrail : MonoBehaviour
    {
        [SerializeField] private float speedThreshold = 12f;

        private PlayerController2D player;
        private TrailRenderer trail;

        private void Awake()
        {
            player = GetComponent<PlayerController2D>();
            trail = GetComponent<TrailRenderer>();

            trail.time = 0.28f;
            trail.startWidth = 0.55f;
            trail.endWidth = 0.04f;
            trail.minVertexDistance = 0.05f;
            trail.autodestruct = false;
            trail.emitting = false;
            trail.numCapVertices = 2;
            trail.textureMode = LineTextureMode.Stretch;
            trail.material = new Material(MakeAdditiveShader());
            trail.startColor = new Color(0.55f, 0.90f, 1f, 0.55f);
            trail.endColor = new Color(0.20f, 0.60f, 1f, 0f);
            trail.sortingOrder = -1;
        }

        private void Update()
        {
            if (player == null || trail == null)
                return;

            bool fast = !player.IsGrounded && player.Speed > speedThreshold;
            trail.emitting = fast;

            if (!fast)
                trail.Clear();
        }

        private static Shader MakeAdditiveShader()
        {
            Shader shader = Shader.Find("Legacy Shaders/Particles/Additive");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");
            return shader;
        }
    }
}
