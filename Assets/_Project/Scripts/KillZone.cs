using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Zona de morte/queda: chama o reset da sala quando o jogador cai.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class KillZone : MonoBehaviour
    {
        private RoomController room;

        public void Configure(RoomController controller)
        {
            room = controller;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController2D>() != null)
            {
                if (room != null)
                    room.Reset();
            }
        }
    }
}
