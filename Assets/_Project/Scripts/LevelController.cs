using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Controla spawn/respawn do jogador na fase e reset manual (R).
    /// </summary>
    public class LevelController : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private PlayerController2D player;
        [SerializeField] private DualHookController hooks;
        [SerializeField] private BoxSyncPuzzle puzzle;

        public Transform SpawnPoint { get { return spawnPoint; } }

        public void Configure(Transform spawn, PlayerController2D playerController,
                              DualHookController hookController, BoxSyncPuzzle boxPuzzle)
        {
            spawnPoint = spawn;
            player = playerController;
            hooks = hookController;
            puzzle = boxPuzzle;
        }

        public void RespawnPlayer()
        {
            if (hooks != null)
                hooks.ReleaseAll(DetachReason.Reset);

            if (player != null && spawnPoint != null)
                player.Teleport(spawnPoint.position);
        }

        public void ResetLevel()
        {
            RespawnPlayer();

            if (puzzle != null)
                puzzle.ResetPuzzle();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
                ResetLevel();
        }
    }
}
