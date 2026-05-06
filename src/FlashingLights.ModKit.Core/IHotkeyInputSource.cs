using UnityEngine;

namespace FlashingLights.ModKit.Core;

public interface IHotkeyInputSource
{
    bool GetKeyDown(KeyCode key);
}
