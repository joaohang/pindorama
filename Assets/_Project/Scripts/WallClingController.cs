using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Wall Cling / Wall Grab para paredes verticais marcadas com a layer
    /// "ClimbableWall". Detecta com dois raios (peito e cintura), trava o
    /// jogador na parede, permite wall jump, drop e soltar ao disparar o gancho.
    /// </summary>
    [DefaultExecutionOrder(100)]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerController2D))]
    public class WallClingController : MonoBehaviour
    {
        [Header("Detecção")]
        [SerializeField] private LayerMask climbableMask;
        [SerializeField] private float wallCheckDistance = 0.55f;
        [SerializeField] private float wallCheckHeight = 0.5f;
        [SerializeField] private float chestOffsetY = 0.35f;
        [SerializeField] private float waistOffsetY = -0.25f;
        [SerializeField] private float regrabCooldown = 0.2f;

        [Header("Wall Jump")]
        [SerializeField] private float wallJumpForceX = 12f;
        [SerializeField] private float wallJumpForceY = 15f;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer visual;

        private PlayerController2D player;
        private Rigidbody2D rb;
        private DualHookController hooks;

        private bool isClinging;
        private int facing = 1;
        private int wallSide;
        private Vector2 wallNormal = Vector2.left;
        private float cachedGravityScale = 4.5f;
        private RigidbodyConstraints2D cachedConstraints;
        private float regrabUntil;

        public bool IsClinging { get { return isClinging; } }

        private void OnValidate()
        {
            if (climbableMask.value == 0)
                climbableMask = LayerMask.GetMask("ClimbableWall");
            if (wallCheckDistance < 0.5f)
                wallCheckDistance = 0.55f;
        }

        private void Awake()
        {
            player = GetComponent<PlayerController2D>();
            rb = GetComponent<Rigidbody2D>();
            hooks = GetComponent<DualHookController>();

            if (visual == null)
                visual = GetComponentInChildren<SpriteRenderer>();

            if (climbableMask.value == 0)
                climbableMask = LayerMask.GetMask("ClimbableWall");

            // O collider do player tem ~0.4 de meio-largura; a checagem precisa
            // alcançar a superfície mesmo quando ele já está encostado.
            if (wallCheckDistance < 0.5f)
                wallCheckDistance = 0.55f;
        }

        private void Update()
        {
            if (isClinging)
            {
                UpdateClinging();
                return;
            }

            if (Time.time < regrabUntil)
                return;

            if (player == null || player.IsGrounded || player.IsBraced ||
                player.IsRidingMovingAnchor)
                return;

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vx = rb != null ? rb.linearVelocity.x : 0f;

            if (Mathf.Abs(horizontal) > 0.1f)
                facing = (int)Mathf.Sign(horizontal);
            else if (Mathf.Abs(vx) > 0.3f)
                facing = (int)Mathf.Sign(vx);

            int side = facing;
            bool hooked = hooks != null && hooks.AnyAttached;

            bool toward = (Mathf.Abs(horizontal) > 0.1f && Mathf.Sign(horizontal) == side) ||
                          (Mathf.Abs(vx) > 0.3f && Mathf.Sign(vx) == side) ||
                          hooked;

            if (!toward)
                return;

            Vector2 normal;
            bool found = ProbeWall(side, out normal);

            // Caso B: puxado pelo gancho sem velocidade horizontal clara;
            // testa o outro lado também.
            if (!found && hooked)
            {
                side = -side;
                found = ProbeWall(side, out normal);
            }

            if (found)
                EnterCling(side, normal);
        }

        private bool ProbeWall(int side, out Vector2 normal)
        {
            normal = Vector2.zero;

            Vector2 dir = side > 0 ? Vector2.right : Vector2.left;
            Vector2 chest = (Vector2)transform.position + new Vector2(0f, chestOffsetY);
            Vector2 waist = (Vector2)transform.position + new Vector2(0f, waistOffsetY);

            RaycastHit2D chestHit = Physics2D.Raycast(chest, dir, wallCheckDistance, climbableMask);
            RaycastHit2D waistHit = Physics2D.Raycast(waist, dir, wallCheckDistance, climbableMask);

            if (chestHit.collider == null || waistHit.collider == null)
                return false;

            // Dois pontos no MESMO collider e com normal horizontal garantem
            // parede vertical contínua, não quina/chão.
            if (chestHit.collider != waistHit.collider)
                return false;

            if (Mathf.Abs(chestHit.normal.x) < 0.6f || Mathf.Abs(waistHit.normal.x) < 0.6f)
                return false;

            normal = ((chestHit.normal + waistHit.normal) * 0.5f).normalized;
            return true;
        }

        private void EnterCling(int side, Vector2 normal)
        {
            isClinging = true;
            wallSide = side;
            wallNormal = normal;
            facing = side;

            cachedGravityScale = rb.gravityScale;
            cachedConstraints = rb.constraints;

            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.constraints |= RigidbodyConstraints2D.FreezePositionX |
                              RigidbodyConstraints2D.FreezeRotation;

            if (visual != null)
                visual.flipX = side < 0;

            if (hooks != null)
            {
                // Caso B: o gancho que trouxe até a parede é desconectado.
                hooks.ReleaseAll(DetachReason.AnchorLost);
                hooks.ResetAirResources();
            }
        }

        private void UpdateClinging()
        {
            Vector2 normal;
            if (!ProbeWall(wallSide, out normal))
            {
                ExitCling();
                return;
            }

            wallNormal = normal;

            // Gancho disparado de outro ponto: solta a parede e deixa o joint puxar.
            if (hooks != null && hooks.AnyAttached)
            {
                ExitCling();
                return;
            }

            bool jump = Input.GetKeyDown(KeyCode.Space) ||
                        (Input.GetJoystickNames().Length > 0 && Input.GetKeyDown(KeyCode.JoystickButton0));

            bool drop = Input.GetKeyDown(KeyCode.S) ||
                        Input.GetKeyDown(KeyCode.DownArrow) ||
                        Input.GetKeyDown(KeyCode.LeftControl) ||
                        (Input.GetJoystickNames().Length > 0 && Input.GetKeyDown(KeyCode.JoystickButton1));

            if (jump)
            {
                // Normal aponta da parede para o jogador: empurra para longe.
                Vector2 impulse = new Vector2(wallNormal.x * wallJumpForceX, wallJumpForceY);
                ExitCling();
                rb.linearVelocity = impulse;
                return;
            }

            if (drop)
            {
                ExitCling();
            }
        }

        private void ExitCling()
        {
            if (!isClinging)
                return;

            isClinging = false;
            rb.constraints = cachedConstraints;
            rb.gravityScale = cachedGravityScale;
            regrabUntil = Time.time + regrabCooldown;
        }

        private void OnDrawGizmosSelected()
        {
            int side = facing >= 0 ? 1 : -1;
            Vector2 dir = side > 0 ? Vector2.right : Vector2.left;

            Gizmos.color = Color.cyan;
            DrawProbe((Vector2)transform.position + new Vector2(0f, chestOffsetY), dir);
            DrawProbe((Vector2)transform.position + new Vector2(0f, waistOffsetY), dir);
        }

        private void DrawProbe(Vector2 origin, Vector2 dir)
        {
            Vector3 end = origin + dir * wallCheckDistance;
            Gizmos.DrawLine(origin, end);
            Gizmos.DrawWireCube(
                (Vector3)(origin + dir * wallCheckDistance * 0.5f),
                new Vector3(wallCheckDistance, wallCheckHeight, 0.1f));
        }
    }
}
