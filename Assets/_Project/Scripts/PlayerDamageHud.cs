using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Pips de falhas + flash de impacto/morte. Fica no Player para não
    /// depender de mudanças nas cenas.
    /// </summary>
    public class PlayerDamageHud : MonoBehaviour
    {
        private PlayerDamageController damage;

        private void Awake()
        {
            damage = GetComponent<PlayerDamageController>();
        }

        private void OnGUI()
        {
            if (damage == null)
                return;

            float width = 220f;
            Rect area = new Rect(Screen.width - width - 16f, 16f, width, 112f);
            GUILayout.BeginArea(area, GUI.skin.box);
            GUILayout.Label("INTEGRIDADE");

            string pips = string.Empty;
            for (int i = 0; i < damage.MaxFailures; i++)
                pips += i < damage.Failures ? "● " : "○ ";
            GUILayout.Label("FALHAS  " + pips);

            switch (damage.State)
            {
                case PlayerDamageState.HitStun:
                    GUILayout.Label("IMPACTO");
                    break;
                case PlayerDamageState.Recovering:
                    GUILayout.Label("RECUPERANDO...");
                    break;
                case PlayerDamageState.Dead:
                case PlayerDamageState.Respawning:
                    GUILayout.Label("SISTEMA CRÍTICO");
                    break;
                default:
                    GUILayout.Label(damage.Failures > 0 ? "ATENÇÃO" : "ESTÁVEL");
                    break;
            }

            GUILayout.EndArea();

            if (Time.unscaledTime >= damage.FlashUntil)
                return;

            float remaining = Mathf.Clamp01((damage.FlashUntil - Time.unscaledTime) / 0.12f);
            bool critical = damage.IsDead;
            GUI.color = critical
                ? new Color(0.8f, 0.05f, 0.08f, 0.32f * remaining)
                : new Color(1f, 1f, 1f, 0.22f * remaining);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height),
                Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
