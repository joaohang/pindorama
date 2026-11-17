using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Comportamento de torreta: rajada -> cooldown -> rajada. Pode ser usada
    /// com qualquer EnemyType (amarelo, QTE azul ou QTE vermelho).
    /// </summary>
    [RequireComponent(typeof(EnemyBase))]
    public class TurretBehavior : MonoBehaviour
    {
        [SerializeField] private EnemyBase enemy;
        [SerializeField] private Transform firePoint;
        [SerializeField] private int burstCount = 3;
        [SerializeField] private float burstInterval = 0.15f;
        [SerializeField] private float cooldown = 1.4f;
        [SerializeField] private float projectileSpeed = 9f;

        private int shotsLeft;
        private float nextShot;

        private void Awake()
        {
            if (enemy == null)
                enemy = GetComponent<EnemyBase>();
            if (firePoint == null)
                firePoint = transform;

            shotsLeft = burstCount;
            nextShot = Time.time + 0.6f;
        }

        private void Update()
        {
            if (enemy == null || enemy.IsDead || enemy.IsInteracting)
                return;
            if (Time.time < nextShot)
                return;

            if (shotsLeft > 0)
            {
                Fire();
                shotsLeft--;
                nextShot = Time.time + burstInterval;

                if (shotsLeft == 0)
                    nextShot = Time.time + cooldown;
            }
            else
            {
                shotsLeft = burstCount;
                nextShot = Time.time + 0.2f;
            }
        }

        private void Fire()
        {
            if (enemy.Player == null)
                return;

            Vector2 origin = firePoint.position;
            Vector2 direction = ((Vector2)enemy.Player.transform.position - origin).normalized;
            EnemyProjectile.Spawn(origin, direction, projectileSpeed, enemy.Level);
        }
    }
}
