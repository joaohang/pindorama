using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// HUD mínimo em OnGUI (sem dependência de TextMeshPro) para o protótipo.
    /// </summary>
    public class HookHud : MonoBehaviour
    {
        private PlayerController2D player;
        private DualHookController controller;
        private HookHand red;
        private HookHand blue;

        public void Configure(PlayerController2D p, DualHookController c, HookHand redHand, HookHand blueHand)
        {
            player = p;
            controller = c;
            red = redHand;
            blue = blueHand;
        }

        private void OnGUI()
        {
            if (player == null || controller == null)
                return;

            GUILayout.BeginArea(new Rect(10, 10, 520, 250), GUI.skin.box);
            GUILayout.Label("PINDORAMA — protótipo de ganchos");
            GUILayout.Label("Mira: " + controller.AimDirection);
            GUILayout.Label("Vermelho (LMB): " + StateText(red));
            GUILayout.Label("Azul (RMB): " + StateText(blue));
            GUILayout.Label("Braço (2 ganchos): " + (player.IsBraced ? "SIM" : "não"));
            GUILayout.Label("No chão: " + (player.IsGrounded ? "SIM" : "não"));

            string boostText;
            if (!controller.IsSwinging)
                boostText = "—";
            else if (controller.BoostReady)
                boostText = "PRONTO";
            else
                boostText = controller.BoostCooldownRemaining.ToString("0.0") + "s";
            GUILayout.Label("Boost (Shift): " + boostText);

            GUILayout.Space(6);
            GUILayout.Label("Mover: A/D | Mirar: mouse | Vermelho: LMB | Azul: RMB");
            GUILayout.Label("Pular/soltar: Espaço | Boost: Shift | Soltar sem pular: Ctrl | Reset: R");
            GUILayout.Label("Espelhado: LMB+RMB juntos | Puxão: apertar o botão da mão presa");
            GUILayout.EndArea();
        }

        private static string StateText(HookHand hand)
        {
            if (hand == null)
                return "-";
            if (hand.IsSwing)
                return hand.State + " (segure)";
            if (hand.IsPullable)
                return hand.State + " (puxável)";
            return hand.State.ToString();
        }
    }
}
