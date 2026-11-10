using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Reset da sala de teste: reposiciona o jogador, reseta os puxáveis e solta os ganchos.
    /// </summary>
    public class RoomController : MonoBehaviour
    {
        private PlayerController2D player;
        private DualHookController hooks;
        private Vector2 spawnPosition;
        private HookPullable[] pullables;
        private Vector2[] initialPositions;

        public void Configure(PlayerController2D p, DualHookController h, Vector2 spawn, HookPullable[] pulls)
        {
            player = p;
            hooks = h;
            spawnPosition = spawn;
            pullables = pulls;

            if (pullables != null)
            {
                initialPositions = new Vector2[pullables.Length];
                for (int i = 0; i < pullables.Length; i++)
                {
                    initialPositions[i] = pullables[i].transform.position;
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
                Reset();
        }

        public void Reset()
        {
            if (hooks != null)
                hooks.ReleaseAll(DetachReason.Reset);

            if (player != null)
                player.Teleport(spawnPosition);

            if (pullables != null)
            {
                for (int i = 0; i < pullables.Length; i++)
                {
                    HookPullable p = pullables[i];
                    if (p == null)
                        continue;
                    p.CancelPull();
                    Rigidbody2D rb = p.GetComponent<Rigidbody2D>();
                    if (rb != null)
                        rb.position = initialPositions[i];
                }
            }
        }
    }
}
