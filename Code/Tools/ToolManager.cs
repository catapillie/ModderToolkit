using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.CommunalTools.Tools;

/// <summary>
/// Handler class for all <see cref="Tool"/>s.
/// </summary>
public static class ToolManager
{
    private static readonly Dictionary<Type, Tool> tools = new();

    public static void Register<T>()
        where T : Tool, new()
    {
        if (Has<T>())
            throw new InvalidOperationException($"Tool of type {typeof(T)} is already registered!");
        
        T tool = new();
        tools.Add(typeof(T), tool);

        tool.Registered();

        if (Engine.Scene is Level level)
        {
            tool.Level = level;
            tool.Restart();
        }
    }

    public static void Unregister<T>()
        where T : Tool
    {
        if (!Has<T>())
            throw new InvalidOperationException($"Tool of type {typeof(T)} was never registered!");

        T tool = tools[typeof(T)] as T;
        tools.Remove(typeof(T));

        tool.Unregistered();
    }

    public static void UnregisterAll()
    {
        foreach (Tool tool in tools.Values)
            tool.Unregistered();

        tools.Clear();
    }

    public static bool Has<T>() where T : Tool
        => tools.ContainsKey(typeof(T));

    public static T Get<T>()
        where T : Tool
    {
        if (!Has<T>())
            throw new InvalidOperationException($"Tool of type {typeof(T)} was never registered!");

        return tools[typeof(T)] as T;
    }

    internal static void Load()
    {
        On.Celeste.Level.ctor += Mod_Level_ctor;
        On.Celeste.Level.Update += Mod_Level_Update;
        On.Celeste.Level.Render += Mod_Level_Render;
    }

    internal static void Unload()
    {
        On.Celeste.Level.ctor -= Mod_Level_ctor;
        On.Celeste.Level.Update -= Mod_Level_Update;
        On.Celeste.Level.Render -= Mod_Level_Render;
    }

    private static void Mod_Level_ctor(On.Celeste.Level.orig_ctor orig, Level self)
    {
        orig(self);

        foreach (Tool tool in tools.Values)
        {
            tool.Level = self;
            tool.Restart();
        }
    }

    private static void Mod_Level_Update(On.Celeste.Level.orig_Update orig, Level self)
    {
        bool update = true;
        foreach (Tool tool in tools.Values)
            update &= tool.UpdateBefore();

        if (update)
            orig(self);

        foreach (Tool tool in tools.Values)
        {
            if (!tool.DiscardedUpdate)
                tool.UpdateAfter();

            tool.DiscardedUpdate = false;
        }
    }

    private static void Mod_Level_Render(On.Celeste.Level.orig_Render orig, Level self)
    {
        bool render = true;
        foreach (Tool tool in tools.Values)
            if (!tool.DiscardedRender)
                render &= tool.RenderBefore();

        if (render)
            orig(self);

        foreach (Tool tool in tools.Values)
        {
            if (!tool.DiscardedRender)
                tool.RenderAfter();

            tool.DiscardedRender = false;
        }
    }
}
