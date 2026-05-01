using Il2CppInterop.Runtime;
using UnityEngine;

namespace FlashingLights.ModKit.Core;

public sealed record SceneQueryResult(string TypeName, int Count, IReadOnlyList<string> Names);

public static class SceneQuery
{
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
}
