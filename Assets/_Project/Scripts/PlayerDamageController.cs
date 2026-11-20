using UnityEngine;

namespace TwinHook
{
    public enum PlayerDamageState
    {
        Normal,
        HitStun,
        Recovering,
        Dead,
        Respawning
    }

    /// <summary>
    /// Dano híbrido: perigos letais matam; dano não-letal gera hit, knockback,
    /// i-frames e um contador de falhas. Também centraliza o reset de ganchos,
    /// parede, interação com inimigo, física e timeScale.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerDamageController : MonoBehaviour
    {
        [Header("Falhas")]
        [SerializeField] private int maxFailures = 3;
        [SerializeField] private float recoveryTime = 2.5f;
        [SerializeField] private float safeGroundRecovery = 0.35f;

        [Header("Hit")]
        [SerializeField] private float hitStunDuration = 0.18f;
        [SerializeField] private float invulnerabilityDuration = 0.9f;
        [SerializeField] private float knockbackForce = 12f;
        [SerializeField] private float hitSlowScale = 0.35f;
        [SerializeField] private float hitSlowDuration = 0.08f;
        [SerializeField] private float hitFlashDuration = 0.12f;

        [Header("Morte / respawn")]
        [SerializeField] private float deathDelay = 0.6f;
        [SerializeField] private float spawnInvulnerability = 1f;

        private PlayerController2D player;
        private Rigidbody2D body;
        private DualHookController hooks;
        private WallClingController wall;
        private LevelController level;
        private RoomController room;

        private PlayerDamageState state = PlayerDamageState.Normal;
        private int failures;
        private float lastHitTime = -999f;
        private float hitStunUntil;
        private float invulnerableUntil;
        private float safeGroundSince;
        private float deadAt;

        public PlayerDamageState State { get { return state; } }
        public int Failures { get { return failures; } }
        public int MaxFailures { get { return maxFailures; } }
        public float FlashUntil { get; private set; }
        public bool IsDead
        {
            get { return state == PlayerDamageState.Dead || state == PlayerDamageState.Respawning; }
        }

        private void Awake()
        {
            player = GetComponent<PlayerController2D>();
            body = GetComponent<Rigidbody2D>();
            hooks = GetComponent<DualHookController>();
            wall = GetComponent<WallClingController>();
            FindWorld();
        }

        private void Start()
        {
            if (GetComponent<PlayerDamageHud>() == null)
                gameObject.AddComponent<PlayerDamageHud>();

            // Garante que uma cena recarregada não comece com timeScale preso.
            TimeScaleService.ClearAll();
        }

        private void Update()
        {
            EnsureRefs();
            TimeScaleService.Tick();

            if (state == PlayerDamageState.Dead)
            {
                if (Time.unscaledTime >= deadAt)
                    Respawn();
                return;
            }

            if (state == PlayerDamageState.HitStun)
            {
                if (Time.unscaledTime >= hitStunUntil)
                {
                    state = PlayerDamageState.Recovering;
                    if (player != null)
                        player.SetControlEnabled(true);
                    if (hooks != null)
                        hooks.SetInputSuspended(false);
                }
                return;
            }

            if (state == PlayerDamageState.Recovering)
            {
                if (player != null && player.IsGrounded)
                    safeGroundSince += Time.unscaledDeltaTime;
                else
                    safeGroundSince = 0f;

                bool byTime = Time.unscaledTime - lastHitTime >= recoveryTime;
                bool byGround = safeGroundSince >= safeGroundRecovery;
                if (byTime || byGround)
                    ResetFailures();
            }
        }

        public void ApplyDamage(DamageInfo info)
        {
            EnsureRefs();

            if (state == PlayerDamageState.Dead || state == PlayerDamageState.Respawning)
                return;

            if (info.Type == DamageType.Lethal)
            {
                Die();
                return;
            }

            if (Time.unscaledTime < invulnerableUntil || state == PlayerDamageState.HitStun)
                return;

            int next = failures + 1;
            if (next >= maxFailures)
            {
                failures = maxFailures;
                Die();
                return;
            }

            failures = next;
            EnterHit(info);
        }

        private void EnterHit(DamageInfo info)
        {
            FindWorld();

            float now = Time.unscaledTime;
            state = PlayerDamageState.HitStun;
            hitStunUntil = now + hitStunDuration;
            invulnerableUntil = now + invulnerabilityDuration;
            lastHitTime = now;
            safeGroundSince = 0f;
            FlashUntil = now + hitFlashDuration;

            if (player != null)
                player.SetControlEnabled(false);

            CancelSystems();

            if (body != null && player != null)
            {
                body.constraints = RigidbodyConstraints2D.FreezeRotation;
                body.gravityScale = player.NormalGravityScale;

                Vector2 dir = (Vector2)player.transform.position - info.SourcePoint;
                if (dir.sqrMagnitude < 0.01f)
                    dir = -body.linearVelocity;
                if (dir.sqrMagnitude < 0.01f)
                    dir = Vector2.left;

                float force = info.Knockback > 0.01f ? info.Knockback : knockbackForce;
                body.linearVelocity = dir.normalized * Mathf.Max(4f, force);
            }

            TimeScaleService.Push("Hit", hitSlowScale, hitSlowDuration);
        }

        private void Die()
        {
            if (state == PlayerDamageState.Dead || state == PlayerDamageState.Respawning)
                return;

            state = PlayerDamageState.Dead;
            deadAt = Time.unscaledTime + deathDelay;
            FlashUntil = Time.unscaledTime + 0.2f;

            TimeScaleService.ClearAll();
            CancelSystems();

            if (player != null)
                player.SetControlEnabled(false);
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.gravityScale = 0f;
            }
        }

        private void Respawn()
        {
            state = PlayerDamageState.Respawning;
            TimeScaleService.ClearAll();
            CancelSystems();

            if (player != null)
                player.SetControlEnabled(false);

            if (level != null)
                level.RespawnPlayer();
            else if (room != null)
                room.Reset();
            else if (player != null)
                player.Teleport(player.transform.position);

            ResetAfterRespawn(true);
        }

        private void CancelSystems()
        {
            EnsureRefs();

            if (hooks != null)
            {
                hooks.SetInputSuspended(true);
                hooks.ReleaseAll(DetachReason.Hit);
            }

            if (wall != null)
                wall.ForceExit();

            EnemyBase[] enemies = FindObjectsByType<EnemyBase>();
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null)
                    enemies[i].ForceCancelInteraction();
            }

            if (player != null && player.IsInEnemyInteraction)
                player.EndEnemyInteraction();

            // O cancelamento do inimigo libera o input; o hit precisa manter
            // os ganchos suspensos até o fim do hitstun.
            if (hooks != null)
                hooks.SetInputSuspended(true);
        }

        private void ResetFailures()
        {
            failures = 0;
            safeGroundSince = 0f;
            state = PlayerDamageState.Normal;

            if (player != null)
                player.SetControlEnabled(true);
        }

        public void ResetAfterRespawn(bool grantSpawnProtection)
        {
            EnsureRefs();
            FindWorld();
            TimeScaleService.ClearAll();
            failures = 0;
            safeGroundSince = 0f;
            lastHitTime = -999f;
            state = PlayerDamageState.Normal;
            invulnerableUntil = grantSpawnProtection
                ? Time.unscaledTime + spawnInvulnerability
                : Time.unscaledTime;

            if (hooks != null)
            {
                hooks.SetInputSuspended(false);
                hooks.ResetAirResources();
            }

            if (wall != null)
                wall.ForceExit();

            if (player != null)
                player.SetControlEnabled(true);

            if (body != null && player != null)
            {
                body.constraints = RigidbodyConstraints2D.FreezeRotation;
                body.gravityScale = player.NormalGravityScale;
                body.linearVelocity = Vector2.zero;
            }
        }

        private void EnsureRefs()
        {
            if (player == null)
                player = GetComponent<PlayerController2D>();
            if (body == null)
                body = GetComponent<Rigidbody2D>();
            if (hooks == null)
                hooks = GetComponent<DualHookController>();
            if (wall == null)
                wall = GetComponent<WallClingController>();
            FindWorld();
        }

        private void FindWorld()
        {
            if (level == null)
                level = FindAnyObjectByType<LevelController>();
            if (room == null)
                room = FindAnyObjectByType<RoomController>();
        }

        private void OnDestroy()
        {
            TimeScaleService.ClearAll();
        }
    }
}
