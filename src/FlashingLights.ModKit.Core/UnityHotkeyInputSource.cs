using UnityEngine;

namespace FlashingLights.ModKit.Core;

internal sealed class UnityHotkeyInputSource : IHotkeyInputSource
{
    public static UnityHotkeyInputSource Instance { get; } = new();

    public bool GetKeyDown(KeyCode key) => Input.GetKeyDown(key);
}
