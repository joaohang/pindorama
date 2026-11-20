using UnityEngine;

namespace TwinHook
{
    public enum HookColor
    {
        Red,
        Blue,
        Neutral
    }

    public enum HookAnchorMotion
    {
        Swing,
        Fixed
    }

    public enum HookHandState
    {
        Idle,
        Firing,
        Attached,
        Pulling,
        Failed
    }

    public enum DetachReason
    {
        Command,
        Jump,
        Drop,
        AnchorLost,
        Reset,
        Hit
    }

    public enum PullAxis
    {
        Horizontal,
        Any
    }
}
