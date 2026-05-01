using Il2CppInterop.Runtime;
using UnityEngine;

namespace FlashingLights.ModKit.Core;

public sealed record SceneQueryResult(string TypeName, int Count, IReadOnlyList<string> Names);

public static class SceneQuery
{
    public static IReadOnlyList<string> FindObjectNamesContaining(
        string nameFragment,
        bool includeInactive = false,
        int maxNames = 25,
        Action<string>? warn = null)
    {
        ValidateNameQuery(nameFragment, maxNames);

        try
        {
            var inactiveMode = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
            var objects = UnityEngine.Object.FindObjectsByType(
                Il2CppType.From(typeof(UnityEngine.Object), throwOnFailure: false),
                inactiveMode,
                FindObjectsSortMode.None);
            var names = new List<string>();

            foreach (var obj in objects)
            {
                if (obj == null)
                {
                    continue;
                }

                names.Add(obj.name);
            }

            return FilterObjectNamesContaining(names, nameFragment, maxNames);
        }
        catch (Exception ex)
        {
            warn?.Invoke($"Scene name query failed for fragment '{nameFragment}': {ex.GetType().Name}: {ex.Message}");
            return Array.Empty<string>();
        }
    }

    public static SceneQueryResult FindObjectNamesByType(
        Type managedType,
        bool includeInactive = false,
        int maxNames = 25,
        Action<string>? warn = null)
    {
        ArgumentNullException.ThrowIfNull(managedType);

        if (maxNames < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxNames), "Max names cannot be negative.");
        }

        var il2CppType = Il2CppType.From(managedType, throwOnFailure: false);
        if (il2CppType == null)
        {
            warn?.Invoke($"Cannot convert managed type to IL2CPP type: {managedType.FullName}");
            return new SceneQueryResult(managedType.FullName ?? managedType.Name, 0, Array.Empty<string>());
        }

        try
        {
            var inactiveMode = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
            var objects = UnityEngine.Object.FindObjectsByType(il2CppType, inactiveMode, FindObjectsSortMode.None);
            var names = new List<string>();

            foreach (var obj in objects)
            {
                if (obj == null)
                {
                    continue;
                }

                if (names.Count < maxNames)
                {
                    names.Add(obj.name);
                }
            }

            return new SceneQueryResult(managedType.FullName ?? managedType.Name, objects.Length, names);
        }
        catch (Exception ex)
        {
            warn?.Invoke($"Scene query failed for {managedType.FullName}: {ex.GetType().Name}: {ex.Message}");
            return new SceneQueryResult(managedType.FullName ?? managedType.Name, 0, Array.Empty<string>());
        }
    }

    internal static IReadOnlyList<string> FilterObjectNamesContaining(
        IEnumerable<string?> objectNames,
        string nameFragment,
        int maxNames = 25)
    {
        ArgumentNullException.ThrowIfNull(objectNames);
        ValidateNameQuery(nameFragment, maxNames);

        if (maxNames == 0)
        {
            return Array.Empty<string>();
        }

        var matches = new List<string>();

        foreach (var name in objectNames)
        {
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            if (!name.Contains(nameFragment, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            matches.Add(name);
            if (matches.Count >= maxNames)
            {
                break;
            }
        }

        return matches;
    }

    private static void ValidateNameQuery(string nameFragment, int maxNames)
    {
        if (string.IsNullOrWhiteSpace(nameFragment))
        {
            throw new ArgumentException("Name fragment cannot be empty.", nameof(nameFragment));
        }

        if (maxNames < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxNames), "Max names cannot be negative.");
        }
    }
}
