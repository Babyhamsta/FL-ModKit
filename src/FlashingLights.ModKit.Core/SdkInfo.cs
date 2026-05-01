using MelonLoader;
using MelonLoader.InternalUtils;
using MelonLoader.Utils;

namespace FlashingLights.ModKit.Core;

public static class SdkInfo
{
    public const string Version = "0.1.0";

    public static void Log(Action<string> info)
    {
        ArgumentNullException.ThrowIfNull(info);

        info($"FlashingLights.ModKit.Core {Version}");
        info($"Game: {UnityInformationHandler.GameDeveloper}/{UnityInformationHandler.GameName} {UnityInformationHandler.GameVersion}");
        info($"Unity: {UnityInformationHandler.EngineVersion}");
        info($"IL2CPP: {MelonUtils.IsGameIl2Cpp()}");
        info($"Base directory: {MelonEnvironment.MelonBaseDirectory}");
        info($"UserData directory: {MelonEnvironment.UserDataDirectory}");
    }
}
