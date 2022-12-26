﻿using Celeste.Mod.CommunalTools.Tools;
using Celeste.Mod.CommunalTools.Tools.Screenshot;
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
        if (Settings.Screenshotting) ToolManager.Register<Screenshot>();
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
}
