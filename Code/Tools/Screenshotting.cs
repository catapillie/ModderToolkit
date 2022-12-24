using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod.Utils;
using System;
using System.IO;
using System.Reflection;

namespace Celeste.Mod.CommunalTools.Tools;

public static class Screenshotting
{
    private static readonly MethodInfo m_Level_StartPauseEffects
        = typeof(Level).GetMethod("StartPauseEffects", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly MethodInfo m_Level_EndPauseEffects
        = typeof(Level).GetMethod("EndPauseEffects", BindingFlags.Instance | BindingFlags.NonPublic);

    public static RenderTarget2D Buffer { get; private set; }
    public static RenderTarget2D Overlay { get; private set; }

    internal static void Initialize()
    {
        Buffer = new RenderTarget2D(Engine.Graphics.GraphicsDevice, 320, 180);
        Overlay = new RenderTarget2D(Engine.Graphics.GraphicsDevice, 320, 180);
    }

    internal static void Load()
    {
        On.Celeste.Level.Update += Mod_Level_Update;
        On.Celeste.Level.Render += Level_Render;
    }

    internal static void Unload()
    {
        On.Celeste.Level.Update -= Mod_Level_Update;
        On.Celeste.Level.Render -= Level_Render;
    }

    private static bool screenshotting = false;

    private static void Mod_Level_Update(On.Celeste.Level.orig_Update orig, Level self)
    {
        UpdateStatus();

        if (screenshotting)
        {
            UpdateScreenshot();
            return;
        }

        orig(self);

        if (!Module.Settings.Screenshotting)
            return;

        if (MInput.Keyboard.Pressed(Keys.F11))
            EnterScreenshot();
    }

    private static void Level_Render(On.Celeste.Level.orig_Render orig, Level self)
    {
        if (screenshotting)
        {
            RenderOverlay();
            RenderPreview();
            return;
        }

        orig(self);

        if (Module.Settings.ScreenshotStatus)
            RenderStatus();
    }

    private static EventInstance snapshot;

    private static float fadeLerp, focusLerp, helpLerp;
    private static Vector2 mouse, click;
    private static Vector2 sa, sb;
    private static bool focusing;

    private static float statusLerp;
    private static string status = string.Empty;
    private static Color statusColor = Color.White;

    private static readonly Rectangle limit = new(0, 0, 320, 180);

    private static string dialog_instr_selection_a;
    private static string dialog_instr_selection_b;
    private static string dialog_hint_enter;
    private static string dialog_info_scale;
    private static string dialog_info_exit;
    private static string dialog_status_success;
    private static string dialog_status_error;

    private static void CaptureLevelBuffer()
    {
        Engine.Instance.GraphicsDevice.SetRenderTarget(Buffer);
        Engine.Instance.GraphicsDevice.Clear(Color.Black);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGrade.Effect);
        Draw.SpriteBatch.Draw(GameplayBuffers.Level, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SaveData.Instance.Assists.MirrorMode ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        Draw.SpriteBatch.End();
    }

    private static void EnterScreenshot()
    {
        CaptureLevelBuffer();

        int scale = Module.Settings.ScaleFactor;

        // re-initializing dialogue so that we don't do it every frame.
        dialog_instr_selection_a    = Dialog.Clean("CommunalTools_screenshotting_dialog_instr_selection_a");
        dialog_instr_selection_b    = Dialog.Clean("CommunalTools_screenshotting_dialog_instr_selection_b");
        dialog_hint_enter           = Dialog.Clean("CommunalTools_screenshotting_dialog_hint_enter");
        dialog_info_scale           = Dialog.Clean("CommunalTools_screenshotting_dialog_info_scale").Replace("$scale", scale.ToString());
        dialog_info_exit            = Dialog.Clean("CommunalTools_screenshotting_dialog_info_exit");
        dialog_status_success       = Dialog.Clean("CommunalTools_screenshotting_dialog_status_success");
        dialog_status_error         = Dialog.Clean("CommunalTools_screenshotting_dialog_status_error");

        status = string.Empty;

        fadeLerp = focusLerp = helpLerp = statusLerp = 0f;
        click = mouse = Vector2.Zero;
        focusing = false;

        Engine.Instance.IsMouseVisible = true;
        screenshotting = true;

        m_Level_StartPauseEffects.Invoke(Engine.Scene as Level, new object[] { });
    }

    private static void ExitScreenshot()
    {
        statusLerp = 14;
        Engine.Instance.IsMouseVisible = false;
        screenshotting = false;

        m_Level_EndPauseEffects.Invoke(Engine.Scene as Level, new object[] { });
    }

    private static void SaveScreenshot(string path, int x, int y, int w, int h, int scale = 1)
    {
        if (scale <= 0)
            throw new ArgumentOutOfRangeException(nameof(scale), "Screenshot scale must be strictly positive.");
        if (w <= 0)
            throw new ArgumentOutOfRangeException(nameof(w), "Screenshot width must be strictly positive");
        if (h <= 0)
            throw new ArgumentOutOfRangeException(nameof(h), "Screenshot height must be stricly positive");

        Rectangle region = new(x, y, w, h);

        if (!limit.Contains(region))
            throw new ArgumentException("The provided position and size of the region are not contained within the bounds of the screen");
        
        Color[] data = new Color[w * h];
        Buffer.GetData(0, region, data, 0, w * h);

        // native resolution screenshot.
        using Texture2D native = new(Engine.Graphics.GraphicsDevice, w, h);
        native.SetData(data);

        // upscaling texture.
        using RenderTarget2D final = new(Engine.Graphics.GraphicsDevice, w * scale, h * scale);

        Engine.Instance.GraphicsDevice.SetRenderTarget(final);
        Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
        Draw.SpriteBatch.Draw(native, new Rectangle(0, 0, final.Width, final.Height), Color.White);
        Draw.SpriteBatch.End();

        // saving in file.
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using Stream stream = File.OpenWrite(path);
        final.SaveAsPng(stream, final.Width, final.Height);
    }

    private static void UpdateScreenshot()
    {
        fadeLerp = Calc.Approach(fadeLerp, 1f, Engine.DeltaTime * 2f);
        focusLerp = Calc.Approach(focusLerp, MInput.Mouse.CheckLeftButton || focusing ? 1f : 0f, Engine.DeltaTime * 3f);
        helpLerp = Calc.Approach(helpLerp, focusing && !MInput.Mouse.CheckLeftButton ? 1f : 0f, Engine.DeltaTime * 4f);

        if (MInput.Mouse.CheckLeftButton)
        {
            mouse = Calc.Clamp(Calc.Floor(MInput.Mouse.Position / 6f), 0, 0, 320 - 1, 180 - 1);
            if (MInput.Mouse.PressedLeftButton)
            {
                if (!focusing)
                {
                    focusing = true;
                    Audio.Play(SFX.ui_game_memorial_text_in);
                }
                click = mouse;
            }

            sa = new(Math.Min(mouse.X, click.X), Math.Min(mouse.Y, click.Y));
            sb = new(Math.Max(mouse.X, click.X), Math.Max(mouse.Y, click.Y));
        }

        if (MInput.Keyboard.Pressed(Keys.Enter))
        {
            // ENTER might cause the pause menu to open, so let's prevent that from happening
            Input.Pause.ConsumeBuffer();

            int x = focusing ? (int)sa.X : 0;
            int y = focusing ? (int)sa.Y : 0;
            int w = focusing ? (int)(sb.X - sa.X) + 1 : 320;
            int h = focusing ? (int)(sb.Y - sa.Y) + 1 : 180;

            Level level = Engine.Scene as Level;
            string room = level.Session.Level;

            // hopefully create safe name to be saved.
            string name = Module.Settings.NameStyle.GetName(DateTime.Now, room);
            name = string.Join("", name.Split(Path.GetInvalidFileNameChars()));

            int scale = Module.Settings.ScaleFactor;

            try
            {
                SaveScreenshot($"Screenshots/{name}.png", x, y, w, h, scale);
                status = dialog_status_success.Replace("$name", $"\"{name}.png\"");
                statusColor = Color.White;
            }
            catch (Exception ex)
            {
                ex.LogDetailed();
                status = dialog_status_error + "\n" + ex.GetType().FullName + ": " + ex.Message;
                statusColor = Calc.HexToColor("f03434");
                Audio.Play(SFX.ui_main_button_invalid);
            }

            ExitScreenshot();
            return;
        }

        if (Input.ESC.Pressed)
        {
            // ESCAPE opens the pause menu, so we consume the input to prevent the pause menu to open.
            Input.ESC.ConsumeBuffer();

            if (focusing)
            {
                Audio.Play(SFX.ui_game_memorial_text_out);
                focusing = false;
            }
            else
            {
                ExitScreenshot();
                Audio.Play(SFX.ui_game_unpause);
            }

            return;
        }

        if (MInput.Keyboard.Pressed(Keys.F11))
        {
            ExitScreenshot();
            Audio.Play(SFX.ui_game_unpause);
        }
    }

    private static void UpdateStatus()
    {
        statusLerp = Calc.Approach(statusLerp, 0f, Engine.DeltaTime * 4f);
    }

    private static void RenderOverlay()
    {
        Engine.Instance.GraphicsDevice.SetRenderTarget(Overlay);
        Engine.Instance.GraphicsDevice.Clear(Color.Transparent);

        Vector2 mouse = Calc.Floor(MInput.Mouse.Position / 6f);
        Color background = Color.Black * (Ease.SineInOut(fadeLerp) * 0.45f + Ease.SineInOut(focusLerp) * 0.45f);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

        if (focusing)
        {
            Draw.Rect(0, 0, sa.X, 180, background);
            Draw.Rect(sa.X, 0, 320 - sa.X, sa.Y, background);
            Draw.Rect(sa.X, sb.Y + 1, 320 - sa.X, 180 - sb.Y, background);
            Draw.Rect(sb.X + 1, sa.Y, 320 - sb.X, sb.Y - sa.Y + 1, background);
        }
        else
        {
            Draw.Rect(0, 0, 320, 180, background);
        }

        Draw.SpriteBatch.End();
    }

    private static void RenderPreview()
    {
        Engine.Instance.GraphicsDevice.SetRenderTarget(null);
        Engine.Instance.GraphicsDevice.Clear(Color.Black);

        Matrix matrix = Matrix.CreateScale(6f) * Engine.ScreenMatrix;

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);
        
        Draw.SpriteBatch.Draw(Buffer, Vector2.Zero, Color.White);
        Draw.SpriteBatch.Draw(Overlay, Vector2.Zero, Color.White);
        
        Draw.SpriteBatch.End();

        Vector2 middle = new Vector2(Engine.Width, Engine.Height) / 2f;

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Engine.ScreenMatrix);

        int scale = Module.Settings.ScaleFactor;
        float opacity = Ease.SineInOut(fadeLerp * (1 - focusLerp));

        ActiveFont.DrawOutline(dialog_instr_selection_a, new(middle.X, 64), new(0.5f, 0.0f), Vector2.One, Color.White * opacity, 2f, Color.Black * opacity);
        ActiveFont.DrawOutline(dialog_hint_enter, new(middle.X, 128), new(0.5f, 0.0f), Vector2.One * 0.75f, Color.White * opacity, 2f, Color.Black * opacity);
        ActiveFont.DrawOutline(dialog_info_scale, new(middle.X, Engine.Height - 64), new(0.5f, 1.0f), Vector2.One * 0.5f, Color.White * opacity, 2f, Color.Black * opacity);
        ActiveFont.DrawOutline(dialog_info_exit, new(middle.X, Engine.Height - 32), new(0.5f, 1.0f), Vector2.One * 0.5f, Color.White * opacity, 2f, Color.Black * opacity);

        if (focusing)
        {
            opacity = Ease.SineInOut(focusLerp);

            int w = (int)(sb.X - sa.X) + 1;
            int h = (int)(sb.Y - sa.Y) + 1;

            Vector2 at = new Vector2((sa.X + sb.X) / 2f, Calc.Clamp(sa.Y, 6, 180)) * 6;
            ActiveFont.DrawOutline($"{w}x{h}", at, new(0.5f, 1.0f), Vector2.One * 0.5f, Color.White * opacity, 2f, Color.Black * opacity);
            if (scale != 1)
            {
                at = new Vector2((sa.X + sb.X) / 2f, Calc.Clamp(sb.Y + 1, 0, 172)) * 6;
                ActiveFont.DrawOutline($"{w * scale}x{h * scale}", at, new(0.5f, 0.0f), Vector2.One * 0.3f, Color.White * opacity, 2f, Color.Black * opacity);
                ActiveFont.DrawOutline($"[x{scale}]", at + Vector2.UnitY * 16, new(0.5f, 0.0f), Vector2.One * 0.3f, Color.White * opacity, 2f, Color.Black * opacity);
            }

            if (helpLerp > 0f)
            {
                opacity = helpLerp;
                float ease = Ease.QuadInOut(helpLerp);
                ActiveFont.DrawOutline(dialog_instr_selection_b, new(middle.X, 96 * ease), new(0.5f, 1.0f), Vector2.One * 0.5f, Color.White * opacity, 2f, Color.Black * opacity);
            }
        }

        Draw.SpriteBatch.End();
    }

    private static void RenderStatus()
    {
        Engine.Instance.GraphicsDevice.SetRenderTarget(null);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Engine.ScreenMatrix);
        
        Vector2 middle = new Vector2(Engine.Width, Engine.Height) / 2f;
        float ease = Calc.Clamp(Ease.QuintOut(statusLerp), 0f, 1f);

        ActiveFont.DrawOutline(status, new(middle.X, 64 * ease), new(0.5f, 1.0f), Vector2.One * 0.5f, statusColor * ease, 2f, Color.Black * ease);

        Draw.SpriteBatch.End();
    }
}