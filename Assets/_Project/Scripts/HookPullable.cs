using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Objeto puxável (pedra/bloco). Kinematic, sem gravidade e sem swing.
    /// O jogador fica fixo (via PlayerController2D) e cada aperto arrasta o
    /// objeto um pouco na direção do jogador (pedra pesada).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class HookPullable : MonoBehaviour
    {
        [Header("Puxão")]
        [SerializeField] private PullAxis axis = PullAxis.Horizontal;
        [SerializeField] private float pullSpeed = 6f;
        [SerializeField] private float pullStepDistance = 0.7f;
        [SerializeField] private float pullStopDistance = 1.4f;
        [SerializeField] private float pullCooldown = 0.3f;

        private Rigidbody2D rb;
        private Collider2D ownCollider;
        private bool isPulling;
        private bool locked;
        private float nextPullTime;
        private Vector2 target;

        public bool IsPulling { get { return isPulling; } }
        public bool IsLocked { get { return locked; } }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            ownCollider = GetComponent<Collider2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = false;
        }

        public void ConfigurePull(PullAxis pullAxis, float stepDistance, float stopDistance,
                                  float speed, float cooldown)
        {
            axis = pullAxis;
            pullStepDistance = stepDistance;
            pullStopDistance = stopDistance;
            pullSpeed = speed;
            pullCooldown = cooldown;
        }

        public void SetLocked(bool value)
        {
            locked = value;
            if (locked)
                isPulling = false;
        }

        private void FixedUpdate()
        {
            if (!isPulling || locked)
                return;

            Vector2 current = rb.position;
            Vector2 delta = target - current;
            float remaining = delta.magnitude;

            if (remaining < 0.01f)
            {
                Finish();
                return;
            }

            Vector2 dir = delta / remaining;
            float step = Mathf.Min(pullSpeed * Time.fixedDeltaTime, remaining);

            // BoxCast com uma caixa levemente menor que a do objeto para não
            // "bater" no chão em que ele está apoiado.
            Vector2 size = ownCollider.bounds.size * 0.8f;
            RaycastHit2D[] hits = Physics2D.BoxCastAll(current, size, 0f, dir, step + 0.02f, ~0);
            float allowed = step;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D c = hits[i].collider;
                if (c == null || c == ownCollider)
                    continue;

                // O jogador pode estar no caminho; ele não bloqueia o puxão.
                if (c.GetComponentInParent<PlayerController2D>() != null)
                    continue;

                float d = hits[i].distance - 0.02f;
                if (d < allowed)
                    allowed = Mathf.Max(0f, d);
            }

            if (allowed <= 0.001f)
            {
                Finish();
                return;
            }

            rb.MovePosition(current + dir * allowed);
        }

        public bool TryBeginPull(PlayerController2D player)
        {
            if (isPulling || locked)
                return false;
            if (player == null)
                return false;
            if (Time.time < nextPullTime)
                return false;

            Vector2 toPlayer = (Vector2)player.transform.position - rb.position;
            if (axis == PullAxis.Horizontal)
                toPlayer.y = 0f;

            float distanceToPlayer = toPlayer.magnitude;
            if (distanceToPlayer <= pullStopDistance + 0.05f)
                return false;

            // Cada puxão arrasta só um passo (pedra pesada).
            float step = Mathf.Min(pullStepDistance, distanceToPlayer - pullStopDistance);
            if (step <= 0.01f)
                return false;

            Vector2 dir = toPlayer / distanceToPlayer;
            target = rb.position + dir * step;
            if (axis == PullAxis.Horizontal)
                target.y = rb.position.y;

            isPulling = true;
            nextPullTime = Time.time + pullCooldown;
            return true;
        }

        public void CancelPull()
        {
            isPulling = false;
        }

        private void Finish()
        {
            isPulling = false;
        }
    }
}
