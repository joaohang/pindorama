using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Mira + input dos dois ganchos. Disparo espelhado: vermelho = mira, azul = -mira.
    /// Puxáveis são alvos válidos; puxão espelhado = apertar os dois botões com as duas mãos presas.
    /// </summary>
    public class DualHookController : MonoBehaviour
    {
        [SerializeField] private PlayerController2D player;
        [SerializeField] private HookSettings settings;
        [SerializeField] private HookHand redHand;
        [SerializeField] private HookHand blueHand;

        [Tooltip("Se true, o brace só acontece quando pelo menos um dos ganchos está em um puxável. " +
                 "Se false, qualquer par de ganchos presos faz o brace.")]
        [SerializeField] private bool braceOnlyWithPullable = false;

        private Collider2D playerCollider;
        private Rigidbody2D playerBody;
        private Vector2 aimDirection = Vector2.right;

        private float redPressedAt = -1f;
        private float bluePressedAt = -1f;
        private bool redPending;
        private bool bluePending;

        public bool AnyAttached
        {
            get { return redHand.IsAttached || blueHand.IsAttached; }
        }

        public bool BothAttached
        {
            get { return redHand.IsAttached && blueHand.IsAttached; }
        }

        public Vector2 AimDirection { get { return aimDirection; } }
        public HookHand RedHand { get { return redHand; } }
        public HookHand BlueHand { get { return blueHand; } }

        private void Update()
        {
            if (playerCollider == null && player != null)
                playerCollider = player.GetComponent<Collider2D>();

            UpdateAim();

            bool redDown =
                Input.GetMouseButtonDown(0) ||
                (Input.GetJoystickNames().Length > 0 && Input.GetKeyDown(KeyCode.JoystickButton4));

            bool blueDown =
                Input.GetMouseButtonDown(1) ||
                (Input.GetJoystickNames().Length > 0 && Input.GetKeyDown(KeyCode.JoystickButton5));

            bool redHeld =
                Input.GetMouseButton(0) ||
                (Input.GetJoystickNames().Length > 0 && Input.GetKey(KeyCode.JoystickButton4));

            bool blueHeld =
                Input.GetMouseButton(1) ||
                (Input.GetJoystickNames().Length > 0 && Input.GetKey(KeyCode.JoystickButton5));

            if (redDown) OnRedPressed();
            if (blueDown) OnBluePressed();

            // Âncora de swing é "segure para manter": soltar o botão solta o gancho.
            ReleaseSwingIfReleased(redHand, redHeld);
            ReleaseSwingIfReleased(blueHand, blueHeld);

            if (redPending && Time.unscaledTime - redPressedAt > settings.dualChordWindow)
            {
                redPending = false;
                FireSingle(redHand);
            }

            if (bluePending && Time.unscaledTime - bluePressedAt > settings.dualChordWindow)
            {
                bluePending = false;
                FireSingle(blueHand);
            }

            UpdateBrace();
        }

        private void ReleaseSwingIfReleased(HookHand hand, bool held)
        {
            if (held || hand == null)
                return;
            if (!hand.IsSwing)
                return;

            hand.ForceDetach(DetachReason.Command);
        }

        /// <summary>
        /// Regra central: só "para" (brace) quando os DOIS ganchos estão presos.
        /// Com um gancho só, o jogador continua sob gravidade/movimento normal.
        /// </summary>
        private void UpdateBrace()
        {
            if (player == null)
                return;

            bool shouldBrace = BothAttached &&
                (!braceOnlyWithPullable || redHand.IsPullable || blueHand.IsPullable);

            if (shouldBrace)
            {
                if (!player.IsBraced)
                    player.EnterBrace();
            }
            else if (player.IsBraced)
            {
                player.ExitBrace();
            }
        }

        public void SetBraceRule(bool onlyWithPullable)
        {
            braceOnlyWithPullable = onlyWithPullable;
        }

        public void Configure(PlayerController2D p, HookSettings s, HookHand red, HookHand blue)
        {
            player = p;
            settings = s;
            redHand = red;
            blueHand = blue;
            playerBody = p != null ? p.GetComponent<Rigidbody2D>() : null;
        }

        private void FixedUpdate()
        {
            if (player == null || playerBody == null || settings == null)
                return;

            // O balanço só é aplicado no ar e fora do brace.
            if (player.IsBraced || player.IsGrounded)
                return;

            float horizontal = Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(horizontal) < 0.01f)
                return;

            Vector2 input = new Vector2(horizontal, 0f);
            bool applied = false;
            applied |= ApplySwingForce(redHand, input);
            applied |= ApplySwingForce(blueHand, input);

            if (applied)
            {
                Vector2 velocity = playerBody.GetVelocity();
                if (velocity.magnitude > settings.maxGrappleSpeed)
                    playerBody.SetVelocity(velocity.normalized * settings.maxGrappleSpeed);
            }
        }

        /// <summary>
        /// Força tangencial no sentido do input: acelera o pêndulo sem brigar
        /// com a restrição radial da corda.
        /// </summary>
        private bool ApplySwingForce(HookHand hand, Vector2 input)
        {
            if (hand == null || !hand.IsAttached || hand.Anchor == null)
                return false;
            if (hand.Anchor.Motion != HookAnchorMotion.Swing)
                return false;

            Vector2 toAnchor = hand.Anchor.AttachPoint - (Vector2)player.transform.position;
            if (toAnchor.sqrMagnitude < 0.01f)
                return false;

            Vector2 normal = toAnchor.normalized;
            Vector2 tangent = input - normal * Vector2.Dot(input, normal);
            playerBody.AddForce(tangent * settings.swingAcceleration, ForceMode2D.Force);
            return true;
        }

        private void UpdateAim()
        {
            // A mira usa o mouse. O stick direito não é lido aqui porque os eixos
            // "RightStickX/Y" não existem no Input Manager padrão (evita ArgumentException).
            Camera cam = Camera.main;
            if (cam == null)
                return;

            Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            Vector2 delta = (Vector2)mouseWorld - (Vector2)player.transform.position;
            if (delta.sqrMagnitude > 0.01f)
                aimDirection = delta.normalized;
        }

        private void OnRedPressed()
        {
            if (redHand.IsAttached)
            {
                redHand.PrimaryCommand();
                return;
            }

            redPressedAt = Time.unscaledTime;
            redPending = true;
            TryResolveChord();
        }

        private void OnBluePressed()
        {
            if (blueHand.IsAttached)
            {
                blueHand.PrimaryCommand();
                return;
            }

            bluePressedAt = Time.unscaledTime;
            bluePending = true;
            TryResolveChord();
        }

        private void TryResolveChord()
        {
            if (!redPending || !bluePending)
                return;

            if (Mathf.Abs(redPressedAt - bluePressedAt) > settings.dualChordWindow)
                return;

            redPending = false;
            bluePending = false;
            FireDual();
        }

        private Vector2 FireOrigin()
        {
            return (Vector2)player.transform.position + aimDirection * 0.1f;
        }

        private void FireSingle(HookHand hand)
        {
            hand.TryFire(FireOrigin(), aimDirection, playerCollider);
        }

        private void FireDual()
        {
            Vector2 origin = FireOrigin();
            redHand.TryFire(origin, aimDirection, playerCollider);
            blueHand.TryFire(origin, -aimDirection, playerCollider);
        }

        public void ReleaseAll(DetachReason reason)
        {
            if (player != null && player.IsBraced)
                player.ExitBrace();

            redHand.ForceDetach(reason);
            blueHand.ForceDetach(reason);
        }
    }
}
