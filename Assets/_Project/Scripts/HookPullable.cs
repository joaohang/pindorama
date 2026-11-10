using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Objeto puxável.
    /// - Modo kinematic (padrão): pedra/bloco que desliza em passos até o jogador.
    /// - Modo dinâmico: Rigidbody2D com gravidade; o puxão aplica velocidade
    ///   horizontal e o corpo cai/colide normalmente (ideal para caixas em pits).
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

        [Header("Corpo dinâmico (gravidade)")]
        [SerializeField] private bool useDynamicBody = false;
        [SerializeField] private float dynamicPullSpeed = 5f;
        [SerializeField] private float dynamicPullAcceleration = 45f;

        private Rigidbody2D rb;
        private Collider2D ownCollider;
        private bool isPulling;
        private bool locked;
        private float nextPullTime;
        private Vector2 target;

        public bool IsPulling { get { return isPulling; } }
        public bool IsLocked { get { return locked; } }
        public bool UsesDynamicBody { get { return useDynamicBody; } }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            ownCollider = GetComponent<Collider2D>();

            if (useDynamicBody)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.freezeRotation = true;
                rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
                if (rb.gravityScale <= 0.01f)
                    rb.gravityScale = 1f;
            }
            else
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.useFullKinematicContacts = false;
            }
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

        public void ConfigureDynamic(bool dynamicBody, float speed, float acceleration)
        {
            useDynamicBody = dynamicBody;
            dynamicPullSpeed = speed;
            dynamicPullAcceleration = acceleration;

            if (!Application.isPlaying || rb == null)
                return;

            if (useDynamicBody)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.freezeRotation = true;
                rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
                if (rb.gravityScale <= 0.01f)
                    rb.gravityScale = 1f;
            }
            else
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.useFullKinematicContacts = false;
            }
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

            if (useDynamicBody)
            {
                UpdateDynamicPull();
                return;
            }

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

        private void UpdateDynamicPull()
        {
            float dx = target.x - rb.position.x;
            if (Mathf.Abs(dx) < 0.05f)
            {
                Finish();
                return;
            }

            float direction = Mathf.Sign(dx);
            Vector2 velocity = rb.linearVelocity;
            velocity.x = Mathf.MoveTowards(
                velocity.x,
                direction * dynamicPullSpeed,
                dynamicPullAcceleration * Time.fixedDeltaTime);

            rb.linearVelocity = velocity;
        }

        public bool TryBeginPull(PlayerController2D player)
        {
            if (isPulling || locked)
                return false;
            if (player == null)
                return false;
            if (Time.time < nextPullTime)
                return false;

            if (useDynamicBody)
            {
                float dx = player.transform.position.x - rb.position.x;
                if (Mathf.Abs(dx) <= pullStopDistance + 0.05f)
                    return false;

                float direction = Mathf.Sign(dx);
                target = new Vector2(
                    player.transform.position.x - direction * pullStopDistance,
                    rb.position.y);

                isPulling = true;
                nextPullTime = Time.time + pullCooldown;
                return true;
            }

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
