namespace FlashingLights.ModKit.Core;

public static class ModKitRegistry
{
    private static readonly object Gate = new();
    private static readonly List<IModKitManagedMod> Mods = [];
    private static readonly Dictionary<string, ModKitManifestValidationResult> Validations =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, ModKitManifestValidationResult?> SelfChecks =
        new(StringComparer.OrdinalIgnoreCase);
    private static string? uiOwnerId;

    public static IReadOnlyList<IModKitManagedMod> ManagedMods
    {
        get
        {
            lock (Gate)
            {
                return Mods.ToArray();
            }
        }
    }

    public static bool HasEnabledMods
    {
        get
        {
            lock (Gate)
            {
                return Mods.Any(mod => mod.IsEnabled);
            }
        }
    }

    public static void Register(IModKitManagedMod mod)
    {
        ArgumentNullException.ThrowIfNull(mod);

        if (string.IsNullOrWhiteSpace(mod.Metadata.Id))
        {
            throw new ArgumentException("Managed mod id cannot be empty.", nameof(mod));
        }

        lock (Gate)
        {
            Mods.RemoveAll(existing => existing.Metadata.Id.Equals(mod.Metadata.Id, StringComparison.OrdinalIgnoreCase));
            SelfChecks.Remove(mod.Metadata.Id);
            Mods.Add(mod);
            uiOwnerId ??= mod.Metadata.Id;
            RevalidateAllLocked();
        }
    }

    public static void Unregister(string modId)
    {
        if (string.IsNullOrWhiteSpace(modId))
        {
            return;
        }

        lock (Gate)
        {
            Mods.RemoveAll(existing => existing.Metadata.Id.Equals(modId, StringComparison.OrdinalIgnoreCase));
            SelfChecks.Remove(modId);
            if (uiOwnerId != null && uiOwnerId.Equals(modId, StringComparison.OrdinalIgnoreCase))
            {
                uiOwnerId = Mods.FirstOrDefault()?.Metadata.Id;
            }
            RevalidateAllLocked();
        }
    }

    public static bool TryGet(string modId, out IModKitManagedMod? mod)
    {
        lock (Gate)
        {
            mod = Mods.FirstOrDefault(existing => existing.Metadata.Id.Equals(modId, StringComparison.OrdinalIgnoreCase));
            return mod != null;
        }
    }

    public static bool IsUiOwner(string modId)
    {
        lock (Gate)
        {
            return uiOwnerId != null && uiOwnerId.Equals(modId, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static ModKitManifestValidationResult GetValidation(string modId)
    {
        if (string.IsNullOrWhiteSpace(modId))
        {
            return ModKitManifestValidationResult.Ok;
        }

        lock (Gate)
        {
            return GetValidationLocked(modId);
        }
    }

    public static bool IsBlockedByManifest(string modId) => !GetValidation(modId).IsValid;

    public static ModKitManifestValidationResult RefreshSelfCheck(string modId)
    {
        if (string.IsNullOrWhiteSpace(modId))
        {
            return ModKitManifestValidationResult.Ok;
        }

        lock (Gate)
        {
            var mod = Mods.FirstOrDefault(existing => existing.Metadata.Id.Equals(modId, StringComparison.OrdinalIgnoreCase));
            if (mod == null)
            {
                return ModKitManifestValidationResult.Ok;
            }

            SelfChecks[mod.Metadata.Id] = mod.SelfCheck();
            RevalidateAllLocked();
            return GetValidationLocked(mod.Metadata.Id);
        }
    }

    private static ModKitManifestValidationResult GetValidationLocked(string modId)
    {
        return Validations.TryGetValue(modId, out var result)
            ? result
            : ModKitManifestValidationResult.Ok;
    }

    private static void RevalidateAllLocked()
    {
        Validations.Clear();
        var snapshot = Mods.Select(m => m.Metadata).ToArray();
        foreach (var mod in Mods)
        {
            var manifestValidation = ModKitManifestValidator.Validate(
                mod.Metadata,
                SdkInfo.Version,
                snapshot);
            Validations[mod.Metadata.Id] = MergeValidation(manifestValidation, GetSelfCheckLocked(mod.Metadata.Id));
        }
    }

    private static ModKitManifestValidationResult? GetSelfCheckLocked(string modId)
    {
        return SelfChecks.TryGetValue(modId, out var result) ? result : null;
    }

    private static ModKitManifestValidationResult MergeValidation(
        ModKitManifestValidationResult manifestValidation,
        ModKitManifestValidationResult? selfCheck)
    {
        if (selfCheck == null || selfCheck.IsValid)
        {
            return manifestValidation;
        }

        if (manifestValidation.IsValid)
        {
            return selfCheck;
        }

        return ModKitManifestValidationResult.Failed(manifestValidation.Issues.Concat(selfCheck.Issues).ToArray());
    }

    internal static void ClearForTests()
    {
        lock (Gate)
        {
            Mods.Clear();
            Validations.Clear();
            SelfChecks.Clear();
            uiOwnerId = null;
        }
    }
}
