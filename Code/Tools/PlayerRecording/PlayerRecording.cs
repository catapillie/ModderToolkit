using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System.Collections.Generic;
using System;
using System.IO;

namespace Celeste.Mod.CommunalTools.Tools.PlayerRecording;

public sealed class PlayerRecording : Tool
{
    private bool recording;

    private readonly List<Player.ChaserState> frames = new();
    private long time;

    public override void Restart()
    {
        recording = false;

        frames.Clear();
        time = 0;
    }

    public override bool UpdateBefore()
    {
        if (MInput.Keyboard.Check(Keys.LeftShift) && MInput.Keyboard.Pressed(Keys.P))
        {
            if (recording)
                StopRecording();
            else
                StartRecording();
        }

        return true;
    }

    public override void UpdateAfter()
    {
        if (recording)
            UpdateRecording();
    }

    private void StartRecording()
    {
        frames.Clear();
        time = 0;

        recording = true;
    }

    private void StopRecording()
    {
        if (frames.Count == 0)
            return;

        string path = "Playbacks/test.bin";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        PlaybackData.Export(frames, path);

        recording = false;
    }

    private void UpdateRecording()
    {
        Player player = Level.Tracker.GetEntity<Player>();
        if (player is null)
        {
            StopRecording();
            return;
        }

        frames.Add(new(player));

        time += TimeSpan.FromSeconds(Engine.RawDeltaTime).Ticks;
    }

    public override void RenderAfter()
    {
        if (recording)
            RenderRecording();
    }

    private void RenderRecording()
    {
        Engine.Instance.GraphicsDevice.SetRenderTarget(null);

        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Engine.ScreenMatrix);

        Draw.SpriteBatch.End();
    }
}
