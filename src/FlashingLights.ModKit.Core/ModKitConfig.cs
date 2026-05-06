using System.Text.Json;
using System.Text.Json.Nodes;

namespace FlashingLights.ModKit.Core;

public static class ModKitConfig
{
    private const string VersionFieldName = "__configVersion";

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
        return LoadOrCreateWithVersion(modId, defaultConfig, baseDirectory, info, warn).Config;
    }

    public static ModKitConfigLoadResult<T> LoadOrCreateWithVersion<T>(
        string modId,
        T defaultConfig,
        string? baseDirectory = null,
        Action<string>? info = null,
        Action<string>? warn = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(defaultConfig);

        var targetVersion = GetTargetVersion(typeof(T));
        var path = GetConfigPath(modId, baseDirectory);
        ModKitPaths.EnsureParentDirectory(path);

        if (!File.Exists(path))
        {
            Save(modId, defaultConfig, baseDirectory, warn);
            info?.Invoke($"Created config: {path}");
            return new ModKitConfigLoadResult<T>(defaultConfig, targetVersion, targetVersion);
        }

        try
        {
            var json = File.ReadAllText(path);
            var loadedVersion = ReadLoadedVersion(json, targetVersion);
            var loaded = JsonSerializer.Deserialize<T>(json, JsonOptions);
            if (loaded == null)
            {
                warn?.Invoke($"Config was empty or null, using defaults: {path}");
                return new ModKitConfigLoadResult<T>(defaultConfig, targetVersion, targetVersion);
            }

            return new ModKitConfigLoadResult<T>(loaded, loadedVersion, targetVersion);
        }
        catch (JsonException ex)
        {
            warn?.Invoke($"Invalid config JSON, using defaults: {path}: {ex.Message}");
            return new ModKitConfigLoadResult<T>(defaultConfig, targetVersion, targetVersion);
        }
        catch (NotSupportedException ex)
        {
            warn?.Invoke($"Unsupported config JSON shape, using defaults: {path}: {ex.Message}");
            return new ModKitConfigLoadResult<T>(defaultConfig, targetVersion, targetVersion);
        }
        catch (IOException ex)
        {
            warn?.Invoke($"Could not read config, using defaults: {path}: {ex.Message}");
            return new ModKitConfigLoadResult<T>(defaultConfig, targetVersion, targetVersion);
        }
        catch (UnauthorizedAccessException ex)
        {
            warn?.Invoke($"Config read denied by OS, using defaults: {path}: {ex.Message}");
            return new ModKitConfigLoadResult<T>(defaultConfig, targetVersion, targetVersion);
        }
    }

    public static bool Save<T>(
        string modId,
        T config,
        string? baseDirectory = null,
        Action<string>? warn = null)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(config);

        var path = GetConfigPath(modId, baseDirectory);
        try
        {
            ModKitPaths.EnsureParentDirectory(path);
            var json = SerializeWithVersion(config);
            File.WriteAllText(path, json);
            return true;
        }
        catch (IOException ex)
        {
            warn?.Invoke($"Could not write config: {path}: {ex.Message}");
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            warn?.Invoke($"Config write denied by OS: {path}: {ex.Message}");
            return false;
        }
    }

    public static string GetConfigPath(string modId, string? baseDirectory = null)
    {
        return ModKitPaths.ConfigPath(modId, baseDirectory);
    }

    public static bool ShouldReload(DateTimeOffset? loadedWriteTime, DateTimeOffset? currentWriteTime)
    {
        return loadedWriteTime.HasValue
            && currentWriteTime.HasValue
            && currentWriteTime.Value > loadedWriteTime.Value;
    }

    private static int GetTargetVersion(Type configType)
    {
        return Attribute.GetCustomAttribute(configType, typeof(ModKitConfigVersionAttribute)) is ModKitConfigVersionAttribute attr
            ? attr.Version
            : 0;
    }

    private static int ReadLoadedVersion(string json, int targetVersion)
    {
        if (targetVersion == 0)
        {
            return 0;
        }

        var node = JsonNode.Parse(json);
        if (node is not JsonObject obj || !obj.TryGetPropertyValue(VersionFieldName, out var versionNode))
        {
            return 0;
        }

        try
        {
            return versionNode?.GetValue<int>() ?? 0;
        }
        catch (Exception ex) when (ex is FormatException or InvalidOperationException)
        {
            throw new JsonException($"Config field '{VersionFieldName}' must be an integer.", ex);
        }
    }

    private static string SerializeWithVersion<T>(T config)
        where T : class
    {
        var targetVersion = GetTargetVersion(config.GetType());
        if (targetVersion == 0)
        {
            return JsonSerializer.Serialize(config, JsonOptions);
        }

        var node = JsonSerializer.SerializeToNode(config, JsonOptions) as JsonObject ?? new JsonObject();
        var output = new JsonObject
        {
            [VersionFieldName] = targetVersion
        };

        foreach (var property in node)
        {
            if (string.Equals(property.Key, VersionFieldName, StringComparison.Ordinal))
            {
                continue;
            }

            output[property.Key] = property.Value == null
                ? null
                : JsonNode.Parse(property.Value.ToJsonString());
        }

        return output.ToJsonString(JsonOptions);
    }
}
