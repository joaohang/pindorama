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

        [Header("Wall poses (placeholders até arte final)")]
        [SerializeField] private Sprite[] wallClingFrames = new Sprite[2];
        [SerializeField] private Sprite[] wallClimbUpFrames = new Sprite[4];
        [SerializeField] private Sprite[] wallClimbDownFrames = new Sprite[4];
        [SerializeField] private Sprite[] wallJumpFrames = new Sprite[2];
        [SerializeField] private float wallFrameRate = 8f;
        [SerializeField] private float wallJumpPoseDuration = 0.16f;

        [SerializeField] private float runReferenceSpeed = 7f;

        private PlayerController2D player;
        private DualHookController hooks;
        private WallClingController wall;
        private Rigidbody2D rb;
        private int facing = 1;
        private bool wasClinging;
        private float wallJumpPoseUntil;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int GroundedHash = Animator.StringToHash("isGrounded");

        public void Configure(SpriteRenderer body, Animator animator,
                              SpriteRenderer pose, Sprite[] frames,
                              Sprite[] cling = null, Sprite[] climbUp = null,
                              Sprite[] climbDown = null, Sprite[] wallJump = null)
        {
            bodyRenderer = body;
            bodyAnimator = animator;
            poseRenderer = pose;
            hangFrames = frames;
            wallClingFrames = cling;
            wallClimbUpFrames = climbUp;
            wallClimbDownFrames = climbDown;
            wallJumpFrames = wallJump;
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

            EnsureWallFrameFallbacks();

            if (poseRenderer != null)
                poseRenderer.enabled = false;
        }

        private void EnsureWallFrameFallbacks()
        {
            if (hangFrames == null || hangFrames.Length < 4)
                return;

            if (wallClingFrames == null || wallClingFrames.Length == 0)
                wallClingFrames = new[] { hangFrames[0], hangFrames[1] };
            if (wallClimbUpFrames == null || wallClimbUpFrames.Length == 0)
                wallClimbUpFrames = new[] { hangFrames[1], hangFrames[2], hangFrames[1], hangFrames[3] };
            if (wallClimbDownFrames == null || wallClimbDownFrames.Length == 0)
                wallClimbDownFrames = new[] { hangFrames[0], hangFrames[1], hangFrames[2], hangFrames[3] };
            if (wallJumpFrames == null || wallJumpFrames.Length == 0)
                wallJumpFrames = new[] { hangFrames[2], hangFrames[3] };
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
            bool wallJumping = Time.time < wallJumpPoseUntil;

            if (wasClinging && !clinging && rb != null && rb.linearVelocity.y > 1f)
                wallJumpPoseUntil = Time.time + wallJumpPoseDuration;
            wasClinging = clinging;
            wallJumping = Time.time < wallJumpPoseUntil;

            if (clinging)
            {
                float climb = wall != null ? wall.ClimbDirection : 0f;
                Sprite[] frames = climb > 0.1f ? wallClimbUpFrames :
                    (climb < -0.1f ? wallClimbDownFrames : wallClingFrames);
                ShowPoseSequence(frames, wallFrameRate);
            }
            else if (wallJumping)
            {
                ShowPoseSequence(wallJumpFrames, wallFrameRate);
            }
            else if (hooked || enemy || braced)
            {
                ShowPoseFrame(hangFrames, 3);
            }
            else
            {
                ShowBody();
            }

            if (bodyAnimator != null)
            {
                float speed = rb != null ? Mathf.Abs(rb.linearVelocity.x) : 0f;
                bodyAnimator.SetFloat(SpeedHash, Mathf.Clamp01(speed / Mathf.Max(0.01f, runReferenceSpeed)));
                bodyAnimator.SetBool(GroundedHash, player.IsGrounded && !hooked);
            }

            UpdateFacing(clinging, hooked || enemy || braced || wallJumping);
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

        private void ShowPoseSequence(Sprite[] frames, float fps)
        {
            if (frames == null || frames.Length == 0)
                return;

            int index = Mathf.FloorToInt(Time.time * Mathf.Max(1f, fps)) % frames.Length;
            ShowPoseFrame(frames, index);
        }

        private void ShowPoseFrame(Sprite[] frames, int index)
        {
            if (bodyRenderer != null)
                bodyRenderer.enabled = false;

            if (poseRenderer == null)
                return;

            poseRenderer.enabled = true;
            if (frames != null && frames.Length > 0)
            {
                int clamped = Mathf.Clamp(index, 0, frames.Length - 1);
                Sprite chosen = frames[clamped];
                if (chosen != null)
                {
                    poseRenderer.sprite = chosen;

                    // Hang e Jump têm pivôs diferentes. Alinha o pé do frame
                    // ao fundo do capsule em vez de deslocar o personagem.
                    float localHeight = chosen.rect.height / Mathf.Max(1f, chosen.pixelsPerUnit);
                    float y = -(0.5f - chosen.pivot.y) * localHeight;
                    poseRenderer.transform.localPosition = new Vector3(0f, y, 0f);
                }
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
