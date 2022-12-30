using Celeste.Mod.ModderToolkit.Tools;
using Celeste.Mod.ModderToolkit.Tools.PlayerRecording;
using Celeste.Mod.ModderToolkit.Tools.Screenshot;
using System;

namespace Celeste.Mod.ModderToolkit;

public class Module : EverestModule
{
    public static Module Instance { get; private set; }

    public override Type SettingsType => typeof(Settings);
    public static Settings Settings => (Settings)Instance._Settings;

    public Module()
    {
        Instance = this;
    }

    public override void Initialize()
    {
        if (Settings.Screenshotting) ToolManager.Register<Screenshot>();
        if (Settings.PlayerRecording) ToolManager.Register<PlayerRecording>();
    }

    public override void Load()
    {
        ToolManager.Load();
    }

    public override void Unload()
    {
        ToolManager.Unload();
        ToolManager.UnregisterAll();
    }

    internal static void Log(string message, LogLevel level = LogLevel.Verbose)
        => Logger.Log(level, "ModderToolkit", message);
}
