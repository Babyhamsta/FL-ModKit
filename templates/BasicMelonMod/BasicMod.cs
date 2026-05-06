using FlashingLights.ModKit.Core;

namespace BasicMelonMod;

[ModKitManifest(
    Id = "basic-melon-mod",
    DisplayName = "Basic Melon Mod",
    Version = "0.1.0",
    Author = "Babyhamsta",
    License = "MIT",
    Changelog = "Initial Basic Melon Mod template.",
    MinSdkVersion = "0.1.0")]
public sealed class BasicMod : ModKitMelonMod<BasicConfig>
{
    protected override string ModId => "basic-melon-mod";

    protected override bool EnableConfigHotReload => Config.EnableHotReload;

    protected override TimeSpan ConfigReloadInterval =>
        TimeSpan.FromSeconds(Math.Clamp(Config.HotReloadIntervalSeconds, 0.25, 30));

    protected override void OnModKitInitialized()
    {
        LogInfo($"{Config.DisplayName} initialized.");
    }

    protected override void OnModKitEnabled()
    {
        LogInfo($"{Config.DisplayName} enabled.");
    }

    protected override void OnModKitDisabled()
    {
        LogInfo($"{Config.DisplayName} disabled.");
    }

    protected override void OnModKitSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (!Config.LogSceneLoads)
        {
            return;
        }

        LogInfo($"Scene loaded: index={buildIndex} name={sceneName}");
    }

    protected override void OnConfigApplied(BasicConfig currentConfig)
    {
        LogInfo($"Config applied: enabled={currentConfig.Enabled} sceneLogs={currentConfig.LogSceneLoads}");
    }

    protected override void OnConfigReloaded(BasicConfig previousConfig, BasicConfig currentConfig)
    {
        LogInfo($"Config reloaded: enabled={currentConfig.Enabled} sceneLogs={currentConfig.LogSceneLoads}");
    }
}
