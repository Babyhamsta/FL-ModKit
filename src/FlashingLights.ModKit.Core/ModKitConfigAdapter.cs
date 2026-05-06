using System.Globalization;
using System.Reflection;

namespace FlashingLights.ModKit.Core;

public sealed class ModKitConfigAdapter<TConfig> : IModKitConfigAdapter
    where TConfig : class, new()
{
    private static readonly BindingFlags PublicInstance = BindingFlags.Instance | BindingFlags.Public;

    private readonly Func<TConfig> getConfig;
    private readonly Action<TConfig> applyConfig;
    private readonly string? baseDirectory;
    private readonly Func<TConfig> createDefault;
    private readonly Action<TConfig>? onChanged;
    private readonly Action<string>? warn;

    public ModKitConfigAdapter(
        string modId,
        Func<TConfig> getConfig,
        Action<TConfig> applyConfig,
        string? baseDirectory = null,
        Func<TConfig>? createDefault = null,
        Action<TConfig>? onChanged = null,
        Action<string>? warn = null)
    {
        if (string.IsNullOrWhiteSpace(modId))
        {
            throw new ArgumentException("Mod id cannot be empty.", nameof(modId));
        }

        ModId = modId;
        this.getConfig = getConfig ?? throw new ArgumentNullException(nameof(getConfig));
        this.applyConfig = applyConfig ?? throw new ArgumentNullException(nameof(applyConfig));
        this.baseDirectory = baseDirectory;
        this.createDefault = createDefault ?? (() => new TConfig());
        this.onChanged = onChanged;
        this.warn = warn;
    }

    public string ModId { get; }

    public string ConfigPath => ModKitConfig.GetConfigPath(ModId, baseDirectory);

    public IReadOnlyList<ModKitConfigProperty> GetProperties()
    {
        var config = getConfig();
        var properties = new List<ModKitConfigProperty>();

        foreach (var property in typeof(TConfig).GetProperties(PublicInstance))
        {
            if (property.GetIndexParameters().Length > 0 || property.GetMethod == null)
            {
                continue;
            }

            var kind = GetKind(property.PropertyType);
            if (kind == ModKitConfigValueKind.Unsupported)
            {
                continue;
            }

            var options = GetOptions(property);
            var value = property.GetValue(config);
            var range = GetRange(property, kind);

            properties.Add(new ModKitConfigProperty(
                property.Name,
                GetDisplayName(property),
                kind,
                property.PropertyType.Name,
                FormatValue(value, kind),
                property.SetMethod != null,
                options,
                range?.Minimum,
                range?.Maximum,
                range?.Step));
        }

        return properties;
    }

    public bool TrySetProperty(string propertyName, string valueText, out string error)
    {
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            error = "Property name cannot be empty.";
            return false;
        }

        var property = typeof(TConfig).GetProperty(propertyName, PublicInstance);
        if (property == null || property.GetIndexParameters().Length > 0)
        {
            error = $"Config property not found: {propertyName}";
            return false;
        }

        if (property.SetMethod == null)
        {
            error = $"Config property is read-only: {propertyName}";
            return false;
        }

        if (!TryParseValue(property.PropertyType, valueText, out var parsed, out error))
        {
            error = $"{propertyName}: {error}";
            return false;
        }

        var config = getConfig();
        property.SetValue(config, parsed);
        onChanged?.Invoke(config);
        return true;
    }

    public void Save()
    {
        ModKitConfig.Save(ModId, getConfig(), baseDirectory, warn);
    }

    public void Reload()
    {
        var loaded = ModKitConfig.LoadOrCreate(ModId, createDefault(), baseDirectory, warn: warn);
        applyConfig(loaded);
    }

    public void ResetDefaults()
    {
        applyConfig(createDefault());
        Save();
    }

    private static ModKitConfigValueKind GetKind(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type == typeof(bool))
        {
            return ModKitConfigValueKind.Boolean;
        }

        if (type == typeof(byte)
            || type == typeof(short)
            || type == typeof(int)
            || type == typeof(long)
            || type == typeof(uint)
            || type == typeof(ulong))
        {
            return ModKitConfigValueKind.Integer;
        }

        if (type == typeof(float)
            || type == typeof(double)
            || type == typeof(decimal))
        {
            return ModKitConfigValueKind.Float;
        }

        if (type == typeof(string))
        {
            return ModKitConfigValueKind.String;
        }

        if (type.IsEnum)
        {
            return ModKitConfigValueKind.Enum;
        }

        if (type == typeof(string[]))
        {
            return ModKitConfigValueKind.StringArray;
        }

        return ModKitConfigValueKind.Unsupported;
    }

    private static string FormatValue(object? value, ModKitConfigValueKind kind)
    {
        if (value == null)
        {
            return string.Empty;
        }

        return kind switch
        {
            ModKitConfigValueKind.Boolean => Convert.ToBoolean(value, CultureInfo.InvariantCulture).ToString().ToLowerInvariant(),
            ModKitConfigValueKind.Integer => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
            ModKitConfigValueKind.Float => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
            ModKitConfigValueKind.String => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
            ModKitConfigValueKind.Enum => value.ToString() ?? string.Empty,
            ModKitConfigValueKind.StringArray => string.Join("\n", ((string?[])value).Select(s => s ?? string.Empty)),
            _ => string.Empty
        };
    }

    private static bool TryParseValue(Type propertyType, string valueText, out object? value, out string error)
    {
        error = string.Empty;
        value = null;

        var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        try
        {
            if (targetType == typeof(bool))
            {
                if (!bool.TryParse(valueText, out var parsedBool))
                {
                    error = $"Expected true or false, got '{valueText}'.";
                    return false;
                }

                value = parsedBool;
                return true;
            }

            if (targetType.IsEnum)
            {
                value = Enum.Parse(targetType, valueText, ignoreCase: true);
                return true;
            }

            if (targetType == typeof(string))
            {
                value = valueText;
                return true;
            }

            if (targetType == typeof(string[]))
            {
                value = valueText
                    .Replace("\r\n", "\n", StringComparison.Ordinal)
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                return true;
            }

            value = Convert.ChangeType(valueText, targetType, CultureInfo.InvariantCulture);
            return true;
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException or ArgumentException)
        {
            error = $"Could not parse '{valueText}' as {targetType.Name}.";
            return false;
        }
    }

    private static ModKitConfigRangeAttribute? GetRange(PropertyInfo property, ModKitConfigValueKind kind)
    {
        if (kind != ModKitConfigValueKind.Integer && kind != ModKitConfigValueKind.Float)
        {
            return null;
        }

        return property.GetCustomAttribute<ModKitConfigRangeAttribute>();
    }

    private static IReadOnlyList<string> GetOptions(PropertyInfo property)
    {
        if (property.PropertyType.IsEnum)
        {
            return Enum.GetNames(property.PropertyType);
        }

        return property.GetCustomAttribute<ModKitConfigOptionsAttribute>()?.Options
            ?? Array.Empty<string>();
    }

    private static string GetDisplayName(PropertyInfo property)
    {
        return property.GetCustomAttribute<ModKitConfigDisplayAttribute>()?.DisplayName
            ?? ToDisplayName(property.Name);
    }

    private static string ToDisplayName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        var chars = new List<char>(name.Length + 8) { name[0] };
        for (var i = 1; i < name.Length; i++)
        {
            if (char.IsUpper(name[i]) && !char.IsWhiteSpace(name[i - 1]))
            {
                chars.Add(' ');
            }

            chars.Add(name[i]);
        }

        return new string(chars.ToArray());
    }
}
