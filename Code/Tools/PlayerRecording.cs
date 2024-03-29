﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System.Collections.Generic;
using System;
using System.IO;
using Microsoft.Xna.Framework;
using Celeste.Mod.ModderToolkit.Utility;

namespace Celeste.Mod.ModderToolkit.Tools;

public sealed class PlayerRecording : Tool
{
    private bool recording;

    private readonly List<Player.ChaserState> frames = new();
    private long time;

    private float countdown;
    private float countdownLerp = 0f;

    private float ribbonLerp;

    private float statusLerp;
    private string status;

    private string dialog_status_success;
    private string dialog_status_error_player_death;
    private string dialog_status_error_player_lost;
    private string dialog_status_error_room_transition;
    private string dialog_countdown;

    public override void Restart()
    {
        recording = false;

        frames.Clear();
        time = 0;

        countdown = 0f;
        countdownLerp = 0f;

        ribbonLerp = 0f;

        statusLerp = 0f;
        status = string.Empty;

        dialog_status_success = Dialog.Clean("ui_ModderToolkit_recording_dialog_status_success");
        dialog_status_error_player_death = Dialog.Clean("ui_ModderToolkit_recording_dialog_status_error_player_death");
        dialog_status_error_player_lost = Dialog.Clean("ui_ModderToolkit_recording_dialog_status_error_player_lost");
        dialog_status_error_room_transition = Dialog.Clean("ui_ModderToolkit_recording_dialog_status_error_room_transition");
        dialog_countdown = Dialog.Clean("ui_ModderToolkit_recording_dialog_countdown");
    }

    public override bool UpdateBefore()
    {
        statusLerp = Calc.Approach(statusLerp, 0f, Engine.DeltaTime * 4f);

        if (Module.Settings.RecordBinding.Pressed)
        {
            Module.Settings.RecordBinding.ConsumePress();
            if (recording)
            {
                StopRecording();
                if (countdown == 0)
                {
                    SaveRecording();
                    Audio.Play(ModSFX.sfx_recording_success);
                }
                else
                {
                    Audio.Play(SFX.ui_game_unpause);
                    Level.EndPauseEffects();
                }
            }
            else
            {
                StartRecording();
            }
        }

        if (recording && countdown > 0)
            return false;

        return true;
    }

    public override void UpdateAfter()
    {
        ribbonLerp = Calc.Approach(ribbonLerp, recording && countdown == 0 ? 1 : 0, Engine.DeltaTime * 4f);

        if (recording)
            UpdateRecording();
    }

    private void StartRecording()
    {
        if (Level.FrozenOrPaused)
            return;

        Player player = Level.Tracker.GetEntity<Player>();
        if (player is null)
            return;

        frames.Clear();
        time = 0;

        if (Module.Settings.RecordingCountdown)
        {
            countdown = 3f;
            countdownLerp = 0f;
            ToolManager.Focus<PlayerRecording>();
            Level.StartPauseEffects();
        }

        recording = true;
    }

    private void StopRecording()
    {
        recording = false;
    }

    private void SaveRecording()
    {
        DateTime now = DateTime.Now;

        string name = $"{now:yyyy-MM-dd}_{now.ToString("T").Replace(':', '.')}";
        name = string.Join("", name.Split(Path.GetInvalidFileNameChars()));

        string path = $"Playbacks/{name}.bin";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        PlaybackData.Export(frames, path);

        string msg = dialog_status_success
            .Replace("$count", frames.Count.ToString())
            .Replace("$path", path);
        SetStatus(msg);
    }

    private void SetStatus(string message)
    {
        statusLerp = 14;
        status = message;
    }

    private void ErrorHalt(string err)
    {
        StopRecording();
        SetStatus(err);
        Audio.Play(SFX.ui_main_button_invalid);
    }

    private void UpdateRecording()
    {
        if (countdown > 0)
        {
            if (Input.ESC.Pressed)
            {
                Input.ESC.ConsumePress();

                ToolManager.Release<PlayerRecording>();
                Level.EndPauseEffects();

                StopRecording();
                return;
            }

            int prev = (int)countdown;
            countdown = Calc.Approach(countdown, 0, Engine.DeltaTime / 0.75f);
            int num = (int)countdown;

            if (prev != num)
                Audio.Play((num + 1) switch
                {
                    3 => ModSFX.sfx_recording_countdown_three,
                    2 => ModSFX.sfx_recording_countdown_two,
                    1 => ModSFX.sfx_recording_countdown_one,
                    _ => SFX.NONE,
                });

            if (countdown == 0)
            {
                ToolManager.Release<PlayerRecording>();
                Level.EndPauseEffects();

                Audio.Play(ModSFX.sfx_recording_go);

                countdownLerp = 0f;
            }
            else
            {
                countdownLerp = Calc.Approach(countdownLerp, 1.0f, Engine.DeltaTime * 6f);
            }

            return;
        }

        if (Level.Paused)
            return;

        if (Level.Transitioning)
        {
            ErrorHalt(dialog_status_error_room_transition);
            return;
        }

        Player player = Level.Tracker.GetEntity<Player>();
        if (player is null)
        {
            ErrorHalt(dialog_status_error_player_lost);
            return;
        }

        if (player.Dead)
        {
            ErrorHalt(dialog_status_error_player_death);
            return;
        }

        frames.Add(new(player));

        time += TimeSpan.FromSeconds(Engine.RawDeltaTime).Ticks;
    }

    public override void RenderAfter()
    {
        Engine.Instance.GraphicsDevice.SetRenderTarget(null);

        int w = Engine.Width;
        int h = Engine.Height;
        TimeSpan span = TimeSpan.FromTicks(time);
        bool counting = countdown > 0;
        string text;

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Engine.ScreenMatrix);

        if (recording && counting)
        {
            float opacity = Ease.SineInOut(countdownLerp);
            text = ((int)countdown + 1).ToString();

            Draw.Rect(0, 0, w, h, Color.Black * 0.5f * opacity);

            ActiveFont.DrawOutline(text, new(w / 2f, h / 2f), new(0.5f, 0.5f), Vector2.One * 5f, Color.White * opacity, 5, Color.Black * opacity);
            ActiveFont.DrawOutline(dialog_countdown, new(w / 2f, h / 2f - 128), new(0.5f, 0.5f), Vector2.One * 0.75f, Color.White * opacity, 2, Color.Black * opacity);
        }

        PixelFont font = Dialog.Languages["english"].Font;
        float faceSize = Dialog.Languages["english"].FontFaceSize;

        MTexture dot = GFX.Gui["lookout/cursor"];
        MTexture ribbon = GFX.Gui["strawberryCountBG"];

        Vector2 position = new(w - 89, 52);
        if (recording && !counting && !Level.Paused && Util.Blink(span.TotalSeconds, 0.4f))
            dot.DrawCentered(position, Color.Red);

        text = span.ToString(@"mm\:ss");

        position = new(w - 192 * Ease.CubeOut(ribbonLerp), 92);
        ribbon.Draw(position, new(ribbon.Width, 0.0f), Color.White, new Vector2(-1.0f, 1.0f));

        position.X += 106;
        position.Y += ribbon.Height / 2f;
        font.DrawOutline(faceSize, text, position, new(0.5f, 0.5f), Vector2.One * 0.8f, Color.White, 2f, Color.Black);

        if (status is not null)
        {
            float ease = Calc.Clamp(Ease.QuintOut(statusLerp), 0f, 1f);
            ActiveFont.DrawOutline(status, new(w / 2f, 64 * ease), new(0.5f, 1.0f), Vector2.One * 0.5f, Color.White * ease, 2f, Color.Black * ease);
        }

        Draw.SpriteBatch.End();
    }
}
