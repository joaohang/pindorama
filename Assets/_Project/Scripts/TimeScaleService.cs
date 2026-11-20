using System.Collections.Generic;
using UnityEngine;

namespace TwinHook
{
    /// <summary>
    /// Fonte única de Time.timeScale. Cada sistema empurra seu modificador
    /// (amarelo, hit slowmo); o menor scale ativo vence. Timers são reais.
    /// </summary>
    public static class TimeScaleService
    {
        private sealed class Entry
        {
            public float Scale;
            public float ExpireAt;
        }

        private static readonly Dictionary<string, Entry> entries =
            new Dictionary<string, Entry>();

        public static float Current { get { return Time.timeScale; } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            entries.Clear();
            Time.timeScale = 1f;
        }

        public static void Push(string owner, float scale, float unscaledDuration = -1f)
        {
            if (string.IsNullOrEmpty(owner))
                return;

            entries[owner] = new Entry
            {
                Scale = Mathf.Clamp(scale, 0.02f, 1f),
                ExpireAt = unscaledDuration > 0f
                    ? Time.unscaledTime + unscaledDuration
                    : float.PositiveInfinity
            };
            Apply();
        }

        public static void Pop(string owner)
        {
            if (string.IsNullOrEmpty(owner))
                return;
            if (entries.Remove(owner))
                Apply();
        }

        public static void ClearAll()
        {
            entries.Clear();
            Time.timeScale = 1f;
        }

        public static void Tick()
        {
            bool changed = false;
            float now = Time.unscaledTime;

            List<string> expired = null;
            foreach (KeyValuePair<string, Entry> pair in entries)
            {
                if (now >= pair.Value.ExpireAt)
                {
                    if (expired == null)
                        expired = new List<string>();
                    expired.Add(pair.Key);
                }
            }

            if (expired != null)
            {
                for (int i = 0; i < expired.Count; i++)
                    entries.Remove(expired[i]);
                changed = true;
            }

            if (changed)
                Apply();
        }

        private static void Apply()
        {
            float scale = 1f;
            foreach (KeyValuePair<string, Entry> pair in entries)
                scale = Mathf.Min(scale, pair.Value.Scale);

            Time.timeScale = Mathf.Clamp(scale, 0.02f, 1f);
        }
    }
}
