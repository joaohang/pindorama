#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TwinHook;

namespace TwinHook.Editor
{
    /// <summary>
    /// Adiciona/configura o PlayerBodyAlignment na cena Level02 já existente,
    /// sem recriar o level (preserva posições e tamanhos que você ajustou).
    /// </summary>
    public static class Level02PlayerAlignmentSetup
    {
        [MenuItem("Pindorama/Ajustar Collider do Player (Level02)")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Level02.unity");

            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogError("Level02: objeto Player não encontrado.");
                return;
            }

            CapsuleCollider2D body = player.GetComponent<CapsuleCollider2D>();
            Transform visualTransform = player.transform.Find("Visual");
            SpriteRenderer visual = visualTransform != null
                ? visualTransform.GetComponent<SpriteRenderer>()
                : null;

            PlayerBodyAlignment align = player.GetComponent<PlayerBodyAlignment>();
            if (align == null)
                align = player.AddComponent<PlayerBodyAlignment>();

            align.Configure(visual, body);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Level02: PlayerBodyAlignment configurado e cena salva.");
        }
    }
}
#endif
