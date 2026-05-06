namespace FlashingLights.ModKit.Core;

public sealed record ModKitConfigLoadResult<TConfig>(
    TConfig Config,
    int LoadedVersion,
    int TargetVersion)
    where TConfig : class;
