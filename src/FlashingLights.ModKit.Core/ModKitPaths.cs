using MelonLoader.Utils;

namespace FlashingLights.ModKit.Core;

public static class ModKitPaths
{
    public const string UserDataFolderName = "FlashingLightsModKit";

    private static readonly HashSet<string> ReservedDosNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    public static string UserDataRoot(string? baseDirectory = null)
    {
        return string.IsNullOrWhiteSpace(baseDirectory)
            ? Path.Combine(MelonEnvironment.UserDataDirectory, UserDataFolderName)
            : baseDirectory;
    }

    public static string ConfigPath(string modId, string? baseDirectory = null)
    {
        var root = UserDataRoot(baseDirectory);
        var fileName = $"{ValidateFileName(modId, nameof(modId))}.json";
        return BuildSafePath(root, fileName, nameof(modId));
    }

    public static string LogPath(string fileName, string? baseDirectory = null)
    {
        var root = UserDataRoot(baseDirectory);
        var validated = ValidateFileName(fileName, nameof(fileName));
        return BuildSafePath(root, validated, nameof(fileName));
    }

    public static string EnsureDirectory(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentException("Directory cannot be empty.", nameof(directory));
        }

        Directory.CreateDirectory(directory);
        return directory;
    }

    public static string EnsureParentDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be empty.", nameof(path));
        }

        var parent = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(parent))
        {
            Directory.CreateDirectory(parent);
        }

        return path;
    }

    public static string ValidateFileName(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("File name cannot be empty.", parameterName);
        }

        if (value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException($"File name contains invalid characters: {value}", parameterName);
        }

        if (value.Contains('/') || value.Contains('\\'))
        {
            throw new ArgumentException($"File name cannot contain path separators: {value}", parameterName);
        }

        var trimmed = value.Trim();
        if (trimmed != value)
        {
            throw new ArgumentException($"File name cannot have leading or trailing whitespace: {value}", parameterName);
        }

        if (value.StartsWith('.') || value.EndsWith('.'))
        {
            throw new ArgumentException($"File name cannot start or end with a dot: {value}", parameterName);
        }

        var nameWithoutExtension = Path.GetFileNameWithoutExtension(value);
        if (ReservedDosNames.Contains(nameWithoutExtension))
        {
            throw new ArgumentException($"File name uses a reserved system name: {value}", parameterName);
        }

        return value;
    }

    private static string BuildSafePath(string root, string fileName, string parameterName)
    {
        var combined = Path.Combine(root, fileName);
        var rootFull = Path.GetFullPath(root);
        var combinedFull = Path.GetFullPath(combined);

        var rootWithSeparator = rootFull.EndsWith(Path.DirectorySeparatorChar)
            ? rootFull
            : rootFull + Path.DirectorySeparatorChar;

        if (!combinedFull.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Resolved path escapes the user-data root: {fileName}", parameterName);
        }

        return combinedFull;
    }
}
