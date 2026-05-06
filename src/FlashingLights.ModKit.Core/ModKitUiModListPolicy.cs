namespace FlashingLights.ModKit.Core;

internal static class ModKitUiModListPolicy
{
    public static IReadOnlyList<IModKitManagedMod> FilterMods(IReadOnlyList<IModKitManagedMod> mods, string? filterText)
    {
        if (string.IsNullOrWhiteSpace(filterText))
        {
            return mods.ToArray();
        }

        var filter = filterText.Trim();
        return mods
            .Where(mod => Matches(mod, filter))
            .ToArray();
    }

    public static bool ShouldShowCategoryHeaders(IReadOnlyList<IModKitManagedMod> mods)
    {
        return mods
            .Select(mod => CategoryFor(mod.Metadata))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(2)
            .Count() >= 2;
    }

    public static string CategoryFor(ModKitModMetadata metadata)
    {
        return string.IsNullOrWhiteSpace(metadata.Category) ? ModKitStrings.Get("ui.uncategorized") : metadata.Category!;
    }

    private static bool Matches(IModKitManagedMod mod, string filter)
    {
        return mod.Metadata.Id.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || mod.Metadata.DisplayName.Contains(filter, StringComparison.OrdinalIgnoreCase)
            || CategoryFor(mod.Metadata).Contains(filter, StringComparison.OrdinalIgnoreCase)
            || mod.Metadata.Tags.Any(tag => tag.Contains(filter, StringComparison.OrdinalIgnoreCase));
    }
}
