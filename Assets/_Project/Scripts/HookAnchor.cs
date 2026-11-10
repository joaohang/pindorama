using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Marca um collider como ponto onde o gancho pode grudar.
    /// - Motion = Swing  -> parede/teto: cria corda e permite swing.
    /// - Motion = Fixed  -> puxável: fixa o jogador no ponto e permite puxão.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HookAnchor : MonoBehaviour
    {
        [SerializeField] private HookColor color = HookColor.Neutral;
        [SerializeField] private HookAnchorMotion motion = HookAnchorMotion.Swing;
        [SerializeField] private Transform attachPoint;

        public HookColor Color { get { return color; } }
        public HookAnchorMotion Motion { get { return motion; } }

        public Vector2 AttachPoint
        {
            get
            {
                return attachPoint != null
                    ? (Vector2)attachPoint.position
                    : (Vector2)transform.position;
            }
        }

        public void Configure(HookColor c, HookAnchorMotion m)
        {
            color = c;
            motion = m;
        }

        public bool Accepts(HookColor handColor)
        {
            return HookColorRules.Accepts(color, handColor);
        }
    }
}
