using System.Reflection;

namespace FlashingLights.ModKit.Core;

public static class TypeResolver
{
    public static Type? ResolveFullName(string fullName, Action<string>? warn = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Type full name cannot be empty.", nameof(fullName));
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(fullName, throwOnError: false);
            if (type != null)
            {
                return type;
            }
        }

        warn?.Invoke($"Type not found: {fullName}");
        return null;
    }

    public static Type? ResolveSuffix(string suffix, Action<string>? warn = null)
    {
        if (string.IsNullOrWhiteSpace(suffix))
        {
            throw new ArgumentException("Type suffix cannot be empty.", nameof(suffix));
        }

        var matches = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(GetLoadableTypes)
            .Where(type => type.FullName?.EndsWith(suffix, StringComparison.Ordinal) == true)
            .Distinct()
            .ToArray();

        if (matches.Length == 1)
        {
            return matches[0];
        }

        if (matches.Length == 0)
        {
            warn?.Invoke($"No type suffix match: {suffix}");
            return null;
        }

        warn?.Invoke($"Multiple type suffix matches for {suffix}: {string.Join(", ", matches.Select(type => type.FullName))}");
        return null;
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type != null)!;
        }
        catch
        {
            return Array.Empty<Type>();
        }
    }
}
