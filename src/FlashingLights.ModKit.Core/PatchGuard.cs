using System.Reflection;
using HarmonyLib;

namespace FlashingLights.ModKit.Core;

public static class PatchGuard
{
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

        try
        {
            harmony.Patch(original, postfix: new HarmonyMethod(postfix));
            return true;
        }
        catch (Exception ex)
        {
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

        try
        {
            harmony.Patch(original, prefix: new HarmonyMethod(prefix));
            return true;
        }
        catch (Exception ex)
        {
            warn?.Invoke($"Patch failed for {targetType?.FullName}.{methodName}: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }
}
