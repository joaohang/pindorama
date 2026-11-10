using System;
using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Marcação no chão (BoxCollider2D IsTrigger) para uma caixa.
    /// Aceita apenas a cor configurada, centraliza a caixa no SnapPoint e
    /// dispara WrongBoxEntered quando a caixa errada entra.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BoxSnapTarget : MonoBehaviour
    {
        [SerializeField] private HookColor acceptedColor = HookColor.Red;
        [SerializeField] private Transform snapPoint;
        [SerializeField] private float snapDuration = 0.18f;

        private BoxIdentity occupant;
        private bool hasWrongBox;

        public HookColor AcceptedColor { get { return acceptedColor; } }
        public BoxIdentity Occupant { get { return occupant; } }
        public bool IsOccupied { get { return occupant != null; } }
        public bool HasWrongBox { get { return hasWrongBox; } }

        public event Action<BoxIdentity> WrongBoxEntered;

        public void Configure(HookColor color, Transform point, float duration)
        {
            acceptedColor = color;
            snapPoint = point;
            snapDuration = duration;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Handle(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Handle(other);
        }

        private void Handle(Collider2D other)
        {
            if (occupant != null)
                return;

            BoxIdentity box = other.GetComponentInParent<BoxIdentity>();
            if (box == null || box.IsSnapped)
                return;

            if (box.Color != acceptedColor)
            {
                if (!hasWrongBox)
                {
                    hasWrongBox = true;
                    if (WrongBoxEntered != null)
                        WrongBoxEntered(box);
                }
                return;
            }

            occupant = box;
            Vector2 target = snapPoint != null
                ? (Vector2)snapPoint.position
                : (Vector2)transform.position;
            box.SnapTo(target, snapDuration);
        }

        public void Clear()
        {
            occupant = null;
            hasWrongBox = false;
        }
    }
}
