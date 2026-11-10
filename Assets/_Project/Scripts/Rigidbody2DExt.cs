using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Pequeno shim para o campo de velocidade do Rigidbody2D, que mudou de
    /// nome entre Unity 2022.3 (velocity) e Unity 6 (linearVelocity).
    /// </summary>
    public static class Rigidbody2DExt
    {
        public static Vector2 GetVelocity(this Rigidbody2D rb)
        {
#if UNITY_6000_0_OR_NEWER
            return rb.linearVelocity;
#else
            return rb.velocity;
#endif
        }

        public static void SetVelocity(this Rigidbody2D rb, Vector2 value)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = value;
#else
            rb.velocity = value;
#endif
        }
    }
}
