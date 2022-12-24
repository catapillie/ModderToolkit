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

    public override void Load() { }
    public override void Unload() { }
}
