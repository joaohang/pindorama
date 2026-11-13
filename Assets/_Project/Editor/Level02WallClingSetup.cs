#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TwinHook;

namespace TwinHook.Editor
{
    /// <summary>
    /// Configura a cena Level02 para Wall Cling: marca as paredes externas
    /// como ClimbableWall, adiciona HookAnchor nelas e põe o componente no Player.
    /// </summary>
    public static class Level02WallClingSetup
    {
        [MenuItem("Pindorama/Configurar Wall Cling (Level02)")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Level02.unity");

            int layer = LayerMask.NameToLayer("ClimbableWall");
            if (layer < 0)
            {
                Debug.LogError("Layer 'ClimbableWall' não existe. Crie em Project Settings > Tags and Layers.");
                return;
            }

            string[] wallNames = { "ParedeEsquerda", "ParedeDireita" };
            foreach (string wallName in wallNames)
            {
                GameObject wall = GameObject.Find(wallName);
                if (wall == null)
                    continue;

                wall.layer = layer;

                HookAnchor anchor = wall.GetComponent<HookAnchor>();
                if (anchor == null)
                    anchor = wall.AddComponent<HookAnchor>();
                anchor.Configure(HookColor.Neutral, HookAnchorMotion.Swing);
            }

            GameObject player = GameObject.Find("Player");
            if (player != null && player.GetComponent<WallClingController>() == null)
                player.AddComponent<WallClingController>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Level02: Wall Cling configurado (paredes ClimbableWall + componente no Player).");
        }
    }
}
#endif
