using System.Reflection;
using HarmonyLib;

namespace FlashingLights.ModKit.Core;

public static class PatchGuard
{
    private static readonly object PatchedGate = new();
    private static readonly HashSet<(string HarmonyId, MethodBase Original, PatchKind Kind)> PatchedTargets = new();

    public static MethodInfo? ResolveMethod(
        Type? targetType,
        string methodName,
        Type[]? parameters = null,
        Action<string>? warn = null)
    {
        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException("Method name cannot be empty.", nameof(methodName));
        }

        if (targetType == null)
        {
            warn?.Invoke($"Patch target type missing for method {methodName}.");
            return null;
        }

        var method = parameters == null
            ? AccessTools.Method(targetType, methodName)
            : AccessTools.Method(targetType, methodName, parameters);

        if (method == null)
        {
            warn?.Invoke($"Patch target method not found: {targetType.FullName}.{methodName}");
        }

        return method;
    }

    public static bool PatchPostfix(
        HarmonyLib.Harmony harmony,
        Type? targetType,
        string methodName,
        MethodInfo postfix,
        Action<string>? warn = null,
        Type[]? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(harmony);
        ArgumentNullException.ThrowIfNull(postfix);

        var original = ResolveMethod(targetType, methodName, parameters, warn);
        if (original == null)
        {
            return false;
        }

        if (!TryRegisterPatch(harmony.Id, original, PatchKind.Postfix, warn))
        {
            return false;
        }

        try
        {
            harmony.Patch(original, postfix: new HarmonyMethod(postfix));
            return true;
        }
        catch (Exception ex)
        {
            UnregisterPatch(harmony.Id, original, PatchKind.Postfix);
            warn?.Invoke($"Patch failed for {targetType?.FullName}.{methodName}: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    public static bool PatchPrefix(
        HarmonyLib.Harmony harmony,
        Type? targetType,
        string methodName,
        MethodInfo prefix,
        Action<string>? warn = null,
        Type[]? parameters = null)
    {
        ArgumentNullException.ThrowIfNull(harmony);
        ArgumentNullException.ThrowIfNull(prefix);

        var original = ResolveMethod(targetType, methodName, parameters, warn);
        if (original == null)
        {
            return false;
        }

        if (!TryRegisterPatch(harmony.Id, original, PatchKind.Prefix, warn))
        {
            return false;
        }

        try
        {
            harmony.Patch(original, prefix: new HarmonyMethod(prefix));
            return true;
        }
        catch (Exception ex)
        {
            UnregisterPatch(harmony.Id, original, PatchKind.Prefix);
            warn?.Invoke($"Patch failed for {targetType?.FullName}.{methodName}: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    internal static void ClearForTests()
    {
        lock (PatchedGate)
        {
            PatchedTargets.Clear();
        }
    }

    private static bool TryRegisterPatch(string harmonyId, MethodBase original, PatchKind kind, Action<string>? warn)
    {
        var key = (harmonyId, original, kind);
        lock (PatchedGate)
        {
            if (PatchedTargets.Contains(key))
            {
                warn?.Invoke($"Patch already installed for {original.DeclaringType?.FullName}.{original.Name} ({kind}); skipping duplicate.");
                return false;
            }
            PatchedTargets.Add(key);
            return true;
        }
    }

    private static void UnregisterPatch(string harmonyId, MethodBase original, PatchKind kind)
    {
        lock (PatchedGate)
        {
            PatchedTargets.Remove((harmonyId, original, kind));
        }
    }

    private enum PatchKind
    {
        Prefix,
        Postfix
    }
}
