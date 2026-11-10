using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Uma mão/gancho (vermelho ou azul). Gerencia o estado e, ao prender:
    /// - Swing -> cria DistanceJoint2D no jogador (corda/pêndulo).
    /// - Fixed -> apenas registra o vínculo (cabo visual). NÃO trava o jogador;
    ///   o travamento só acontece quando os dois ganchos estão presos (Brace).
    /// Em Fixed, cada PrimaryCommand executa um novo puxão (nunca solta).
    /// </summary>
    public class HookHand : MonoBehaviour
    {
        [SerializeField] private HookColor handColor = HookColor.Red;
        [SerializeField] private Transform muzzle;
        [SerializeField] private HookSettings settings;
        [SerializeField] private PlayerController2D player;
        [SerializeField] private HookRopeView ropeView;

        private DistanceJoint2D joint;
        private HookAnchor anchor;
        private Rigidbody2D playerBody;
        private float nextFireTime;
        private HookPullable currentPullable;

        private bool reeling;
        private float reelTarget;
        private float ropeLength;

        private Vector2 visualAttachPoint;
        private bool hasVisualAttachPoint;

        public HookColor Color { get { return handColor; } }
        public HookHandState State { get; private set; }
        public HookAnchor Anchor { get { return anchor; } }
        public float AttachedAt { get; private set; }

        public Vector2 VisualAttachPoint
        {
            get
            {
                if (hasVisualAttachPoint)
                    return visualAttachPoint;
                return anchor != null ? anchor.AttachPoint : Vector2.zero;
            }
        }

        public bool IsAttached
        {
            get { return State == HookHandState.Attached || State == HookHandState.Pulling; }
        }

        public bool IsPullable
        {
            get { return IsAttached && anchor != null && anchor.Motion == HookAnchorMotion.Fixed; }
        }

        public bool IsSwing
        {
            get { return IsAttached && anchor != null && anchor.Motion == HookAnchorMotion.Swing; }
        }

        private void Awake()
        {
            State = HookHandState.Idle;
        }

        private void Update()
        {
            if (State == HookHandState.Pulling && (currentPullable == null || !currentPullable.IsPulling))
            {
                State = HookHandState.Attached;
                currentPullable = null;
            }

            // Cabo de puxável não tem joint; se o jogador se afastar demais,
            // solta para o cabo não esticar infinitamente.
            if (IsPullable && settings != null)
            {
                float distance = Vector2.Distance(player.transform.position, anchor.AttachPoint);
                if (distance > settings.maxRopeLength)
                    ForceDetach(DetachReason.AnchorLost);
            }
        }

        public void Configure(HookColor color, Transform muzzleTransform, HookSettings s, PlayerController2D p, HookRopeView view)
        {
            handColor = color;
            muzzle = muzzleTransform;
            settings = s;
            player = p;
            ropeView = view;
            playerBody = p != null ? p.GetComponent<Rigidbody2D>() : null;
        }

        private void FixedUpdate()
        {
            if (joint == null || anchor == null || playerBody == null || settings == null)
                return;

            // Puxada inicial com limite: recolhe um trecho da corda ao prender.
            if (reeling)
            {
                joint.distance = Mathf.MoveTowards(
                    joint.distance,
                    reelTarget,
                    settings.attachReelSpeed * Time.fixedDeltaTime);

                if (Mathf.Abs(joint.distance - reelTarget) < 0.01f)
                    reeling = false;
            }

            // Limite de esticamento: remove a velocidade que aponta para fora
            // da corda. O collider do jogador continua resolvendo o chão.
            Vector2 delta = (Vector2)player.transform.position - anchor.AttachPoint;
            float distance = delta.magnitude;
            if (distance > 0.001f && distance > joint.distance * settings.maxStretch)
            {
                Vector2 normal = delta / distance;
                Vector2 velocity = playerBody.GetVelocity();
                float outward = Vector2.Dot(velocity, normal);
                if (outward > 0f)
                    playerBody.SetVelocity(velocity - normal * outward);
            }
        }

        public bool TryFire(Vector2 origin, Vector2 direction, Collider2D playerCollider)
        {
            if (State != HookHandState.Idle && State != HookHandState.Failed)
                return false;
            if (Time.time < nextFireTime)
                return false;
            if (direction.sqrMagnitude < 0.001f)
            {
                Fail();
                return false;
            }

            Vector2 impactPoint;
            HookAnchor hit = Cast(origin, direction, playerCollider, out impactPoint);
            if (hit == null)
            {
                Fail();
                return false;
            }

            Attach(hit, impactPoint);
            return true;
        }

        private HookAnchor Cast(Vector2 origin, Vector2 direction, Collider2D playerCollider, out Vector2 impactPoint)
        {
            impactPoint = Vector2.zero;
            RaycastHit2D[] buffer = new RaycastHit2D[16];
            int n = Physics2D.RaycastNonAlloc(origin, direction, buffer, settings.range, settings.hitMask);

            for (int i = 0; i < n; i++)
            {
                Collider2D c = buffer[i].collider;
                if (c == null || c == playerCollider)
                    continue;

                HookAnchor a = c.GetComponent<HookAnchor>();
                if (a != null && a.Accepts(handColor))
                {
                    impactPoint = buffer[i].point;
                    return a;
                }

                // Primeiro hit que não é o jogador decide: parede bloqueia; âncora errada bloqueia.
                return null;
            }

            return null;
        }

        private void Attach(HookAnchor a, Vector2 impactPoint)
        {
            anchor = a;
            State = HookHandState.Attached;
            AttachedAt = Time.time;
            visualAttachPoint = impactPoint;
            hasVisualAttachPoint = true;

            HookImpactVfx.Play(impactPoint, HookImpactVfx.ColorFor(handColor));

            if (a.Motion == HookAnchorMotion.Swing)
            {
                CreateJoint(a);
            }
            // Fixed: não cria joint nem trava o jogador. O brace é decidido
            // pelo DualHookController quando os DOIS ganchos estão presos.
        }

        private void CreateJoint(HookAnchor a)
        {
            joint = player.gameObject.AddComponent<DistanceJoint2D>();
            joint.autoConfigureDistance = false;
            joint.maxDistanceOnly = true;
            joint.enableCollision = false;
            joint.breakForce = Mathf.Infinity;
            joint.breakTorque = Mathf.Infinity;
            joint.anchor = Vector2.zero;

            Rigidbody2D body = a.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                joint.connectedBody = body;
                joint.connectedAnchor = a.transform.InverseTransformPoint(a.AttachPoint);
            }
            else
            {
                joint.connectedBody = null;
                joint.connectedAnchor = a.AttachPoint;
            }

            float dist = Vector2.Distance(player.transform.position, a.AttachPoint);
            joint.distance = Mathf.Clamp(dist, settings.minRopeLength, settings.maxRopeLength);
            ropeLength = joint.distance;

            // Impulso inicial + puxada limitada (reel) na direção da âncora.
            reeling = true;
            reelTarget = Mathf.Max(settings.minRopeLength, ropeLength - settings.attachReelDistance);

            if (settings.attachImpulse > 0f && playerBody != null)
            {
                Vector2 dir = (a.AttachPoint - (Vector2)player.transform.position).normalized;
                playerBody.AddForce(dir * settings.attachImpulse, ForceMode2D.Impulse);
            }
        }

        public void PrimaryCommand()
        {
            if (!IsAttached || anchor == null)
                return;

            if (anchor.Motion == HookAnchorMotion.Swing)
            {
                ForceDetach(DetachReason.Command);
                return;
            }

            // Fixed: puxão repetível.
            if (State == HookHandState.Pulling)
                return;

            HookPullable pullable = anchor.GetComponentInParent<HookPullable>();
            if (pullable == null)
                return;

            if (pullable.TryBeginPull(player))
            {
                State = HookHandState.Pulling;
                currentPullable = pullable;
            }
        }

        public void ForceDetach(DetachReason reason)
        {
            if (joint != null)
            {
                Destroy(joint);
                joint = null;
            }

            reeling = false;
            currentPullable = null;
            anchor = null;
            hasVisualAttachPoint = false;
            State = HookHandState.Idle;
        }

        private void Fail()
        {
            State = HookHandState.Failed;
            nextFireTime = Time.time + settings.failCooldown;
            Invoke(nameof(ResetFailed), settings.failCooldown);
        }

        private void ResetFailed()
        {
            if (State == HookHandState.Failed)
                State = HookHandState.Idle;
        }
    }
}
