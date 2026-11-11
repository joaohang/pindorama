using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Movimento base do jogador: corrida, pulo, coyote, jump buffer e
    /// estado de fixação (FixedInteraction) usado pelos puxáveis.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movimento")]
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float groundAccel = 60f;
        [SerializeField] private float groundDecel = 80f;
        [SerializeField] private float airAccel = 30f;
        [SerializeField] private float airDecel = 10f;

        [Header("Pulo")]
        [SerializeField] private float jumpVelocity = 16.8f;
        [Tooltip("Impulso vertical ao saltar soltando o gancho no ar (salto de gancho).")]
        [SerializeField] private float grappleJumpVelocity = 13f;
        [SerializeField] private float gravityScale = 4.5f;
        [Tooltip("Gravidade reduzida enquanto há um gancho de swing ativo (suspensão heroica).")]
        [SerializeField] private float swingGravityScale = 3.4f;
        [SerializeField] private float maxFallSpeed = 24f;
        [SerializeField] private float coyoteTime = 0.1f;
        [SerializeField] private float jumpBuffer = 0.12f;

        private Rigidbody2D rb;
        private Collider2D col;
        private DualHookController hooks;

        private bool isBraced;
        private float cachedGravityScale;
        private float coyoteTimer;
        private float jumpBufferTimer;
        private float horizontal;

        private int movingRideCount;
        private Transform rideOriginalParent;
        private bool rideOriginalSimulated = true;
        private float rideOriginalGravityScale = 1f;


        public bool IsGrounded { get; private set; }
        public bool IsBraced { get { return isBraced; } }
        public bool IsRidingMovingAnchor { get { return movingRideCount > 0; } }
        public float Speed { get { return rb != null ? rb.GetVelocity().magnitude : 0f; } }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            rb.gravityScale = gravityScale;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            cachedGravityScale = gravityScale;
        }

        private void Start()
        {
            hooks = FindAnyObjectByType<DualHookController>();
        }

        /// <summary>
        /// Prende o jogador à âncora móvel (Bola Amarela) para ele ser
        /// transportado sem escorregar. A física fica desligada enquanto o
        /// transform é filho da âncora.
        /// </summary>
        public void BeginMovingRide(Transform anchor)
        {
            if (anchor == null)
                return;

            if (movingRideCount == 0)
            {
                rideOriginalParent = transform.parent;
                rideOriginalSimulated = rb != null && rb.simulated;

                if (rb != null)
                {
                    rideOriginalGravityScale = rb.gravityScale;
                    rb.linearVelocity = Vector2.zero;
                    rb.gravityScale = 0f;
                    rb.simulated = false;
                }

                transform.SetParent(anchor, true);
            }

            movingRideCount++;
        }

        public void EndMovingRide()
        {
            if (movingRideCount <= 0)
                return;

            movingRideCount--;
            if (movingRideCount > 0)
                return;

            transform.SetParent(rideOriginalParent, true);

            if (rb != null)
            {
                rb.simulated = rideOriginalSimulated;
                rb.gravityScale = rideOriginalGravityScale;
                rb.position = transform.position;
                rb.linearVelocity = Vector2.zero;
            }
        }

        private void Update()
        {
            horizontal = Input.GetAxisRaw("Horizontal");

            bool jumpPressed =
                Input.GetKeyDown(KeyCode.Space) ||
                (Input.GetJoystickNames().Length > 0 && Input.GetKeyDown(KeyCode.JoystickButton0));

            bool dropPressed =
                Input.GetKeyDown(KeyCode.LeftControl) ||
                (Input.GetJoystickNames().Length > 0 && Input.GetKeyDown(KeyCode.JoystickButton1));

            if (jumpPressed)
            {
                bool wasAttached = hooks != null && hooks.AnyAttached;

                if (wasAttached)
                    hooks.ReleaseAll(DetachReason.Jump);

                if (wasAttached && !IsGrounded && coyoteTimer <= 0f)
                {
                    // Salto de gancho: solta a corda e impulsiona para cima,
                    // preservando o momentum horizontal do balanço.
                    Vector2 velocity = rb.GetVelocity();
                    rb.SetVelocity(new Vector2(velocity.x, Mathf.Max(velocity.y, grappleJumpVelocity)));
                    jumpBufferTimer = 0f;
                    coyoteTimer = 0f;
                }
                else
                {
                    jumpBufferTimer = jumpBuffer;
                }
            }

            if (dropPressed)
            {
                if (hooks != null)
                    hooks.ReleaseAll(DetachReason.Drop);
            }

            if (jumpBufferTimer > 0f)
                jumpBufferTimer -= Time.deltaTime;

            if (IsGrounded)
                coyoteTimer = coyoteTime;
            else
                coyoteTimer -= Time.deltaTime;

            UpdateGravity();
        }

        /// <summary>
        /// Suspensão heroica: com gancho de swing ativo, a gravidade cai um
        /// pouco. No brace a gravidade já é zero e não mexemos nela.
        /// </summary>
        private void UpdateGravity()
        {
            if (isBraced || IsRidingMovingAnchor)
                return;

            bool swinging = hooks != null &&
                ((hooks.RedHand != null && hooks.RedHand.IsSwing) ||
                 (hooks.BlueHand != null && hooks.BlueHand.IsSwing));

            rb.gravityScale = swinging ? swingGravityScale : gravityScale;
        }

        private void FixedUpdate()
        {
            if (IsRidingMovingAnchor)
                return;

            if (isBraced)
            {
                rb.SetVelocity(Vector2.zero);
                return;
            }

            float targetVX = horizontal * moveSpeed;
            float accel = IsGrounded ? groundAccel : airAccel;
            float decel = IsGrounded ? groundDecel : airDecel;

            Vector2 vel = rb.GetVelocity();

            if (Mathf.Abs(horizontal) > 0.01f)
            {
                float vx = Mathf.MoveTowards(vel.x, targetVX, accel * Time.fixedDeltaTime);
                rb.SetVelocity(new Vector2(vx, vel.y));
            }
            else
            {
                float vx = Mathf.MoveTowards(vel.x, 0f, decel * Time.fixedDeltaTime);
                rb.SetVelocity(new Vector2(vx, vel.y));
            }

            vel = rb.GetVelocity();
            if (vel.y < -maxFallSpeed)
                rb.SetVelocity(new Vector2(vel.x, -maxFallSpeed));

            if (jumpBufferTimer > 0f && coyoteTimer > 0f)
            {
                vel = rb.GetVelocity();
                rb.SetVelocity(new Vector2(vel.x, jumpVelocity));
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
            }

            UpdateGrounded();
        }

        private void UpdateGrounded()
        {
            float radius = col.bounds.extents.x * 0.9f;
            Vector2 origin = (Vector2)transform.position - new Vector2(0f, col.bounds.extents.y + 0.01f);

            Collider2D[] results = Physics2D.OverlapCircleAll(origin, radius);
            IsGrounded = false;
            for (int i = 0; i < results.Length; i++)
            {
                Collider2D c = results[i];
                if (c == null || c == col)
                    continue;
                if (c.isTrigger)
                    continue;
                IsGrounded = true;
                break;
            }
        }

        /// <summary>
        /// Estado de "âncora" com os dois ganchos presos: o jogador para no ar
        /// (ou no chão) para mirar/puxar. Só deve ser chamado quando os dois
        /// ganchos estiverem presos.
        /// </summary>
        public void EnterBrace()
        {
            if (isBraced)
                return;

            isBraced = true;
            cachedGravityScale = rb.gravityScale;
            rb.gravityScale = 0f;
            rb.SetVelocity(Vector2.zero);
        }

        public void ExitBrace()
        {
            if (!isBraced)
                return;

            isBraced = false;
            rb.gravityScale = cachedGravityScale;
        }

        public void Teleport(Vector2 position)
        {
            rb.position = position;
            rb.SetVelocity(Vector2.zero);
        }
    }
}
