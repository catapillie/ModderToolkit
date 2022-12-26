using Celeste.Mod.CommunalTools.Tools.Screenshot;
using Celeste.Mod.CommunalTools.UI;
using System;
using System.Linq;

namespace Celeste.Mod.CommunalTools;

[SettingName("modoptions_CommunalTools")]
public sealed class Settings : EverestModuleSettings
{
    [SettingSubHeader("modoptions_CommunalTools_Screenshotting_header")]
    [SettingName("modoptions_CommunalTools_Screenshotting")]
    [SettingSubText("modoptions_CommunalTools_Screenshotting_desc")]
    public bool Screenshotting { get; set; } = true;
    public void CreateScreenshottingEntry(TextMenu menu, bool _)
        => TextMenuHelper.CreateToolSwitch<Screenshot>(nameof(Screenshotting), menu);

    //[SettingName("modoptions_CommunalTools_NameStyle")]
    public ScreenshotNameStyle NameStyle { get; set; } = ScreenshotNameStyle.Short;
    public void CreateNameStyleEntry(TextMenu menu, bool _)
    {
        TextMenuExt.EaseInSubHeaderExt info = null;

        var item = new TextMenuExt.EnumSlider<ScreenshotNameStyle>(Dialog.Clean("modoptions_CommunalTools_NameStyle"), NameStyle)
            .Change(setting =>
            {
                NameStyle = setting;
                info.Title = setting.Info();
            });

        item.Values = (Enum.GetValues(typeof(ScreenshotNameStyle)) as ScreenshotNameStyle[])
            .Select(setting => Tuple.Create(setting.Name(), setting))
            .ToList();

        menu.Add(item);

        info = item.AddThenGetDescription(menu, NameStyle.Info());
        item.AddDescription(menu, Dialog.Clean("modoptions_CommunalTools_NameStyle_desc"));
    }

    [SettingRange(1, 16)]
    [SettingName("modoptions_CommunalTools_ScaleFactor")]
    [SettingSubText("modoptions_CommunalTools_ScaleFactor_desc")]
    public int ScaleFactor { get; set; } = 1;

    [SettingName("modoptions_CommunalTools_ScreenshotStatus")]
    [SettingSubText("modoptions_CommunalTools_ScreenshotStatus_desc")]
    public bool ScreenshotStatus { get; set; } = true;

    [SettingName("modoptions_CommunalTools_ScreenshotAudio")]
    [SettingSubText("modoptions_CommunalTools_ScreenshotAudio_desc")]
    public bool ScreenshotAudio { get; set; } = true;
}
