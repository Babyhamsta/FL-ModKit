namespace FlashingLights.ModKit.Core;

public sealed record ModKitConfigProperty(
    string Name,
    string DisplayName,
    ModKitConfigValueKind Kind,
    string TypeName,
    string ValueText,
    bool CanWrite,
    IReadOnlyList<string> Options,
    double? Minimum = null,
    double? Maximum = null,
    double? Step = null)
{
    public bool HasRange => Minimum.HasValue
        && Maximum.HasValue
        && Maximum.Value > Minimum.Value;
}
