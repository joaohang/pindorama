using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Move um Rigidbody2D kinematic entre dois pontos, em velocidade fixa e
    /// vaivém. Colocado na Bola Amarela, permite que o gancho se prenda e o
    /// jogador seja transportado junto (o DistanceJoint2D usa o Rigidbody2D
    /// da bola como connectedBody).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingAnchor : MonoBehaviour
    {
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField] private float speed = 2f;
        [SerializeField] private bool startAtA = true;

        private Rigidbody2D rb;
        private bool movingToB;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
            }

            movingToB = startAtA;
        }

        public void Configure(Transform a, Transform b, float moveSpeed)
        {
            pointA = a;
            pointB = b;
            speed = moveSpeed;
        }

        private void FixedUpdate()
        {
            if (rb == null || pointA == null || pointB == null)
                return;

            Vector2 target = movingToB ? (Vector2)pointB.position : (Vector2)pointA.position;
            Vector2 next = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
            rb.MovePosition(next);

            if (Vector2.Distance(next, target) < 0.01f)
                movingToB = !movingToB;
        }
    }
}
