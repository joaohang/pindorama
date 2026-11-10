namespace TwinHook
{
    public static class HookColorRules
    {
        public static bool Accepts(HookColor anchorColor, HookColor handColor)
        {
            // Mãos são sempre Red ou Blue; Neutral é usado apenas por âncoras.
            if (handColor == HookColor.Neutral)
                return false;

            return anchorColor == HookColor.Neutral || anchorColor == handColor;
        }
    }
}
