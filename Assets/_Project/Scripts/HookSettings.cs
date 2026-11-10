using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Configuração de tuning do gancho. No protótipo ela é criada em runtime
    /// pelo GameBootstrap; no jogo final vira um asset (CreateAssetMenu).
    /// </summary>
    [CreateAssetMenu(menuName = "TwinHook/Hook Settings")]
    public class HookSettings : ScriptableObject
    {
        [Header("Disparo")]
        public float range = 8f;
        public float cooldown = 0.12f;
        public float failCooldown = 0.18f;
        public float dualChordWindow = 0.07f;
        public LayerMask hitMask = ~0;

        [Header("Corda (Swing)")]
        public float minRopeLength = 1.5f;
        public float maxRopeLength = 10f;
        public float swingAcceleration = 30f;
        public float maxGrappleSpeed = 26f;

        [Header("Puxada inicial ao prender (Swing)")]
        public float attachReelDistance = 1.5f;
        public float attachReelSpeed = 9f;
        public float attachImpulse = 3.5f;

        [Header("Limite de esticamento")]
        [Tooltip("Fator máximo acima do comprimento atual da corda antes de remover a velocidade para fora.")]
        public float maxStretch = 1.2f;
    }
}
