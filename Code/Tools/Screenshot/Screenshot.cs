using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;
using MonoMod.Utils;

namespace Celeste.Mod.CommunalTools.Tools.Screenshot;

public sealed class Screenshot : Tool
{
    private static readonly MethodInfo m_Level_StartPauseEffects
        = typeof(Level).GetMethod("StartPauseEffects", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly MethodInfo m_Level_EndPauseEffects
        = typeof(Level).GetMethod("EndPauseEffects", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly Rectangle bounds = new(0, 0, 320, 180);

    private readonly RenderTarget2D buffer = new(Engine.Graphics.GraphicsDevice, 320, 180);
    private readonly RenderTarget2D overlay = new(Engine.Graphics.GraphicsDevice, 320, 180);

    private bool screenshotting;

    private static float fadeLerp, focusLerp, helpLerp;
    private static Vector2 mouse, lastMouse, click;

    private static bool focusing;
    private static Vector2 sa, sb;

    private static float statusLerp;
    private static string status;

    private string dialog_instr_selection_a;
    private string dialog_instr_selection_b;
    private string dialog_hint_enter;
    private string dialog_info_scale;
    private string dialog_info_exit;
    private string dialog_status_success;
    private string dialog_status_error;

    private static EventInstance sfx;

    public override void Begin()
    {
        screenshotting = false;

        fadeLerp = focusLerp = helpLerp = 0f;
        mouse = lastMouse = click = Vector2.Zero;

        focusing = false;
        sa = sb = Vector2.Zero;

        statusLerp = 0f;
        status = string.Empty;

        sfx = null;
    }

    public override bool UpdateBefore()
    {
        UpdateStatus();

        if (screenshotting)
        {
            UpdateScreenshot();
            return false;
        }

        return true;
    }

    public override void UpdateAfter()
    {
        if (MInput.Keyboard.Pressed(Keys.F11))
            EnterScreenshot();
    }

    private void EnterScreenshot()
    {
        // capturing frame into buffer
        Engine.Instance.GraphicsDevice.SetRenderTarget(buffer);
        Engine.Instance.GraphicsDevice.Clear(Color.Black);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGrade.Effect);
        Draw.SpriteBatch.Draw(GameplayBuffers.Level, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SaveData.Instance.Assists.MirrorMode ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0f);
        Draw.SpriteBatch.End();

        int scale = Module.Settings.ScaleFactor;

        // re-initializing dialogue so that we don't do it every frame.
        dialog_instr_selection_a = Dialog.Clean("CommunalTools_screenshotting_dialog_instr_selection_a");
        dialog_instr_selection_b = Dialog.Clean("CommunalTools_screenshotting_dialog_instr_selection_b");
        dialog_hint_enter = Dialog.Clean("CommunalTools_screenshotting_dialog_hint_enter");
        dialog_info_scale = Dialog.Clean("CommunalTools_screenshotting_dialog_info_scale").Replace("$scale", scale.ToString());
        dialog_info_exit = Dialog.Clean("CommunalTools_screenshotting_dialog_info_exit");
        dialog_status_success = Dialog.Clean("CommunalTools_screenshotting_dialog_status_success");
        dialog_status_error = Dialog.Clean("CommunalTools_screenshotting_dialog_status_error");

        status = string.Empty;

        fadeLerp = focusLerp = helpLerp = statusLerp = 0f;
        click = mouse = Vector2.Zero;
        focusing = false;

        Engine.Instance.IsMouseVisible = true;
        screenshotting = true;

        m_Level_StartPauseEffects.Invoke(Engine.Scene as Level, new object[] { });

        if (Module.Settings.ScreenshotAudio)
            sfx ??= Audio.Play(ModSFX.sfx_screenshot_selection);
    }

    private void ExitScreenshot()
    {
        statusLerp = 14;
        Engine.Instance.IsMouseVisible = false;
        screenshotting = false;

        m_Level_EndPauseEffects.Invoke(Engine.Scene as Level, new object[] { });

        if (Module.Settings.ScreenshotAudio && sfx is not null)
        {
            Audio.Stop(sfx);
            sfx = null;
        }
    }

    private void SaveScreenshot(string path, int x, int y, int w, int h, int scale = 1)
    {
        if (scale <= 0)
            throw new ArgumentOutOfRangeException(nameof(scale), "Screenshot scale must be strictly positive.");
        if (w <= 0)
            throw new ArgumentOutOfRangeException(nameof(w), "Screenshot width must be strictly positive");
        if (h <= 0)
            throw new ArgumentOutOfRangeException(nameof(h), "Screenshot height must be stricly positive");

        Rectangle region = new(x, y, w, h);

        if (!bounds.Contains(region))
            throw new ArgumentException("The provided position and size of the region are not contained within the bounds of the screen");

        Color[] data = new Color[w * h];
        buffer.GetData(0, region, data, 0, w * h);

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

    private void UpdateScreenshot()
    {
        lastMouse = mouse;

        fadeLerp = Calc.Approach(fadeLerp, 1f, Engine.DeltaTime * 2f);
        focusLerp = Calc.Approach(focusLerp, MInput.Mouse.CheckLeftButton || focusing ? 1f : 0f, Engine.DeltaTime * 3f);
        helpLerp = Calc.Approach(helpLerp, focusing && !MInput.Mouse.CheckLeftButton ? 1f : 0f, Engine.DeltaTime * 4f);

        if (MInput.Mouse.CheckLeftButton)
        {
            mouse = (MInput.Mouse.Position / 6f).Floor().Clamp(0, 0, 320 - 1, 180 - 1);
            if (MInput.Mouse.PressedLeftButton)
            {
                if (!focusing)
                {
                    focusing = true;
                    if (Module.Settings.ScreenshotAudio)
                        Audio.Play(SFX.ui_game_memorial_text_in);
                }
                click = mouse;
            }

            sa = new(Math.Min(mouse.X, click.X), Math.Min(mouse.Y, click.Y));
            sb = new(Math.Max(mouse.X, click.X), Math.Max(mouse.Y, click.Y));

            if (focusing && sfx is not null)
            {
                float movement = Math.Min(1, Vector2.Distance(mouse, lastMouse) / 8);
                Audio.SetParameter(sfx, "movement", movement);
            }
        }

        if (MInput.Mouse.ReleasedLeftButton && focusing)
        {
            if (Module.Settings.ScreenshotAudio)
            {
                if (sfx is not null)
                    Audio.SetParameter(sfx, "movement", 0f);
                Audio.Play(ModSFX.sfx_screenshot_fix_selection);
            }
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

                if (Module.Settings.ScreenshotAudio)
                    Audio.Play(ModSFX.sfx_screenshot_success);
            }
            catch (Exception ex)
            {
                ex.LogDetailed();
                status = dialog_status_error + "\n" + ex.GetType().FullName + ": " + ex.Message;

                if (Module.Settings.ScreenshotAudio)
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
                if (Module.Settings.ScreenshotAudio)
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

    private void UpdateStatus()
    {
        statusLerp = Calc.Approach(statusLerp, 0f, Engine.DeltaTime * 4f);
    }

    public override bool RenderBefore()
    {
        if (screenshotting)
        {
            RenderOverlay();
            RenderPreview();
            return false;
        }

        return true;
    }

    public override void RenderAfter()
    {
        if (Module.Settings.ScreenshotStatus)
            RenderStatus();
    }

    private void RenderOverlay()
    {
        Engine.Instance.GraphicsDevice.SetRenderTarget(overlay);
        Engine.Instance.GraphicsDevice.Clear(Color.Transparent);

        Vector2 mouse = (MInput.Mouse.Position / 6f).Floor();
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

    private void RenderPreview()
    {
        Engine.Instance.GraphicsDevice.SetRenderTarget(null);
        Engine.Instance.GraphicsDevice.Clear(Color.Black);

        Matrix matrix = Matrix.CreateScale(6f) * Engine.ScreenMatrix;

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, matrix);

        Draw.SpriteBatch.Draw(buffer, Vector2.Zero, Color.White);
        Draw.SpriteBatch.Draw(overlay, Vector2.Zero, Color.White);

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

    private void RenderStatus()
    {
        Engine.Instance.GraphicsDevice.SetRenderTarget(null);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Engine.ScreenMatrix);

        Vector2 middle = new Vector2(Engine.Width, Engine.Height) / 2f;
        float ease = Calc.Clamp(Ease.QuintOut(statusLerp), 0f, 1f);

        ActiveFont.DrawOutline(status, new(middle.X, 64 * ease), new(0.5f, 1.0f), Vector2.One * 0.5f, Color.White * ease, 2f, Color.Black * ease);

        Draw.SpriteBatch.End();
    }
}
