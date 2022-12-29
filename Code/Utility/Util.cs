namespace Celeste.Mod.CommunalTools.Utility;

public static class Util
{
    public static bool Blink(float time, float duration) => time % (duration * 2) < duration;
    public static bool Blink(double time, double duration) => time % (duration * 2) < duration;
}
