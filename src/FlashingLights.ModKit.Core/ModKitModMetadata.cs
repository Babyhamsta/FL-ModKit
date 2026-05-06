namespace FlashingLights.ModKit.Core;

public sealed record ModKitModMetadata(
    string Id,
    string DisplayName,
    string Version,
    string Author,
    string? GitHubUrl = null,
    string? Changelog = null,
    string? License = null,
    string? MinSdkVersion = null,
    string? Category = null)
{
    public const string DefaultGitHubUrl = "https://github.com/Babyhamsta/FL-ModKit";

    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}
