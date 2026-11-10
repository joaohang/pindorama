using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Identidade de uma caixa de puzzle: cor, spawn e estado de encaixe.
    /// Funciona com HookPullable (translação) e trava rotação no eixo Z.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class BoxIdentity : MonoBehaviour
    {
        [SerializeField] private HookColor color = HookColor.Red;
        [SerializeField] private string label = "BOX";
        [SerializeField] private Vector2 spawnPosition;
        [SerializeField] private float snapDuration = 0.18f;

        private Rigidbody2D rb;
        private HookPullable pullable;
        private Collider2D[] colliders;

        private Vector2 snapStart;
        private Vector2 snapTarget;
        private float snapElapsed;
        private bool snapping;
        private bool snapped;

        public HookColor Color { get { return color; } }
        public string Label { get { return label; } }
        public bool IsSnapped { get { return snapped; } }
        public Vector2 SpawnPosition { get { return spawnPosition; } }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            pullable = GetComponent<HookPullable>();
            colliders = GetComponents<Collider2D>();

            if (spawnPosition == Vector2.zero)
                spawnPosition = transform.position;

            if (rb != null)
                rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
        }

        public void Configure(HookColor boxColor, string boxLabel)
        {
            color = boxColor;
            label = boxLabel;
            spawnPosition = transform.position;
        }

        public void SnapTo(Vector2 target, float duration)
        {
            if (snapped)
                return;

            snapped = true;
            snapping = true;
            snapStart = rb != null ? rb.position : (Vector2)transform.position;
            snapTarget = target;
            snapElapsed = 0f;
            snapDuration = Mathf.Max(0.01f, duration);

            if (pullable != null)
                pullable.SetLocked(true);

            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
            }

            // Depois de encaixada, a caixa não interage mais com o mundo.
            if (colliders != null)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i] != null)
                        colliders[i].enabled = false;
                }
            }
        }

        public void ResetBox()
        {
            snapping = false;
            snapped = false;

            if (pullable != null)
                pullable.SetLocked(false);

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.velocity = Vector2.zero;
                rb.position = spawnPosition;
                rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
            }

            transform.position = new Vector3(spawnPosition.x, spawnPosition.y, 0f);
            transform.rotation = Quaternion.identity;

            if (colliders != null)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i] != null)
                        colliders[i].enabled = true;
                }
            }
        }

        public void ExplodeAndReset()
        {
            BoxExplosionVfx.Play(transform.position, color);
            ResetBox();
        }

        private void FixedUpdate()
        {
            if (!snapping || rb == null)
                return;

            snapElapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(snapElapsed / snapDuration);
            rb.MovePosition(Vector2.Lerp(snapStart, snapTarget, t));

            if (t >= 1f)
                snapping = false;
        }
    }
}
