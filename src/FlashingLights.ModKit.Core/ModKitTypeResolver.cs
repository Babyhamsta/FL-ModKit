using System.Collections.Concurrent;
using System.Reflection;

namespace FlashingLights.ModKit.Core;

public static class ModKitTypeResolver
{
    private static readonly ConcurrentDictionary<string, Type> FullNameHits = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, Type> SuffixHits = new(StringComparer.Ordinal);
    private static int subscribedAssemblyLoad;

    public static Type? ResolveFullName(string fullName, Action<string>? warn = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Type full name cannot be empty.", nameof(fullName));
        }

        if (FullNameHits.TryGetValue(fullName, out var cached))
        {
            return cached;
        }

        EnsureAssemblyLoadSubscription();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = GetLoadableType(assembly, fullName);
            if (type != null)
            {
                FullNameHits.TryAdd(fullName, type);
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

        if (SuffixHits.TryGetValue(suffix, out var cached))
        {
            return cached;
        }

        EnsureAssemblyLoadSubscription();
        var matches = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(GetLoadableTypes)
            .Where(type => type.FullName?.EndsWith(suffix, StringComparison.Ordinal) == true)
            .Distinct()
            .ToArray();

        if (matches.Length == 1)
        {
            SuffixHits.TryAdd(suffix, matches[0]);
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

    internal static void ClearCacheForTests()
    {
        FullNameHits.Clear();
        SuffixHits.Clear();
    }

    private static void EnsureAssemblyLoadSubscription()
    {
        if (System.Threading.Interlocked.Exchange(ref subscribedAssemblyLoad, 1) != 0)
        {
            return;
        }

        try
        {
            AppDomain.CurrentDomain.AssemblyLoad += (_, _) =>
            {
                // New assembly may resolve previous misses, but cached hits are still valid.
                // Misses aren't cached, so no invalidation is needed for fullname.
                // Suffix hits may become ambiguous; clear suffix cache to be safe.
                SuffixHits.Clear();
            };
        }
        catch
        {
            // If subscription fails, we keep working without invalidation. Misses retry every call.
        }
    }

    private static Type? GetLoadableType(Assembly assembly, string fullName)
    {
        try
        {
            return assembly.GetType(fullName, throwOnError: false);
        }
        catch
        {
            return null;
        }
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
