using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Zona letal genérica (espinhos, queda, etc.). Respawna o jogador no
    /// spawn do LevelController sem resetar as caixas.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LethalHazard : MonoBehaviour
    {
        [SerializeField] private LevelController level;
        [SerializeField] private float hitCooldown = 0.2f;

        private float nextHitTime;

        public void Configure(LevelController levelController)
        {
            level = levelController;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Hit(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Hit(other);
        }

        private void Hit(Collider2D other)
        {
            if (Time.time < nextHitTime)
                return;
            if (other.GetComponentInParent<PlayerController2D>() == null)
                return;

            nextHitTime = Time.time + hitCooldown;
            if (level != null)
                level.RespawnPlayer();
        }
    }
}
