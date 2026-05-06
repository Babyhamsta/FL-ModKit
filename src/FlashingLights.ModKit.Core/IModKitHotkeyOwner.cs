using UnityEngine;

namespace FlashingLights.ModKit.Core;

public interface IModKitHotkeyOwner
{
    IReadOnlyList<HotkeyBinding>? GetHotkeyBindings();
    bool TryRebindHotkey(string name, KeyCode newKey);
}
