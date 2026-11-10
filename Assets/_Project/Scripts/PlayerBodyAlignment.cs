using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Mantém o CapsuleCollider2D do jogador ajustado ao SpriteRenderer visual.
    /// Roda também no editor ([ExecuteAlways]): se você mover/redimensionar a
    /// imagem, o collider acompanha tamanho e centro automaticamente.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class PlayerBodyAlignment : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private CapsuleCollider2D bodyCollider;
        [SerializeField] private bool fitSize = true;
        [SerializeField] private bool fitOffset = true;
        [SerializeField] private bool lockVisualRotation = true;
        [SerializeField] private Vector2 padding = Vector2.zero;

        private void OnValidate()
        {
            if (!Application.isPlaying)
                Fit();
        }

        private void Awake()
        {
            Fit();
        }

        private void Update()
        {
            // No editor mantém tudo alinhado enquanto você arrasta a imagem.
            if (!Application.isPlaying)
                Fit();
        }

        public void Configure(SpriteRenderer visualRenderer, CapsuleCollider2D capsule)
        {
            visual = visualRenderer;
            bodyCollider = capsule;
            Fit();
        }

        [ContextMenu("Ajustar collider à imagem")]
        public void Fit()
        {
            if (visual == null)
                visual = GetComponentInChildren<SpriteRenderer>();
            if (bodyCollider == null)
                bodyCollider = GetComponent<CapsuleCollider2D>();

            if (visual == null || bodyCollider == null || visual.sprite == null)
                return;

            if (lockVisualRotation && visual.transform.localRotation != Quaternion.identity)
                visual.transform.localRotation = Quaternion.identity;

            Bounds bounds = visual.bounds;
            Vector3 scale = transform.lossyScale;
            float sx = Mathf.Max(0.0001f, Mathf.Abs(scale.x));
            float sy = Mathf.Max(0.0001f, Mathf.Abs(scale.y));

            if (fitSize)
            {
                Vector2 size = new Vector2(
                    bounds.size.x / sx + padding.x,
                    bounds.size.y / sy + padding.y);

                CapsuleDirection2D direction = size.y >= size.x
                    ? CapsuleDirection2D.Vertical
                    : CapsuleDirection2D.Horizontal;

                if ((bodyCollider.size - size).sqrMagnitude > 0.000001f ||
                    bodyCollider.direction != direction)
                {
                    bodyCollider.size = size;
                    bodyCollider.direction = direction;
                }
            }

            if (fitOffset)
            {
                Vector3 localCenter = transform.InverseTransformPoint(bounds.center);
                Vector2 offset = new Vector2(localCenter.x, localCenter.y);

                if ((bodyCollider.offset - offset).sqrMagnitude > 0.000001f)
                    bodyCollider.offset = offset;
            }
        }
    }
}
