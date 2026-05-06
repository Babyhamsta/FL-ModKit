using UnityEngine;

namespace FlashingLights.ModKit.Core;

public interface IModKitManagedMod : IModKitHotkeyOwner
{
    ModKitModMetadata Metadata { get; }
    bool IsEnabled { get; }
    string ConfigPath { get; }
    IModKitConfigAdapter? ConfigAdapter { get; }
    void SetEnabled(bool enabled);

    IReadOnlyList<HotkeyBinding>? IModKitHotkeyOwner.GetHotkeyBindings() => null;

    bool IModKitHotkeyOwner.TryRebindHotkey(string name, KeyCode newKey) => false;

    ModKitManifestValidationResult? SelfCheck() => null;
}
