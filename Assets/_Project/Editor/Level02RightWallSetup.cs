#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TwinHook;

namespace TwinHook.Editor
{
    /// <summary>
    /// Divide a ParedeDireita da Level02 em dois segmentos:
    /// - metade de baixo: ClimbableWall (verde/teal), com HookAnchor;
    /// - metade de cima: parede comum (cinza), sem escalada/gancho.
    /// </summary>
    public static class Level02RightWallSetup
    {
        [MenuItem("Pindorama/Configurar Parede Direita Parcial (Level02)")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Level02.unity");

            GameObject wall = GameObject.Find("ParedeDireita");
            if (wall == null)
            {
                Debug.LogError("Level02: ParedeDireita não encontrada.");
                return;
            }

            Vector3 position = wall.transform.position;
            Vector3 scale = wall.transform.localScale;
            Transform parent = wall.transform.parent;

            SpriteRenderer sourceRenderer = wall.GetComponent<SpriteRenderer>();
            Sprite sprite = sourceRenderer != null ? sourceRenderer.sprite : null;
            int sortingOrder = sourceRenderer != null ? sourceRenderer.sortingOrder : 0;

            float width = Mathf.Abs(scale.x);
            float height = Mathf.Abs(scale.y);
            float lowerHeight = height * 0.55f;
            float upperHeight = height - lowerHeight;

            Vector3 lowerCenter = position + Vector3.down * (height * 0.5f - lowerHeight * 0.5f);
            Vector3 upperCenter = position + Vector3.up * (height * 0.5f - upperHeight * 0.5f);

            Object.DestroyImmediate(wall);

            int climbableLayer = LayerMask.NameToLayer("ClimbableWall");
            if (climbableLayer < 0)
                climbableLayer = 0;

            CreateSegment(parent, "ParedeDireita_Grimpavel", lowerCenter,
                new Vector3(width, lowerHeight, 1f), sprite, sortingOrder,
                new Color(0.16f, 0.72f, 0.62f), climbableLayer, true);

            CreateSegment(parent, "ParedeDireita_Comum", upperCenter,
                new Vector3(width, upperHeight, 1f), sprite, sortingOrder,
                new Color(0.22f, 0.24f, 0.30f), 0, false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Level02: ParedeDireita dividida em trecho escalável (baixo) e comum (cima).");
        }

        private static void CreateSegment(Transform parent, string name, Vector3 position,
                                           Vector3 scale, Sprite sprite, int sortingOrder,
                                           Color color, int layer, bool climbable)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.layer = layer;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = sortingOrder;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            if (climbable)
            {
                HookAnchor anchor = go.AddComponent<HookAnchor>();
                anchor.Configure(HookColor.Neutral, HookAnchorMotion.Swing);
            }
        }
    }
}
#endif
