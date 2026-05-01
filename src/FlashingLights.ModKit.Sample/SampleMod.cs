using FlashingLights.ModKit.Core;
using HarmonyLib;
using MelonLoader;

namespace FlashingLights.ModKit.Sample;

public sealed class SampleMod : MelonMod
{
    private const int MinLoggedObjectNames = 0;
    private const int MaxLoggedObjectNamesLimit = 50;

    private SampleConfig config = new();

    private int MaxLoggedObjectNames => Math.Clamp(
        config.MaxLoggedObjectNames,
        MinLoggedObjectNames,
        MaxLoggedObjectNamesLimit);

    public override void OnInitializeMelon()
    {
        config = ModConfig.LoadOrCreate(
            "sample",
            new SampleConfig(),
            info: LoggerInstance.Msg,
            warn: LoggerInstance.Warning);

        SdkInfo.Log(LoggerInstance.Msg);
        ResolveKnownTypes();

        if (config.EnableHarmlessPatch)
        {
            RegisterPatches();
        }
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (!config.EnableSceneScan)
        {
            return;
        }

        LoggerInstance.Msg($"Scene loaded: {sceneName} ({buildIndex})");
        LogSceneQuery(GameTypes.OmniAiVehicleController);
        LogSceneQuery(GameTypes.EvpVehicleController);
        LogSceneQuery(GameTypes.SimpleCarController);
    }

    private void ResolveKnownTypes()
    {
        foreach (var typeName in GameTypes.AllKnownTypeNames)
        {
            var type = TypeResolver.ResolveFullName(typeName, LoggerInstance.Warning);
            LoggerInstance.Msg(type == null ? $"Missing type: {typeName}" : $"Found type: {typeName}");
        }
    }

    private void LogSceneQuery(string typeName)
    {
        var type = TypeResolver.ResolveFullName(typeName, LoggerInstance.Warning);
        if (type == null)
        {
            return;
        }

        var result = SceneQuery.FindObjectNamesByType(
            type,
            includeInactive: false,
            maxNames: MaxLoggedObjectNames,
            warn: LoggerInstance.Warning);

        LoggerInstance.Msg($"{result.TypeName}: {result.Count} active objects");

        foreach (var objectName in result.Names)
        {
            LoggerInstance.Msg($"  - {objectName}");
        }
    }

    private void RegisterPatches()
    {
        SamplePatches.SetLogger(LoggerInstance);

        var targetType = TypeResolver.ResolveFullName(GameTypes.OmniAiVehicleController, LoggerInstance.Warning);
        var postfix = AccessTools.Method(typeof(SamplePatches), nameof(SamplePatches.AIVehicleControllerAwakePostfix));

        if (postfix == null)
        {
            LoggerInstance.Warning("Sample postfix method was not found.");
            return;
        }

        var patched = PatchGuard.PatchPostfix(
            HarmonyInstance,
            targetType,
            "Awake",
            postfix,
            LoggerInstance.Warning);

        LoggerInstance.Msg(patched ? "Sample observation patch registered." : "Sample observation patch skipped.");
    }
}
