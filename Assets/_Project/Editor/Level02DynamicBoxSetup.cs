#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TwinHook;

namespace TwinHook.Editor
{
    /// <summary>
    /// Converte as caixas da Level02 para Rigidbody2D dinâmico com gravidade,
    /// mantendo o puxão horizontal. Preserva as posições atuais da cena.
    /// </summary>
    public static class Level02DynamicBoxSetup
    {
        [MenuItem("Pindorama/Converter Caixas para Dinâmicas (Level02)")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Level02.unity");

            BoxIdentity[] boxes = Object.FindObjectsOfType<BoxIdentity>();
            foreach (BoxIdentity box in boxes)
            {
                GameObject go = box.gameObject;

                Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.gravityScale = 1f;
                    rb.freezeRotation = true;
                    rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                }

                HookPullable pull = go.GetComponent<HookPullable>();
                if (pull != null)
                {
                    pull.ConfigurePull(PullAxis.Horizontal, 1.2f, 1.5f, 7f, 0.25f);
                    pull.ConfigureDynamic(true, 5f, 45f);
                }

                box.Configure(box.Color, box.Label);
                box.SetDynamicBody(true);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Level02: caixas convertidas para dinâmicas (gravidade + puxão horizontal).");
        }
    }
}
#endif
