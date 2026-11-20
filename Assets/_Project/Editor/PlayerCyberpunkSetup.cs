#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using TwinHook;

namespace TwinHook.Editor
{
    /// <summary>
    /// Gera o AnimatorController e o prefab visual do personagem cyberpunk.
    /// Não altera cenas: o PlayerController2D instancia o prefab via Resources.
    /// </summary>
    public static class PlayerCyberpunkSetup
    {
        private const string SpriteRoot = "Assets/_Project/Art/PlayerCyberpunk";
        private const string AnimRoot = "Assets/_Project/Animations/PlayerCyberpunk";
        private const string ControllerPath = AnimRoot + "/PlayerCyberpunk.controller";
        private const string PrefabPath = "Assets/_Project/Resources/PlayerCyberpunkVisual.prefab";

        [MenuItem("Pindorama/Configurar Visual Cyberpunk")]
        public static void Apply()
        {
            EnsureFolders();
            AnimationClip idle = LoadClip("Cyberpunk_Idle");
            AnimationClip run = LoadClip("Cyberpunk_Run");
            AnimationClip jump = LoadClip("Cyberpunk_Jump");

            Sprite[] idles = LoadSprites("Idle", "Idle", 4);
            Sprite[] hangs = LoadSprites("Hang", "Hang", 4);

            if (idle == null || run == null || jump == null ||
                idles[0] == null || hangs[0] == null)
            {
                Debug.LogError("PlayerCyberpunkSetup: sprites ou clips ausentes.");
                return;
            }

            AnimatorController controller = CreateController(idle, run, jump);
            CreatePrefab(controller, idles[0], hangs);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Pindorama: visual cyberpunk configurado.");
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Animations"))
                AssetDatabase.CreateFolder("Assets/_Project", "Animations");
            if (!AssetDatabase.IsValidFolder(AnimRoot))
                AssetDatabase.CreateFolder("Assets/_Project/Animations", "PlayerCyberpunk");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Resources"))
                AssetDatabase.CreateFolder("Assets/_Project", "Resources");
        }

        private static AnimationClip LoadClip(string name)
        {
            return AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimRoot}/{name}.anim");
        }

        private static Sprite[] LoadSprites(string folder, string prefix, int count)
        {
            Sprite[] result = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                string path = $"{SpriteRoot}/{folder}/{prefix}{i + 1}.png";
                result[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            return result;
        }

        private static AnimatorController CreateController(AnimationClip idle,
                                                             AnimationClip run,
                                                             AnimationClip jump)
        {
            AssetDatabase.DeleteAsset(ControllerPath);
            AnimatorController controller =
                AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("isGrounded", AnimatorControllerParameterType.Bool);

            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState idleState = machine.AddState("Idle");
            AnimatorState runState = machine.AddState("Run");
            AnimatorState jumpState = machine.AddState("Jump");
            idleState.motion = idle;
            runState.motion = run;
            jumpState.motion = jump;
            machine.defaultState = idleState;

            AddTransition(idleState, runState,
                AnimatorConditionMode.Greater, "Speed", 0.05f,
                AnimatorConditionMode.If, "isGrounded", 0f);
            AddTransition(runState, idleState,
                AnimatorConditionMode.Less, "Speed", 0.05f,
                AnimatorConditionMode.If, "isGrounded", 0f);
            AddTransition(idleState, jumpState,
                AnimatorConditionMode.IfNot, "isGrounded", 0f);
            AddTransition(runState, jumpState,
                AnimatorConditionMode.IfNot, "isGrounded", 0f);
            AddTransition(jumpState, idleState,
                AnimatorConditionMode.If, "isGrounded", 0f,
                AnimatorConditionMode.Less, "Speed", 0.05f);
            AddTransition(jumpState, runState,
                AnimatorConditionMode.If, "isGrounded", 0f,
                AnimatorConditionMode.Greater, "Speed", 0.05f);

            return controller;
        }

        private static void AddTransition(AnimatorState from, AnimatorState to,
                                          AnimatorConditionMode mode1, string param1, float threshold1,
                                          AnimatorConditionMode mode2 = AnimatorConditionMode.If,
                                          string param2 = "", float threshold2 = 0f)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0.05f;
            transition.AddCondition(mode1, threshold1, param1);
            if (!string.IsNullOrEmpty(param2))
                transition.AddCondition(mode2, threshold2, param2);
        }

        private static void CreatePrefab(AnimatorController controller, Sprite bodySprite,
                                         Sprite[] hangFrames)
        {
            GameObject root = new GameObject("PlayerCyberpunkVisual");
            root.transform.localScale = Vector3.one * 3f;

            GameObject body = new GameObject("Body");
            body.transform.SetParent(root.transform, false);
            SpriteRenderer bodyRenderer = body.AddComponent<SpriteRenderer>();
            bodyRenderer.sprite = bodySprite;
            bodyRenderer.sortingOrder = 4;

            Animator animator = body.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            GameObject pose = new GameObject("Pose");
            pose.transform.SetParent(root.transform, false);
            pose.transform.localPosition = new Vector3(0f, -0.315f, 0f);
            SpriteRenderer poseRenderer = pose.AddComponent<SpriteRenderer>();
            poseRenderer.sprite = hangFrames[0];
            poseRenderer.sortingOrder = 5;
            poseRenderer.enabled = false;

            PlayerCharacterVisual visual = root.AddComponent<PlayerCharacterVisual>();
            visual.Configure(bodyRenderer, animator, poseRenderer, hangFrames);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
        }
    }
}
#endif
