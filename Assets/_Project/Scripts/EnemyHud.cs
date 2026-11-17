using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// HUD do EnemyLab: mostra o prompt do QTE/estilingue e os controles.
    /// </summary>
    public class EnemyHud : MonoBehaviour
    {
        private EnemyBase[] enemies;

        private void Start()
        {
            enemies = FindObjectsByType<EnemyBase>();
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 270, 520, 90), GUI.skin.box);
            GUILayout.Label("ENEMY LAB — validação de inimigos");

            if (enemies != null)
            {
                for (int i = 0; i < enemies.Length; i++)
                {
                    EnemyBase enemy = enemies[i];
                    if (enemy != null && !string.IsNullOrEmpty(enemy.Prompt))
                        GUILayout.Label(enemy.Prompt);
                }
            }

            GUILayout.Label("Gancho: LMB/RMB | Soltar QTE: botão da cor | Amarelo: segure e solte");
            GUILayout.EndArea();
        }
    }
}
