using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Visual do personagem cyberpunk 80x80.
    /// - Corpo (Idle/Run/Jump) usa Animator.
    /// - Pose (Hang1-4) é um SpriteRenderer separado para evitar o salto de pivô.
    /// - Wall cling/climb e hurt usam a pose padrão Hang1 até existirem frames próprios.
    /// </summary>
    public class PlayerCharacterVisual : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private Animator bodyAnimator;
        [SerializeField] private SpriteRenderer poseRenderer;
        [SerializeField] private Sprite[] hangFrames = new Sprite[4];

        [SerializeField] private float runReferenceSpeed = 7f;

        private PlayerController2D player;
        private DualHookController hooks;
        private WallClingController wall;
        private Rigidbody2D rb;
        private int facing = 1;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int GroundedHash = Animator.StringToHash("isGrounded");

        public void Configure(SpriteRenderer body, Animator animator,
                              SpriteRenderer pose, Sprite[] frames)
        {
            bodyRenderer = body;
            bodyAnimator = animator;
            poseRenderer = pose;
            hangFrames = frames;
        }

        private void Awake()
        {
            if (bodyRenderer == null)
                bodyRenderer = GetComponentInChildren<SpriteRenderer>();
            if (bodyAnimator == null)
                bodyAnimator = GetComponentInChildren<Animator>();
            if (poseRenderer == null)
            {
                Transform pose = transform.Find("Pose");
                if (pose != null)
                    poseRenderer = pose.GetComponent<SpriteRenderer>();
            }

            player = GetComponentInParent<PlayerController2D>();
            if (player != null)
            {
                hooks = player.GetComponent<DualHookController>();
                wall = player.GetComponent<WallClingController>();
                rb = player.GetComponent<Rigidbody2D>();
            }

            if (poseRenderer != null)
                poseRenderer.enabled = false;
        }

        private void LateUpdate()
        {
            EnsurePlayerReferences();
            if (player == null)
                return;

            bool clinging = wall != null && wall.IsClinging;
            bool hooked = hooks != null && hooks.AnyAttached;
            bool enemy = player.IsInEnemyInteraction || player.IsBeingPulledByEnemy;
            bool braced = player.IsBraced;

            bool poseActive = clinging || hooked || enemy || braced;
            if (poseActive)
                ShowPose(clinging ? 0 : 3);
            else
                ShowBody();

            if (bodyAnimator != null)
            {
                float speed = rb != null ? Mathf.Abs(rb.linearVelocity.x) : 0f;
                bodyAnimator.SetFloat(SpeedHash, Mathf.Clamp01(speed / Mathf.Max(0.01f, runReferenceSpeed)));
                bodyAnimator.SetBool(GroundedHash, player.IsGrounded && !hooked);
            }

            UpdateFacing(clinging, hooked || enemy || braced);
        }

        private void EnsurePlayerReferences()
        {
            if (player == null)
                player = GetComponentInParent<PlayerController2D>();
            if (player == null)
                return;

            if (hooks == null)
                hooks = player.GetComponent<DualHookController>();
            if (wall == null)
                wall = player.GetComponent<WallClingController>();
            if (rb == null)
                rb = player.GetComponent<Rigidbody2D>();

            // O GameBootstrap pode adicionar o visual branco/alinhamento depois
            // deste prefab; garante que o visual novo seja o único ativo.
            Transform oldVisual = player.transform.Find("Visual");
            if (oldVisual != null && oldVisual.gameObject.activeSelf)
                oldVisual.gameObject.SetActive(false);

            PlayerBodyAlignment alignment = player.GetComponent<PlayerBodyAlignment>();
            if (alignment != null && alignment.enabled)
                alignment.enabled = false;

            CapsuleCollider2D capsule = player.GetComponent<CapsuleCollider2D>();
            if (capsule != null)
            {
                capsule.size = new Vector2(0.8f, 1.8f);
                capsule.offset = Vector2.zero;
            }
        }

        private void ShowPose(int index)
        {
            if (bodyRenderer != null)
                bodyRenderer.enabled = false;

            if (poseRenderer == null)
                return;

            poseRenderer.enabled = true;
            if (hangFrames != null && hangFrames.Length > 0)
            {
                int clamped = Mathf.Clamp(index, 0, hangFrames.Length - 1);
                if (hangFrames[clamped] != null)
                    poseRenderer.sprite = hangFrames[clamped];
            }
        }

        private void ShowBody()
        {
            if (bodyRenderer != null)
                bodyRenderer.enabled = true;
            if (poseRenderer != null)
                poseRenderer.enabled = false;
        }

        private void UpdateFacing(bool clinging, bool hookPose)
        {
            if (clinging && wall != null)
            {
                facing = wall.WallSide;
            }
            else
            {
                float horizontal = Input.GetAxisRaw("Horizontal");
                if (Mathf.Abs(horizontal) > 0.1f)
                    facing = (int)Mathf.Sign(horizontal);
                else if (hookPose && hooks != null && Mathf.Abs(hooks.AimDirection.x) > 0.1f)
                    facing = (int)Mathf.Sign(hooks.AimDirection.x);
            }

            bool flip = facing < 0;
            if (bodyRenderer != null)
                bodyRenderer.flipX = flip;
            if (poseRenderer != null)
                poseRenderer.flipX = flip;
        }
    }
}
