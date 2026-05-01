using System.Text.Json;
using MelonLoader;
using MelonLoader.Utils;

namespace FlashingLights.ModKit.Core;

public static class ModConfig
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static T LoadOrCreate<T>(
        string modId,
        T defaultConfig,
        string? baseDirectory = null,
        Action<string>? info = null,
        Action<string>? warn = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(defaultConfig);

        var path = GetConfigPath(modId, baseDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        if (!File.Exists(path))
        {
            Save(modId, defaultConfig, baseDirectory);
            info?.Invoke($"Created config: {path}");
            return defaultConfig;
        }

        try
        {
            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<T>(json, JsonOptions);
            if (loaded == null)
            {
                warn?.Invoke($"Config was empty or null, using defaults: {path}");
                return defaultConfig;
            }

            return loaded;
        }
        catch (JsonException ex)
        {
            warn?.Invoke($"Invalid config JSON, using defaults: {path}: {ex.Message}");
            return defaultConfig;
        }
        catch (IOException ex)
        {
            warn?.Invoke($"Could not read config, using defaults: {path}: {ex.Message}");
            return defaultConfig;
        }
    }

    public static void Save<T>(string modId, T config, string? baseDirectory = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(config);

        var path = GetConfigPath(modId, baseDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(path, json);
    }

    public static string GetConfigPath(string modId, string? baseDirectory = null)
    {
        var safeModId = ValidateModId(modId);
        var root = string.IsNullOrWhiteSpace(baseDirectory)
            ? Path.Combine(MelonEnvironment.UserDataDirectory, "FlashingLightsModKit")
            : baseDirectory;

        return Path.Combine(root, $"{safeModId}.json");
    }

    private static string ValidateModId(string modId)
    {
        if (string.IsNullOrWhiteSpace(modId))
        {
            throw new ArgumentException("Mod id cannot be empty.", nameof(modId));
        }

        if (modId.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new ArgumentException($"Mod id contains invalid file-name characters: {modId}", nameof(modId));
        }

        return modId;
    }
}
