using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Base dos inimigos do EnemyLab. O tipo define a interação com o gancho
    /// (amarelo/QTE azul/QTE vermelho); o arquétipo define o corpo (soldado/turreta).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class EnemyBase : MonoBehaviour, IHookReceiver
    {
        [Header("Tipo / corpo")]
        [SerializeField] private EnemyType type = EnemyType.Yellow;
        [SerializeField] private EnemyArchetype archetype = EnemyArchetype.Soldier;

        [Header("Puxão do gancho")]
        [SerializeField] private float pullSpeed = 9f;
        [SerializeField] private float pullAcceleration = 35f;
        [SerializeField] private float stopDistance = 1.4f;

        [Header("Amarelo (estilingue)")]
        [SerializeField] private float yellowSlowScale = 0.25f;
        [SerializeField] private float launchSpeed = 20f;

        [Header("QTE azul/vermelho")]
        [SerializeField] private float qteWindow = 1f;
        [SerializeField] private float qteKnockback = 12f;

        [Header("Referências")]
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private Collider2D bodyCollider;

        private Rigidbody2D rb;
        private LevelController level;
        private DualHookController hooks;
        private PlayerController2D player;
        private Rigidbody2D playerBody;
        private HookHand hand;

        private EnemyState state = EnemyState.Idle;
        private float qteEnd;
        private float yellowPreviousTimeScale = 1f;
        private float nextInteractTime;
        private bool endingInternally;

        public EnemyType Type { get { return type; } }
        public EnemyArchetype Archetype { get { return archetype; } }
        public EnemyState State { get { return state; } }
        public bool IsDead { get { return state == EnemyState.Dead; } }
        public bool IsQteActive { get { return state == EnemyState.QteWindow; } }
        public bool IsYellowStuck { get { return state == EnemyState.YellowStuck; } }
        public bool IsInteracting
        {
            get
            {
                return state == EnemyState.Pulling ||
                       state == EnemyState.YellowStuck ||
                       state == EnemyState.QteWindow;
            }
        }

        public LevelController Level { get { return level; } }
        public PlayerController2D Player { get { return player; } }
        public DualHookController Hooks { get { return hooks; } }

        public string Prompt
        {
            get
            {
                if (state == EnemyState.QteWindow)
                {
                    if (type == EnemyType.BlueQte)
                        return "QTE AZUL: aperte o botão DIREITO (RMB)!";
                    return "QTE VERMELHO: aperte o botão ESQUERDO (LMB)!";
                }

                if (state == EnemyState.YellowStuck)
                    return "AMARELO: mantenha o botão, mire e solte para lançar!";

                return string.Empty;
            }
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                rb.freezeRotation = true;
            }

            if (sprite == null)
                sprite = GetComponent<SpriteRenderer>();
            if (bodyCollider == null)
                bodyCollider = GetComponent<Collider2D>();

            level = FindAnyObjectByType<LevelController>();
            player = FindAnyObjectByType<PlayerController2D>();
            hooks = FindAnyObjectByType<DualHookController>();

            if (player != null)
                playerBody = player.GetComponent<Rigidbody2D>();
        }

        public void Configure(EnemyType enemyType, EnemyArchetype enemyArchetype)
        {
            type = enemyType;
            archetype = enemyArchetype;
        }

        public void OnHookAttached(HookHand hookHand, Vector2 hitPoint)
        {
            if (state == EnemyState.Dead || state == EnemyState.Pulling ||
                state == EnemyState.YellowStuck || state == EnemyState.QteWindow)
                return;
            if (Time.time < nextInteractTime)
                return;

            hand = hookHand;
            player = hookHand != null ? hookHand.PlayerController : null;
            playerBody = hookHand != null ? hookHand.PlayerBody : null;
            hooks = player != null ? player.GetComponent<DualHookController>() : null;

            if (hooks == null)
                hooks = FindAnyObjectByType<DualHookController>();

            if (player != null)
                player.BeginEnemyPull();

            state = EnemyState.Pulling;
        }

        public void OnHookDetached(HookHand hookHand, DetachReason reason)
        {
            if (endingInternally || state == EnemyState.Dead)
                return;

            if (state == EnemyState.Pulling)
            {
                if (player != null)
                    player.EndEnemyPull();
                state = EnemyState.Idle;
                hand = null;
                return;
            }

            if (state == EnemyState.YellowStuck || state == EnemyState.QteWindow)
                CancelInteraction();
        }

        private void FixedUpdate()
        {
            if (state != EnemyState.Pulling)
                return;

            if (hand == null || !hand.IsAttached || player == null || playerBody == null)
            {
                if (player != null)
                    player.EndEnemyPull();
                state = EnemyState.Idle;
                hand = null;
                return;
            }

            float distance = Vector2.Distance(player.transform.position, transform.position);
            if (distance <= stopDistance)
            {
                BeginStick();
                return;
            }

            Vector2 dir = ((Vector2)transform.position - (Vector2)player.transform.position).normalized;
            Vector2 velocity = playerBody.linearVelocity;
            playerBody.linearVelocity = Vector2.MoveTowards(
                velocity,
                dir * pullSpeed,
                pullAcceleration * Time.fixedDeltaTime);
        }

        private void BeginStick()
        {
            if (player == null || hand == null)
            {
                state = EnemyState.Idle;
                return;
            }

            player.EndEnemyPull();
            player.BeginEnemyInteraction();
            if (hooks != null)
                hooks.SetInputSuspended(true);

            if (type == EnemyType.Yellow)
            {
                yellowPreviousTimeScale = Time.timeScale;
                Time.timeScale = yellowSlowScale;
                state = EnemyState.YellowStuck;
            }
            else
            {
                qteEnd = Time.unscaledTime + qteWindow;
                state = EnemyState.QteWindow;
            }
        }

        private void Update()
        {
            if (state == EnemyState.YellowStuck)
            {
                if (hand == null || !hand.IsAttached)
                {
                    CancelInteraction();
                    return;
                }

                if (!HandHeld(hand))
                    LaunchYellow();

                return;
            }

            if (state == EnemyState.QteWindow)
            {
                if (hand == null || !hand.IsAttached)
                {
                    CancelInteraction();
                    return;
                }

                bool redDown = Input.GetMouseButtonDown(0) ||
                               (Input.GetJoystickNames().Length > 0 && Input.GetKeyDown(KeyCode.JoystickButton4));
                bool blueDown = Input.GetMouseButtonDown(1) ||
                                (Input.GetJoystickNames().Length > 0 && Input.GetKeyDown(KeyCode.JoystickButton5));

                if (redDown || blueDown)
                {
                    bool correct = (type == EnemyType.RedQte && redDown) ||
                                   (type == EnemyType.BlueQte && blueDown);
                    if (correct)
                        QteSuccess();
                    else
                        QteFail();
                    return;
                }

                if (Time.unscaledTime >= qteEnd)
                    QteFail();
            }
        }

        private bool HandHeld(HookHand hookHand)
        {
            if (hookHand == null)
                return false;

            if (hookHand.Color == HookColor.Red)
            {
                return Input.GetMouseButton(0) ||
                       (Input.GetJoystickNames().Length > 0 && Input.GetKey(KeyCode.JoystickButton4));
            }

            return Input.GetMouseButton(1) ||
                   (Input.GetJoystickNames().Length > 0 && Input.GetKey(KeyCode.JoystickButton5));
        }

        private void LaunchYellow()
        {
            Vector2 direction = hooks != null ? hooks.AimDirection : Vector2.right;

            endingInternally = true;
            state = EnemyState.Dead;
            Time.timeScale = 1f;

            if (hooks != null)
                hooks.SetInputSuspended(false);

            if (hand != null)
                hand.ForceDetach(DetachReason.Command);

            if (player != null)
            {
                player.EndEnemyInteraction();
                if (playerBody != null)
                    playerBody.linearVelocity = direction * launchSpeed;
            }

            KillEnemy();
            endingInternally = false;
        }

        private void QteSuccess()
        {
            endingInternally = true;
            state = EnemyState.Dead;

            if (hooks != null)
                hooks.SetInputSuspended(false);

            if (hand != null)
                hand.ForceDetach(DetachReason.Command);

            if (player != null)
                player.EndEnemyInteraction();

            KillEnemy();
            endingInternally = false;
        }

        private void QteFail()
        {
            endingInternally = true;
            state = EnemyState.Idle;
            nextInteractTime = Time.time + 0.7f;

            if (hooks != null)
                hooks.SetInputSuspended(false);

            if (hand != null)
                hand.ForceDetach(DetachReason.Command);

            if (player != null && playerBody != null)
            {
                Vector2 away = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
                player.EndEnemyInteraction();
                playerBody.linearVelocity = away * qteKnockback;
            }
            else if (player != null)
            {
                player.EndEnemyInteraction();
            }

            hand = null;
            endingInternally = false;
        }

        private void CancelInteraction()
        {
            if (state == EnemyState.Dead)
                return;

            endingInternally = true;

            if (state == EnemyState.YellowStuck)
                Time.timeScale = 1f;

            if (hooks != null)
                hooks.SetInputSuspended(false);

            if (player != null && player.IsInEnemyInteraction)
                player.EndEnemyInteraction();
            if (player != null && player.IsBeingPulledByEnemy)
                player.EndEnemyPull();

            state = EnemyState.Idle;
            hand = null;
            endingInternally = false;
        }

        private void KillEnemy()
        {
            state = EnemyState.Dead;

            if (bodyCollider != null)
                bodyCollider.enabled = false;
            if (sprite != null)
                sprite.enabled = false;

            BoxExplosionVfx.Play(transform.position, HookColorFor(type));
            Destroy(gameObject, 0.2f);
        }

        private static HookColor HookColorFor(EnemyType enemyType)
        {
            switch (enemyType)
            {
                case EnemyType.BlueQte:
                    return HookColor.Blue;
                case EnemyType.RedQte:
                    return HookColor.Red;
                default:
                    return HookColor.Neutral;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (state == EnemyState.Dead || IsInteracting)
                return;

            if (collision.collider.GetComponentInParent<PlayerController2D>() != null && level != null)
                level.RespawnPlayer();
        }

        private void OnDisable()
        {
            if (state == EnemyState.YellowStuck)
                Time.timeScale = 1f;

            if (hooks != null)
                hooks.SetInputSuspended(false);

            if (player != null && player.IsInEnemyInteraction)
                player.EndEnemyInteraction();
            if (player != null && player.IsBeingPulledByEnemy)
                player.EndEnemyPull();
        }
    }
}
