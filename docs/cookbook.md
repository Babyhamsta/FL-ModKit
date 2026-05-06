# Cookbook

Short recipes for the things every mod needs.

## How do I declare metadata in one place?

Use `[ModKitManifest]` on the mod class. Skip the virtual property overrides.

```csharp
[ModKitManifest(
    Id = "myname.mymod",
    DisplayName = "My Mod",
    Version = "1.0.0",
    Author = "Babyhamsta",
    License = "MIT",
    Category = "Gameplay",
    Tags = new[] { "traffic", "single-player" },
    GitHubUrl = "https://github.com/Babyhamsta/FL-ModKit",
    Changelog = "Initial public release.",
    MinSdkVersion = "0.1.0")]
public sealed class MyMod : ModKitMelonMod<MyConfig>
{
    protected override string ModId => "myname.mymod";
}
```

`ModId` still has to come from a property override because the framework needs it before `Metadata` is resolved (used for the config path). Everything else can live in the attribute.

## How do I add a config option that shows up in the UI?

Public property with a setter on the config class. Decorate with one of the metadata attributes for nicer rendering:

```csharp
public sealed class MyConfig
{
    public bool Enabled { get; set; } = true;

    [ModKitConfigDisplay("Spawn distance (m)")]
    [ModKitConfigRange(50, 500, 10)]
    public int SpawnDistance { get; set; } = 200;

    [ModKitConfigOptions("Easy", "Normal", "Hard")]
    public string Difficulty { get; set; } = "Normal";

    public string[] Tags { get; set; } = Array.Empty<string>();
}
```

Editable types: `bool`, integer numerics, floating-point numerics, `string`, enums, and `string[]`. Anything else is read-only-rendered or skipped.

## How do I react to config changes?

Override `OnConfigApplied`. It's called for every reload and every UI edit:

```csharp
protected override void OnConfigApplied(MyConfig currentConfig)
{
    spawnDistance = currentConfig.SpawnDistance;
    spawnSink?.UpdateThreshold(currentConfig.SpawnDistance);
}
```

Use `OnConfigReloaded(prev, current)` only when you need the diff (e.g., "skip expensive recompute if the field I care about didn't change").

## How do I declare a dependency on another mod?

Add a `Dependencies` array to the manifest. Use `mod.id>=X.Y.Z` to require a minimum version, or just `mod.id` for any version:

```csharp
[ModKitManifest(
    Id = "myname.featuremod",
    Dependencies = new[] { "babyhamsta.modkit-core>=0.1.0", "third-party.helper" })]
```

The SDK refuses to call `OnModKitEnabled` until all dependencies are registered. The mod stays visible in the in-game UI with a "Blocked" status so the player can see why it's not running.

Append `?` for optional integrations:

```csharp
[ModKitManifest(Dependencies = new[] { "third-party.helper?", "other.mod>=2.0.0?" })]
```

An optional dependency may be absent. If it is present and below the requested version, validation still blocks the mod.

## How do I patch a game method safely?

Use `PatchPostfix` / `PatchPrefix` from the base class. They resolve the method, log a single warning if it doesn't exist, and skip the patch instead of crashing the load:

```csharp
protected override void OnModKitEnabled()
{
    var postfix = AccessTools.Method(typeof(MyPatches), nameof(MyPatches.OnFire));
    PatchPostfix("Il2CppEVP.VehicleController", "OnFire", postfix);
}
```

For more complex Harmony work, use `PatchGuard` directly with the mod's `HarmonyInstance`.

## How do I share state with my Harmony patches?

Define a state class. Use `PatchContext<TState>` instead of static fields with bespoke setter methods:

```csharp
internal sealed class MyPatchState
{
    public required ModKitFileLog Log;
    public required MyConfig Config;
}

protected override void OnModKitEnabled()
{
    PatchContext<MyPatchState>.Set(new MyPatchState { Log = log, Config = Config });
    PatchContext<MyPatchState>.SetActive(true);
    InstallPatches();
}

protected override void OnModKitDisabled()
{
    PatchContext<MyPatchState>.SetActive(false);
}

// In the patch class:
public static void Postfix(VehicleController __instance)
{
    if (!PatchContext<MyPatchState>.IsActive) return;
    var state = PatchContext<MyPatchState>.State!;
    state.Log.Info(ModKitLogFormat.KeyValue("event", "fire"));
}
```

`IsActive` is `false` until `Set` + `SetActive(true)` are both called, so a half-initialized mod's patches simply no-op.

## How do I write to a per-mod log file?

`ModKitFileLog` handles the path, locking, and graceful fallback when writes fail:

```csharp
ModKitFileLog log = null!;

protected override void OnModKitInitialized()
{
    log = new ModKitFileLog(Metadata.Id,
        info: LogInfo,
        warn: LogWarning);
    log.Header($"# {Metadata.DisplayName} v{Metadata.Version} {DateTimeOffset.UtcNow:O}");
}

protected override void OnModKitUpdate()
{
    if (somethingHappened)
    {
        log.Info(ModKitLogFormat.KeyValue("event", "tick"));
    }
}
```

The file lands at `UserData\FlashingLightsModKit\<mod-id>.log`. Toggle `log.LogToConsole` and `log.LogToFile` from your config if you want player-controllable logging.

Use `LogDebug("message")` from `ModKitMelonMod<TConfig>` for verbose details. It writes to the mod's `FileLog` when configured and only mirrors to console when the config has `public bool VerboseLogging { get; set; } = true;`.

## How do I register a hotkey?

Construct one `Hotkeys` per mod, return it from the protected `Hotkeys` property, and register bindings as needed:

```csharp
private readonly Hotkeys hotkeys = new(warn: msg => LogWarning(msg));

protected override Hotkeys? Hotkeys => hotkeys;

protected override void OnModKitInitialized()
{
    hotkeys.Register($"{Metadata.Id}.toggle", KeyCode.F8,
        () => SetEnabled(!IsEnabled),
        displayName: "Toggle Mod");
}
```

Returning the instance from the protected `Hotkeys` property lets the Config tab show and rebind the binding. The base class drives `Hotkeys.Update()` for you when the mod is enabled.

For a config-driven binding, read the keycode out of `Config` and re-register on `OnConfigApplied`:

```csharp
protected override void OnConfigApplied(MyConfig current)
{
    hotkeys.Unregister($"{Metadata.Id}.toggle");
    hotkeys.Register($"{Metadata.Id}.toggle", current.ToggleKey,
        () => SetEnabled(!IsEnabled));
}
```

## How do I find a GameObject by name?

`SceneQuery` enumerates the active scene. Useful for diagnostics and discovery:

```csharp
var matches = SceneQuery.FindObjectNamesContaining("PoliceCruiser",
    ignoreCase: true, maxNames: 10, warn: LogWarning);

foreach (var match in matches)
{
    LogInfo($"Found {match.Name}");
}
```

In shipping code, prefer caching a reference at scene-load time over scanning every frame.

## How do I find a type from the IL2CPP assemblies?

`ModKitTypeResolver.ResolveFullName` walks loaded assemblies for the full name. The base class provides a one-liner shortcut:

```csharp
var vehicleType = ResolveType("Il2CppEVP.VehicleController");
if (vehicleType != null)
{
    // ...
}
```

Use the suffix variant only during exploratory work — full names won't drift unexpectedly between game updates as much as suffix matches will.

## How do I snapshot object state for diagnostics?

Use `ModKitObjectSnapshot` with explicit member names:

```csharp
LogDebug(ModKitObjectSnapshot.Capture(aiController,
    "AiMode",
    "speed",
    "target",
    "vehicleRigidbody"));
```

The helper formats primitive values, Unity vectors, GameObjects, Components, and Rigidbody state into structured log text. Keep the member list in your mod so Core stays generic.

## How do I gate work on multiplayer state?

Don't, the SDK does it for you. While any managed mod is enabled:

- The main menu shows a "Mods enabled — online play disabled" banner.
- Photon room create/join/rejoin operations are refused at the source.
- Online play UI elements are disabled where the visual scanner can find them.

If you need the inverse (only run when offline) the framework already enforces this — your `OnModKitEnabled` will not fire while the player is in an online room. If you need finer control, query `ModKitMultiplayerPolicy.ShouldBlockOnline(...)` directly.

## How do I unit-test against the SDK?

The Core test app at `tests\FlashingLights.ModKit.Core.Tests\Program.cs` is a console app — no xunit or NUnit. Add a test by appending a `(Name, Body)` tuple to the `tests` array and writing a static method that throws on failure. See existing tests for the pattern.

The SDK zip also includes `templates\ModTests\`, a small console harness you can copy into a mod repo. It starts with config round-trip and manifest-validation examples.

To run only the SDK tests during development:

```powershell
dotnet run --project .\tests\FlashingLights.ModKit.Core.Tests\FlashingLights.ModKit.Core.Tests.csproj -c Release
```

The harness exits with code 0 on full pass, 1 on any failure.
