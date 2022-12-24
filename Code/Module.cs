using Celeste.Mod.CommunalTools.Tools;
using System;

namespace Celeste.Mod.CommunalTools;

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
        Screenshotting.Initialize();
    }

    public override void Load()
    {
        Screenshotting.Load();
    }

    public override void Unload()
    {
        Screenshotting.Unload();
    }
}
