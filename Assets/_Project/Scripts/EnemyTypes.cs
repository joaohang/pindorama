using UnityEngine;

namespace TwinHook
{
    public enum EnemyType
    {
        Yellow,   // estilingue: puxa, gruda, slow motion, lança
        BlueQte,  // QTE com botão azul (RMB)
        RedQte    // QTE com botão vermelho (LMB)
    }

    public enum EnemyArchetype
    {
        Soldier,
        Turret
    }

    public enum EnemyState
    {
        Idle,
        Pulling,
        YellowStuck,
        QteWindow,
        Dead
    }

    public interface IHookReceiver
    {
        void OnHookAttached(HookHand hand, Vector2 hitPoint);
        void OnHookDetached(HookHand hand, DetachReason reason);
    }
}
