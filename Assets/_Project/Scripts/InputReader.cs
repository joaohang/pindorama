using UnityEngine;
using UnityEngine.InputSystem;

namespace TwinHook
{
    /// <summary>
    /// Entrada centralizada no Input System. Mantém o mesmo esquema de controle
    /// antigo: WASD/setas, mouse para mirar, gamepad com os mesmos botões.
    /// </summary>
    public static class InputReader
    {
        public static bool HasGamepad { get { return Gamepad.current != null; } }
        public static bool HasMouse { get { return Mouse.current != null; } }

        public static Vector2 Move
        {
            get
            {
                float x = 0f;
                float y = 0f;

                Keyboard keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
                    if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
                    if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) y -= 1f;
                    if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) y += 1f;
                }

                Gamepad gamepad = Gamepad.current;
                if (gamepad != null)
                {
                    if (Mathf.Abs(x) < 0.01f && Mathf.Abs(y) < 0.01f)
                    {
                        Vector2 stick = gamepad.leftStick.ReadValue();
                        Vector2 dpad = gamepad.dpad.ReadValue();
                        x = Mathf.Abs(stick.x) > Mathf.Abs(dpad.x) ? stick.x : dpad.x;
                        y = Mathf.Abs(stick.y) > Mathf.Abs(dpad.y) ? stick.y : dpad.y;
                    }
                }

                return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
            }
        }

        public static float Horizontal { get { return Move.x; } }
        public static float Vertical { get { return Move.y; } }

        public static Vector2 AimStick
        {
            get
            {
                Gamepad gamepad = Gamepad.current;
                return gamepad != null ? gamepad.rightStick.ReadValue() : Vector2.zero;
            }
        }

        public static Vector2 MouseScreenPosition
        {
            get
            {
                Mouse mouse = Mouse.current;
                return mouse != null ? mouse.position.ReadValue() : Vector2.zero;
            }
        }

        public static bool JumpDown
        {
            get
            {
                Keyboard keyboard = Keyboard.current;
                Gamepad gamepad = Gamepad.current;
                return (keyboard != null && keyboard.spaceKey.wasPressedThisFrame) ||
                       (gamepad != null && gamepad.buttonSouth.wasPressedThisFrame);
            }
        }

        public static bool DropDown
        {
            get
            {
                Keyboard keyboard = Keyboard.current;
                Gamepad gamepad = Gamepad.current;
                return (keyboard != null &&
                        (keyboard.leftCtrlKey.wasPressedThisFrame ||
                         keyboard.rightCtrlKey.wasPressedThisFrame)) ||
                       (gamepad != null && gamepad.buttonEast.wasPressedThisFrame);
            }
        }

        public static bool BoostDown
        {
            get
            {
                Keyboard keyboard = Keyboard.current;
                Gamepad gamepad = Gamepad.current;
                return (keyboard != null &&
                        (keyboard.leftShiftKey.wasPressedThisFrame ||
                         keyboard.rightShiftKey.wasPressedThisFrame)) ||
                       (gamepad != null && gamepad.buttonWest.wasPressedThisFrame);
            }
        }

        public static bool ResetDown
        {
            get
            {
                Keyboard keyboard = Keyboard.current;
                Gamepad gamepad = Gamepad.current;
                return (keyboard != null && keyboard.rKey.wasPressedThisFrame) ||
                       (gamepad != null && gamepad.startButton.wasPressedThisFrame);
            }
        }

        public static bool RedHookDown
        {
            get
            {
                Mouse mouse = Mouse.current;
                Gamepad gamepad = Gamepad.current;
                return (mouse != null && mouse.leftButton.wasPressedThisFrame) ||
                       (gamepad != null && gamepad.leftShoulder.wasPressedThisFrame);
            }
        }

        public static bool BlueHookDown
        {
            get
            {
                Mouse mouse = Mouse.current;
                Gamepad gamepad = Gamepad.current;
                return (mouse != null && mouse.rightButton.wasPressedThisFrame) ||
                       (gamepad != null && gamepad.rightShoulder.wasPressedThisFrame);
            }
        }

        public static bool RedHookHeld
        {
            get
            {
                Mouse mouse = Mouse.current;
                Gamepad gamepad = Gamepad.current;
                return (mouse != null && mouse.leftButton.isPressed) ||
                       (gamepad != null && gamepad.leftShoulder.isPressed);
            }
        }

        public static bool BlueHookHeld
        {
            get
            {
                Mouse mouse = Mouse.current;
                Gamepad gamepad = Gamepad.current;
                return (mouse != null && mouse.rightButton.isPressed) ||
                       (gamepad != null && gamepad.rightShoulder.isPressed);
            }
        }

        public static bool RedHookUp
        {
            get
            {
                Mouse mouse = Mouse.current;
                Gamepad gamepad = Gamepad.current;
                return (mouse != null && mouse.leftButton.wasReleasedThisFrame) ||
                       (gamepad != null && gamepad.leftShoulder.wasReleasedThisFrame);
            }
        }

        public static bool BlueHookUp
        {
            get
            {
                Mouse mouse = Mouse.current;
                Gamepad gamepad = Gamepad.current;
                return (mouse != null && mouse.rightButton.wasReleasedThisFrame) ||
                       (gamepad != null && gamepad.rightShoulder.wasReleasedThisFrame);
            }
        }
    }
}
