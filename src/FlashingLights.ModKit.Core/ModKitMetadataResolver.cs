using System.Reflection;

namespace FlashingLights.ModKit.Core;

internal static class ModKitMetadataResolver
{
    public static ModKitModMetadata Resolve(
        Type modType,
        string fallbackId,
        string fallbackDisplayName,
        string fallbackVersion,
        string fallbackAuthor,
        string? fallbackGitHubUrl = null,
        string? fallbackChangelog = null,
        string? fallbackLicense = null,
        string? fallbackMinSdkVersion = null)
    {
        ArgumentNullException.ThrowIfNull(modType);

        var attr = modType.GetCustomAttribute<ModKitManifestAttribute>(inherit: false);
        var dependencies = attr?.Dependencies is { Length: > 0 } values
            ? (IReadOnlyList<string>)values.ToArray()
            : Array.Empty<string>();
        var tags = attr?.Tags is { Length: > 0 } tagValues
            ? (IReadOnlyList<string>)tagValues.ToArray()
            : Array.Empty<string>();

        return new ModKitModMetadata(
            Id: NotBlank(attr?.Id) ?? fallbackId,
            DisplayName: NotBlank(attr?.DisplayName) ?? fallbackDisplayName,
            Version: NotBlank(attr?.Version) ?? fallbackVersion,
            Author: NotBlank(attr?.Author) ?? fallbackAuthor,
            GitHubUrl: NotBlank(attr?.GitHubUrl) ?? fallbackGitHubUrl,
            Changelog: NotBlank(attr?.Changelog) ?? fallbackChangelog,
            License: NotBlank(attr?.License) ?? fallbackLicense,
            MinSdkVersion: NotBlank(attr?.MinSdkVersion) ?? fallbackMinSdkVersion,
            Category: NotBlank(attr?.Category))
        {
            Dependencies = dependencies,
            Tags = tags
        };
    }

    private static string? NotBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
