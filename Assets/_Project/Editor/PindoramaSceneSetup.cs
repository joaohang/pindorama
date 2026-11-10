#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TwinHook;

namespace TwinHook.Editor
{
    /// <summary>
    /// Cria a cena Main.unity com um GameBootstrap, para abrir e dar Play sem
    /// configurar nada no inspector. Também adiciona a cena ao Build Settings.
    /// </summary>
    public static class PindoramaSceneSetup
    {
        public static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var go = new GameObject("GameBootstrap");
            go.AddComponent<GameBootstrap>();

            const string folder = "Assets/_Project/Scenes";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string parent = "Assets/_Project";
                AssetDatabase.CreateFolder(parent, "Scenes");
            }

            const string path = "Assets/_Project/Scenes/Main.unity";
            EditorSceneManager.SaveScene(scene, path);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(path, true)
            };

            AssetDatabase.SaveAssets();
            Debug.Log("PINDORAMA: cena Main.unity criada e adicionada ao Build Settings.");
        }
    }
}
#endif
