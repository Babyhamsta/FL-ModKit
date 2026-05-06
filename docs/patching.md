# Patching

The SDK wraps Harmony with two helpers: `PatchGuard` for safe lookup-and-patch, and `PatchContext<TState>` for typed state-sharing between your mod and your patch class.

## Safe patching with PatchGuard

Resolve and patch a method without crashing the mod load when the type or method is missing:

```csharp
using HarmonyLib;
using FlashingLights.ModKit.Core;

protected override void OnModKitEnabled()
{
    var postfix = AccessTools.Method(typeof(MyPatches), nameof(MyPatches.OnFire));
    PatchPostfix("Il2CppEVP.VehicleController", "OnFire", postfix);
}
```

`PatchPostfix` and `PatchPrefix` on the base class resolve the type via `ModKitTypeResolver.ResolveFullName`, look up the method, log a single warning if either step fails, and return `false`. Your mod stays loaded, the patch just isn't installed.

For more control (different Harmony instance, multiple parameter overloads), call `PatchGuard` directly:

```csharp
PatchGuard.PatchPostfix(
    HarmonyInstance,
    ResolveType("Il2CppFoo.Bar"),
    "MethodName",
    postfix,
    LogWarning,
    parameters: new[] { typeof(int), typeof(string) });
```

Pass the explicit `parameters` array when the target type has overloads.

## Sharing state with PatchContext

Harmony patch methods are static. The legacy pattern was a static field on the patch class plus an `internal static SetX(...)` method called from the mod's `OnModKitEnabled`. That's now `PatchContext<TState>`:

```csharp
internal sealed class MyPatchState
{
    public required ModKitFileLog Log;
    public required MyConfig Config;
}

protected override void OnModKitEnabled()
{
    var state = new MyPatchState { Log = log, Config = Config };
    PatchContext<MyPatchState>.Set(state);
    PatchContext<MyPatchState>.SetActive(true);

    var postfix = AccessTools.Method(typeof(MyPatches), nameof(MyPatches.Postfix));
    PatchPostfix("Il2CppEVP.VehicleController", "OnFire", postfix);
}

protected override void OnModKitDisabled()
{
    PatchContext<MyPatchState>.SetActive(false);
}

public static class MyPatches
{
    public static void Postfix(VehicleController __instance)
    {
        if (!PatchContext<MyPatchState>.IsActive) return;
        var state = PatchContext<MyPatchState>.State!;
        state.Log.Info(ModKitLogFormat.KeyValue("event", "fire"));
    }
}
```

Each closed generic (one per `TState` type) gets its own static slot in the CLR — two mods using `PatchContext<MyPatchState>` with different `MyPatchState` classes will not collide. This works because each mod loads its own assembly with its own `MyPatchState` type.

`IsActive` returns `false` until both `Set(value)` and `SetActive(true)` have run. `Clear()` nulls the state and deactivates atomically — useful in `OnApplicationQuit` style cleanup.

## When not to patch

If you're hooking a `Globals` event, vehicle event, save event, day/night transition, or pool spawn lifecycle, please wait for the v1.0 `FlashingLights.ModKit.Game` assembly that wraps these as plain C# events. Direct Harmony patches against those targets will need rework when the wrapper layer ships.

For everything else (custom game systems, niche PlayMaker actions), Harmony with `PatchGuard` + `PatchContext` is the right call.

## Type discovery

`ModKitTypeResolver.ResolveFullName` walks loaded assemblies. Cache the resolved type in `OnModKitInitialized` rather than re-resolving on every patch. The resolver emits a single warning when it can't find a name; if you don't want the warning, pass a no-op `Action<string>` instead of `LogWarning`.

For exploratory work, `ModKitTypeResolver.ResolveSuffix` matches by class-name suffix. Don't ship code that depends on suffix matches — names can collide.

## Game type catalog

Core does not maintain a list of game-specific type names — those drift between game updates and are the v1.0 `Game` assembly's responsibility. Until then, mods own the type-name strings they patch and update them when the game updates.
