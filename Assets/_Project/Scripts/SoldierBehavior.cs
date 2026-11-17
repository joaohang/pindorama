using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Patrulha simples de soldado. Para quando está em interação com o gancho.
    /// </summary>
    [RequireComponent(typeof(EnemyBase))]
    public class SoldierBehavior : MonoBehaviour
    {
        [SerializeField] private EnemyBase enemy;
        [SerializeField] private float patrolRange = 2f;
        [SerializeField] private float speed = 1.2f;

        private Rigidbody2D rb;
        private Vector2 start;
        private int direction = 1;

        private void Awake()
        {
            if (enemy == null)
                enemy = GetComponent<EnemyBase>();
            rb = GetComponent<Rigidbody2D>();
            start = transform.position;
        }

        private void FixedUpdate()
        {
            if (enemy == null || enemy.IsDead || enemy.IsInteracting || rb == null)
                return;
            if (patrolRange <= 0.01f)
                return;

            Vector2 target = start + Vector2.right * (direction * patrolRange);
            rb.MovePosition(Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime));

            if (Vector2.Distance(rb.position, target) < 0.05f)
                direction *= -1;
        }
    }
}
