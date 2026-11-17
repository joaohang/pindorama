#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TwinHook;

namespace TwinHook.Editor
{
    /// <summary>
    /// Atualiza a cor do HookAnchor de cada inimigo da EnemyLab:
    /// RedQte -> vermelho, BlueQte -> azul, Yellow -> neutro.
    /// </summary>
    public static class EnemyLabHookColorSetup
    {
        [MenuItem("Pindorama/Atualizar Cores dos Ganchos (EnemyLab)")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/EnemyLab.unity");
            EnemyBase[] enemies = Object.FindObjectsByType<EnemyBase>();

            foreach (EnemyBase enemy in enemies)
            {
                HookAnchor anchor = enemy.GetComponentInChildren<HookAnchor>();
                if (anchor == null)
                    continue;

                HookColor color = enemy.Type == EnemyType.BlueQte ? HookColor.Blue :
                    (enemy.Type == EnemyType.RedQte ? HookColor.Red : HookColor.Neutral);
                anchor.Configure(color, anchor.Motion);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("EnemyLab: cores dos HookAnchor dos inimigos atualizadas.");
        }
    }
}
#endif
