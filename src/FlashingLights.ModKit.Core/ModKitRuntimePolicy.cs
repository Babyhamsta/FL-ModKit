namespace FlashingLights.ModKit.Core;

public static class ModKitRuntimePolicy
{
    public static bool ShouldRun(bool isEnabled, bool blockedByManifest)
    {
        return isEnabled && !blockedByManifest;
    }
}
