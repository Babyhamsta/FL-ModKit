namespace FlashingLights.ModKit.Core;

public sealed record ModKitDependency(string Id, Version? MinVersion, bool IsOptional = false);
