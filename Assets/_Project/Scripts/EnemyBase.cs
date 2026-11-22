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
                    bool hookIsRed = hand != null && hand.Color == HookColor.Red;

                    if (type == EnemyType.BlueQte)
                    {
                        return hookIsRed
                            ? "QTE AZUL: aperte RMB!"
                            : "QTE AZUL: aperte LMB (ou solte RMB)!";
                    }

                    return hookIsRed
                        ? "QTE VERMELHO: aperte RMB (ou solte LMB)!"
                        : "QTE VERMELHO: aperte LMB!";
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

            // O gancho só puxa se a cor da mão casar com a cor do inimigo.
            HookAnchor hookAnchor = GetComponentInChildren<HookAnchor>();
            if (hookAnchor != null)
                hookAnchor.Configure(HookColorFor(type), hookAnchor.Motion);

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

            HookAnchor hookAnchor = GetComponentInChildren<HookAnchor>();
            if (hookAnchor != null)
                hookAnchor.Configure(HookColorFor(type), hookAnchor.Motion);
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
                TimeScaleService.Push("Yellow", yellowSlowScale);
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

                bool redDown = InputReader.RedHookDown;
                bool blueDown = InputReader.BlueHookDown;
                bool redUp = InputReader.RedHookUp;
                bool blueUp = InputReader.BlueHookUp;

                bool hookIsRed = hand.Color == HookColor.Red;
                bool hookMatchesColor = (type == EnemyType.RedQte && hookIsRed) ||
                                        (type == EnemyType.BlueQte && !hookIsRed);

                // Aperta o botão da cor OU, se o gancho já está na mão dessa cor,
                // aperta o outro botão / solta o botão preso.
                bool enemyColorDown = (type == EnemyType.RedQte && redDown) ||
                                      (type == EnemyType.BlueQte && blueDown);
                bool oppositeDown = (type == EnemyType.RedQte && hookIsRed && blueDown) ||
                                    (type == EnemyType.BlueQte && !hookIsRed && redDown);
                bool hookReleased = hookMatchesColor && (hookIsRed ? redUp : blueUp);

                if (redDown || blueDown || redUp || blueUp)
                {
                    if (enemyColorDown || oppositeDown || hookReleased)
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

            return hookHand.Color == HookColor.Red
                ? InputReader.RedHookHeld
                : InputReader.BlueHookHeld;
        }

        private void LaunchYellow()
        {
            Vector2 direction = hooks != null ? hooks.AimDirection : Vector2.right;

            endingInternally = true;
            state = EnemyState.Dead;
            TimeScaleService.Pop("Yellow");

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

            if (player != null)
                player.EndEnemyInteraction();

            PlayerDamageController damage =
                player != null ? player.GetComponent<PlayerDamageController>() : null;
            if (damage != null)
            {
                damage.ApplyDamage(new DamageInfo(
                    DamageType.NonLethal, transform.position, qteKnockback, gameObject));
            }
            else if (player != null && playerBody != null)
            {
                Vector2 away = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
                playerBody.linearVelocity = away * qteKnockback;
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
                TimeScaleService.Pop("Yellow");

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

            PlayerDamageController damage =
                collision.collider.GetComponentInParent<PlayerDamageController>();
            if (damage != null)
            {
                damage.ApplyDamage(new DamageInfo(
                    DamageType.NonLethal, transform.position, 12f, gameObject));
            }
            else if (level != null)
            {
                level.RespawnPlayer();
            }
        }

        public void ForceCancelInteraction()
        {
            if (state == EnemyState.Dead)
                return;

            if (state == EnemyState.Pulling)
            {
                if (player != null)
                    player.EndEnemyPull();
                state = EnemyState.Idle;
                hand = null;
                return;
            }

            CancelInteraction();
        }

        private void OnDisable()
        {
            if (state == EnemyState.YellowStuck)
                TimeScaleService.Pop("Yellow");

            if (hooks != null)
                hooks.SetInputSuspended(false);

            if (player != null && player.IsInEnemyInteraction)
                player.EndEnemyInteraction();
            if (player != null && player.IsBeingPulledByEnemy)
                player.EndEnemyPull();
        }
    }
}
