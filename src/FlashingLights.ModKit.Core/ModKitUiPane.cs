namespace FlashingLights.ModKit.Core;

[Flags]
public enum ModKitUiPane
{
    None = 0,
    Mods = 1,
    SelectedMod = 2,
    Config = 4,
    Logs = 8,
    About = 16
}
