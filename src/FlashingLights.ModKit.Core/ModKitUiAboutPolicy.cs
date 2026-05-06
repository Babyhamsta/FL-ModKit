namespace FlashingLights.ModKit.Core;

internal static class ModKitUiAboutPolicy
{
    public const string LicenseUnspecified = "Unspecified";
    public const string MinSdkAny = "any";
    public const string DependenciesNone = "None";
    public const string ValidationOk = "OK";

    public static string FormatLicense(string? license) =>
        string.IsNullOrWhiteSpace(license) ? ModKitStrings.Get("ui.unspecified") : license!;

    public static string FormatMinSdkVersion(string? minSdkVersion) =>
        string.IsNullOrWhiteSpace(minSdkVersion) ? ModKitStrings.Get("ui.any") : $">= {minSdkVersion}";

    public static string FormatDependencies(IReadOnlyList<string> dependencies) =>
        dependencies.Count == 0 ? ModKitStrings.Get("ui.none") : string.Join(", ", dependencies);

    public static string FormatValidationStatus(ModKitManifestValidationResult validation)
    {
        if (validation.IsValid)
        {
            return ModKitStrings.Get("ui.validationOk");
        }
        var noun = validation.Issues.Count == 1
            ? ModKitStrings.Get("ui.issue")
            : ModKitStrings.Get("ui.issues");
        return ModKitStrings.Get("ui.validationBlocked", validation.Issues.Count, noun);
    }

    public static string FormatToggleBlockedMessage(string displayName) =>
        ModKitStrings.Get("ui.modBlocked", displayName);
}
