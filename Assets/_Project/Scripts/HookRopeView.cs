using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Visual do cabo (LineRenderer). É apenas visual: a física fica no
    /// DistanceJoint2D (Swing) ou no estado fixo (Fixed).
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class HookRopeView : MonoBehaviour
    {
        [SerializeField] private HookHand hand;
        [SerializeField] private Transform muzzle;

        private LineRenderer line;

        private void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startWidth = 0.06f;
            line.endWidth = 0.06f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.sortingOrder = 10;
            line.enabled = false;
        }

        public void Configure(HookHand handRef, Transform muzzleTransform, Color color)
        {
            hand = handRef;
            muzzle = muzzleTransform;
            line.startColor = color;
            line.endColor = color;
        }

        private void LateUpdate()
        {
            if (hand == null || muzzle == null || !hand.IsAttached || hand.Anchor == null)
            {
                if (line != null)
                    line.enabled = false;
                return;
            }

            line.enabled = true;
            line.SetPosition(0, muzzle.position);
            line.SetPosition(1, hand.Anchor.AttachPoint);
        }
    }
}
