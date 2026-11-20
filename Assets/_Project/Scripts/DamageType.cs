using UnityEngine;

namespace TwinHook
{
    public enum DamageType
    {
        NonLethal,
        Lethal
    }

    public struct DamageInfo
    {
        public DamageType Type;
        public Vector2 SourcePoint;
        public float Knockback;
        public GameObject Source;

        public DamageInfo(DamageType type, Vector2 sourcePoint,
                          float knockback = 12f, GameObject source = null)
        {
            Type = type;
            SourcePoint = sourcePoint;
            Knockback = knockback;
            Source = source;
        }
    }
}
