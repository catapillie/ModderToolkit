using System;

namespace Celeste.Mod.CommunalTools.Tools;

public enum ScreenshotNameStyle
{
    /// <summary>
    /// Compact date, time and room.
    /// </summary>
    Short,

    /// <summary>
    /// Full date and time, room.
    /// </summary>
    Long,
}

public static class ScreenshotNameStyleExt
{
    public static string Name(this ScreenshotNameStyle screenshotNameStyle)
        => Dialog.Clean($"modoptions_CommunalTools_ScreenshotNameStyle_{screenshotNameStyle}");

    public static string Info(this ScreenshotNameStyle screenshotNameStyle)
        => Dialog.Clean($"modoptions_CommunalTools_ScreenshotNameStyle_{screenshotNameStyle}_info") + "\n"
         + Dialog.Clean("modoptions_CommunalTools_ScreenshotNameStyle_example") + ": " + screenshotNameStyle.GetName(DateTime.Now, "[...]");

    public static string GetName(this ScreenshotNameStyle screenshotNameStyle, DateTime dateTime, string room)
        => screenshotNameStyle switch
        {
            ScreenshotNameStyle.Short => $"{dateTime:yyyy-MM-dd}_{dateTime.ToString("T").Replace(':', '.')}_{room}",
            ScreenshotNameStyle.Long => $"{dateTime:D} at {dateTime.ToString("T").Replace(':', '.')}, in room {room}",
            _ => throw new IndexOutOfRangeException(),
        };
}
