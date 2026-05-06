namespace FlashingLights.ModKit.Core;

internal static class ModKitUiLayoutPolicy
{
    public const int HeaderHeight = 58;
    public const int FooterHeight = 42;
    public const int VerticalGap = 4;
    public const int MinimumBodyHeight = 260;

    public static ModKitUiPane GetVisiblePanes(ModKitUiTab tab)
    {
        return tab switch
        {
            ModKitUiTab.Mods => ModKitUiPane.Mods | ModKitUiPane.SelectedMod,
            ModKitUiTab.Config => ModKitUiPane.Mods | ModKitUiPane.Config,
            ModKitUiTab.Logs => ModKitUiPane.Logs,
            ModKitUiTab.About => ModKitUiPane.About,
            _ => ModKitUiPane.Mods | ModKitUiPane.Config
        };
    }

    public static int CalculateBodyHeight(int screenHeight)
    {
        var available = screenHeight - HeaderHeight - FooterHeight - VerticalGap;
        return Math.Max(MinimumBodyHeight, available);
    }
}
