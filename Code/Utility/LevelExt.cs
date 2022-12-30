using System.Reflection;

namespace Celeste.Mod.ModderToolkit.Utility;

public static class LevelExt
{
    private static readonly MethodInfo m_Level_StartPauseEffects
        = typeof(Level).GetMethod("StartPauseEffects", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly MethodInfo m_Level_EndPauseEffects
        = typeof(Level).GetMethod("EndPauseEffects", BindingFlags.Instance | BindingFlags.NonPublic);

    public static void StartPauseEffects(this Level level) => m_Level_StartPauseEffects.Invoke(level, Everest._EmptyObjectArray);
    public static void EndPauseEffects(this Level level) => m_Level_EndPauseEffects.Invoke(level, Everest._EmptyObjectArray);
}
