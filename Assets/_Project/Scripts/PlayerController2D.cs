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

        private readonly Collider2D[] groundResults = new Collider2D[8];

        public bool IsGrounded { get; private set; }
        public bool IsBraced { get { return isBraced; } }

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
            hooks = FindObjectOfType<DualHookController>();
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
        }

        private void FixedUpdate()
        {
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

            int n = Physics2D.OverlapCircleNonAlloc(origin, radius, groundResults);
            IsGrounded = false;
            for (int i = 0; i < n; i++)
            {
                Collider2D c = groundResults[i];
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
