using MelonLoader;

namespace FlashingLights.ModKit.Sample;

public static class SamplePatches
{
    private static MelonLogger.Instance? logger;
    private static bool loggedAwake;

    public static void SetLogger(MelonLogger.Instance loggerInstance)
    {
        logger = loggerInstance;
    }

    public static void AIVehicleControllerAwakePostfix()
    {
        if (loggedAwake)
        {
            return;
        }

        loggedAwake = true;
        logger?.Msg("Observed AIVehicleController.Awake once.");
    }
}
