using UnityEngine;

namespace FlashingLights.ModKit.Core;

internal static class ModKitUiInputPolicy
{
    public const int DeferredCursorRestoreFrames = 4;
    public const int OverlayGuiDepth = -10000;

    public static bool ShouldBlockEvent(EventType eventType)
    {
        return eventType switch
        {
            EventType.MouseDown => true,
            EventType.MouseUp => true,
            EventType.MouseMove => true,
            EventType.MouseDrag => true,
            EventType.ScrollWheel => true,
            EventType.KeyDown => true,
            EventType.KeyUp => true,
            _ => false
        };
    }

    public static bool ShouldCloseOverlay(EventType eventType, KeyCode keyCode)
    {
        return eventType == EventType.KeyDown
            && (keyCode == KeyCode.Insert || keyCode == KeyCode.Escape);
    }

    public static void SuppressGameInput(Action<string>? warn = null)
    {
        try
        {
            Input.ResetInputAxes();
        }
        catch (Exception ex)
        {
            warn?.Invoke($"ModKit UI input suppression failed: {ex.GetType().Name}: {ex.Message}");
        }
    }
}
