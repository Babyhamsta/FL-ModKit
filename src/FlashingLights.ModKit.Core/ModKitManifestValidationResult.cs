namespace FlashingLights.ModKit.Core;

public sealed record ModKitManifestValidationResult(bool IsValid, IReadOnlyList<string> Issues)
{
    public static ModKitManifestValidationResult Ok { get; } =
        new(IsValid: true, Issues: Array.Empty<string>());

    public static ModKitManifestValidationResult Failed(IReadOnlyList<string> issues) =>
        new(IsValid: false, Issues: issues);
}
