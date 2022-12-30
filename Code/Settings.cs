using Celeste.Mod.ModderToolkit.Tools;
using Celeste.Mod.ModderToolkit.UI;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace Celeste.Mod.ModderToolkit;

[SettingName("modoptions_ModderToolkit")]
public sealed class Settings : EverestModuleSettings
{
    #region Screenshot Tool

    [SettingSubHeader("modoptions_ui_ModderToolkit_Screenshotting_header")]
    [SettingName("modoptions_ui_ModderToolkit_Screenshotting")]
    [SettingSubText("modoptions_ui_ModderToolkit_Screenshotting_desc")]
    public bool Screenshotting { get; set; } = true;
    public void CreateScreenshottingEntry(TextMenu menu, bool _)
        => TextMenuHelper.CreateToolSwitch<Screenshot>(nameof(Screenshotting), menu);

    [SettingName("modoptions_ModderToolkit_ScreenshotBinding")]
    [DefaultButtonBinding(0, Keys.F8)]
    public ButtonBinding ScreenshotBinding { get; set; }

    [SettingRange(1, 16)]
    [SettingName("modoptions_ModderToolkit_ScaleFactor")]
    [SettingSubText("modoptions_ModderToolkit_ScaleFactor_desc")]
    public int ScaleFactor { get; set; } = 1;

    [SettingName("modoptions_ModderToolkit_ScreenshotStatus")]
    [SettingSubText("modoptions_ModderToolkit_ScreenshotStatus_desc")]
    public bool ScreenshotStatus { get; set; } = true;

    [SettingName("modoptions_ModderToolkit_ScreenshotAudio")]
    [SettingSubText("modoptions_ModderToolkit_ScreenshotAudio_desc")]
    public bool ScreenshotAudio { get; set; } = true;

    #endregion

    #region Player Recording Tool

    [SettingSubHeader("modoptions_ModderToolkit_PlayerRecording_header")]
    [SettingName("modoptions_ModderToolkit_PlayerRecording")]
    [SettingSubText("modoptions_ModderToolkit_PlayerRecording_desc")]
    public bool PlayerRecording { get; set; } = true;
    public void CreatePlayerRecordingEntry(TextMenu menu, bool _)
        => TextMenuHelper.CreateToolSwitch<PlayerRecording>(nameof(PlayerRecording), menu);

    [SettingName("modoptions_ModderToolkit_RecordingCountdown")]
    [SettingSubText("modoptions_ModderToolkit_RecordingCountdown_desc")]
    public bool RecordingCountdown { get; set; } = true;

    [SettingName("modoptions_ModderToolkit_RecordingBinding")]
    [DefaultButtonBinding(0, Keys.F12)]
    public ButtonBinding RecordBinding { get; set; }

    #endregion
}
