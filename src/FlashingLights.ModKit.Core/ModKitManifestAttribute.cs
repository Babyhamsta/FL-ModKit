namespace FlashingLights.ModKit.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ModKitManifestAttribute : Attribute
{
    public string? Id { get; set; }
    public string? DisplayName { get; set; }
    public string? Version { get; set; }
    public string? Author { get; set; }
    public string? GitHubUrl { get; set; }
    public string? Changelog { get; set; }
    public string? License { get; set; }
    public string? MinSdkVersion { get; set; }
    public string[]? Dependencies { get; set; }
    public string? Category { get; set; }
    public string[]? Tags { get; set; }
}
