using UnityEngine;

namespace FlashingLights.ModKit.Core;

public sealed record HotkeyBinding(string Name, KeyCode Key, string DisplayName);
