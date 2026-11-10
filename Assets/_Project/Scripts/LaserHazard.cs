using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Laser letal. Enquanto ativo, contato com o jogador respawna no início.
    /// O BoxSyncPuzzle chama SetActive(false) permanentemente ao resolver.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LaserHazard : MonoBehaviour
    {
        [SerializeField] private bool active = true;
        [SerializeField] private SpriteRenderer beamRenderer;
        [SerializeField] private SpriteRenderer glowRenderer;
        [SerializeField] private Collider2D hazardCollider;
        [SerializeField] private LevelController level;
        [SerializeField] private float pulseSpeed = 6f;
        [SerializeField] private float minAlpha = 0.55f;

        private Color beamColor = new Color(1f, 0.2f, 0.25f, 1f);
        private Color glowColor = new Color(1f, 0.2f, 0.25f, 0.25f);
        private float nextHitTime;

        public bool IsActive { get { return active; } }

        public void Configure(SpriteRenderer beam, SpriteRenderer glow, Collider2D col,
                              LevelController levelController, bool startActive)
        {
            beamRenderer = beam;
            glowRenderer = glow;
            hazardCollider = col;
            level = levelController;
            active = startActive;
            CaptureColors();
            Apply();
        }

        private void Awake()
        {
            if (hazardCollider == null)
                hazardCollider = GetComponent<Collider2D>();
            CaptureColors();
            Apply();
        }

        private void CaptureColors()
        {
            if (beamRenderer != null)
                beamColor = beamRenderer.color;
            if (glowRenderer != null)
                glowColor = glowRenderer.color;
        }

        public void SetActive(bool value)
        {
            active = value;
            Apply();
        }

        private void Apply()
        {
            if (beamRenderer != null)
                beamRenderer.enabled = active;
            if (glowRenderer != null)
                glowRenderer.enabled = active;
            if (hazardCollider != null)
                hazardCollider.enabled = active;
        }

        private void Update()
        {
            if (!active || beamRenderer == null)
                return;

            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

            Color beam = beamColor;
            beam.a = Mathf.Lerp(minAlpha, 1f, t);
            beamRenderer.color = beam;

            if (glowRenderer != null)
            {
                Color glow = glowColor;
                glow.a = Mathf.Lerp(glowColor.a * 0.5f, glowColor.a, t);
                glowRenderer.color = glow;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Hit(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Hit(other);
        }

        private void Hit(Collider2D other)
        {
            if (!active || Time.time < nextHitTime)
                return;
            if (other.GetComponentInParent<PlayerController2D>() == null)
                return;

            nextHitTime = Time.time + 0.2f;
            if (level != null)
                level.RespawnPlayer();
        }
    }
}
