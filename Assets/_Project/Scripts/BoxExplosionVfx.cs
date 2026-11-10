using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Explosão visual de caixa. Reaproveita o sistema de faíscas/poeira do
    /// HookImpactVfx em três pontos para dar volume.
    /// </summary>
    public static class BoxExplosionVfx
    {
        public static void Play(Vector2 position, HookColor color)
        {
            Color main = HookImpactVfx.ColorFor(color);
            HookImpactVfx.Play(position, main);
            HookImpactVfx.Play(position + Vector2.left * 0.35f, new Color(1f, 0.45f, 0.15f, 1f));
            HookImpactVfx.Play(position + Vector2.right * 0.35f, new Color(1f, 0.75f, 0.20f, 1f));
        }
    }
}
