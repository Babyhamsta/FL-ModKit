# API Reference

Public surface of `FlashingLights.ModKit.Core` v0.1.0. Everything here is a stable contract — additive changes only between minor versions, see [SDK versioning](sdk-versioning.md).

## Base mod

### `ModKitMelonMod<TConfig>`

Abstract base for ModKit-managed mods. Inherit, override `ModId`, decorate with `[ModKitManifest]`, and the SDK handles config, UI, lifecycle, and multiplayer safety.

| Member | Description |
|--------|-------------|
| `protected abstract string ModId { get; }` | Required. Identity used for config path, registry, and UI keys. |
| `protected virtual string DisplayName { get; }` | Defaults to `GetType().Name`. Overridden by `[ModKitManifest(DisplayName = ...)]` when present. |
| `protected virtual string ModVersion { get; }` | Defaults to `"0.1.0"`. Overridden by manifest. |
| `protected virtual string ModAuthor { get; }` | Defaults to `"Babyhamsta"`. Overridden by manifest. |
| `protected virtual string? GitHubUrl { get; }` | Defaults to the SDK GitHub URL. Overridden by manifest. |
| `protected virtual string? Changelog { get; }` | Free-form per-version notes shown in the UI. Overridden by manifest. |
| `protected virtual bool RegisterWithModKitUi { get; }` | Default `true`. Set to `false` for console-only or worker mods that should not appear in the overlay. |
| `protected virtual bool EnableConfigHotReload { get; }` | Default `false`. Set to `true` to re-read the JSON file when its mtime advances. |
| `protected virtual TimeSpan ConfigReloadInterval { get; }` | Default 1s. Override to throttle hot-reload checks. |
| `public bool IsEnabled { get; }` | The user-controlled enabled flag, mirrored from `Config.Enabled` if present. |
| `public ModKitModMetadata Metadata { get; }` | Resolved manifest data. Lazy. |
| `public IModKitConfigAdapter? ConfigAdapter { get; }` | UI binding adapter for the current config object. |
| `protected TConfig Config { get; }` | The currently active config object. Replaced on reload. |
| `protected string ConfigPath { get; }` | Absolute path to the JSON config. |
| `public void SetEnabled(bool enabled)` | Toggle the mod. Refuses when multiplayer guard is active or manifest validation failed. |
| `public TConfig ReloadConfig()` | Force re-read from disk. Fires `OnConfigApplied`. |
| `protected void LogInfo(string message)` | Writes to MelonLoader console at info level. |
| `protected void LogWarning(string message)` | Writes to MelonLoader console at warning level. |
| `protected Type? ResolveType(string fullName)` | Convenience for `ModKitTypeResolver.ResolveFullName(...)` with `LogWarning` as the warn sink. |
| `protected bool PatchPostfix(string typeName, string method, MethodInfo postfix, Type[]? parameters = null)` | Convenience for `PatchGuard.PatchPostfix(HarmonyInstance, ...)`. |
| `protected bool PatchPrefix(string typeName, string method, MethodInfo prefix, Type[]? parameters = null)` | Convenience for `PatchGuard.PatchPrefix(HarmonyInstance, ...)`. |

#### Override hooks

| Hook | Default | When it fires |
|------|---------|---------------|
| `OnModKitInitialized()` | no-op | Once, after registry registration. |
| `OnModKitEnabled()` | no-op | On every transition into runtime-active state (`IsEnabled` and manifest validation passed). |
| `OnModKitDisabled()` | no-op | On every transition out of runtime-active state. |
| `OnModKitUpdate()` | no-op | Per frame, only while runtime-active. |
| `OnModKitGui()` | no-op | Per OnGUI pass, only while runtime-active. |
| `OnModKitSceneWasLoaded(int buildIndex, string sceneName)` | no-op | Scene-load hook, only while runtime-active. |
| `OnConfigApplied(TConfig current)` | no-op | After any config change. |
| `OnConfigReloaded(TConfig prev, TConfig current)` | no-op | After a full config replacement (hot reload, manual reload, reset defaults). |

See [lifecycle.md](lifecycle.md) for the full ordering diagram.

## Manifest

### `ModKitManifestAttribute`

Class-level attribute that supplies metadata without requiring virtual property overrides.

```csharp
[ModKitManifest(
    Id = "myname.mymod",
    DisplayName = "My Mod",
    Version = "1.0.0",
    Author = "Babyhamsta",
    License = "MIT",
    GitHubUrl = "https://github.com/Babyhamsta/FL-ModKit",
    Changelog = "Initial public release.",
    MinSdkVersion = "0.1.0",
    Dependencies = new[] { "other.mod>=1.0.0", "bare.mod" })]
```

Every property is optional. Empty values fall back to the matching virtual override on `ModKitMelonMod<TConfig>`.

### `ModKitModMetadata`

Record returned from `Metadata`. Carries the merged attribute + override values plus runtime-derived fields.

```csharp
public sealed record ModKitModMetadata(
    string Id,
    string DisplayName,
    string Version,
    string Author,
    string? GitHubUrl = null,
    string? Changelog = null,
    string? License = null,
    string? MinSdkVersion = null,
    string? Category = null)
{
    public IReadOnlyList<string> Dependencies { get; init; }
    public IReadOnlyList<string> Tags { get; init; }
}
```

### `ModKitManifestValidator`

```csharp
public static class ModKitManifestValidator
{
    public static ModKitManifestValidationResult Validate(
        ModKitModMetadata metadata,
        string currentSdkVersion,
        IReadOnlyList<ModKitModMetadata> registeredMods);
}
```

Checks `MinSdkVersion`, dependency presence/version, and dependency cycles. Dependency ids match case-insensitively. Specs accept `mod.id`, `mod.id>=X.Y.Z`, `mod.id?`, or `mod.id>=X.Y.Z?`; `?` means the dependency may be absent, but if present it must still satisfy the version constraint.

### `ModKitManifestValidationResult`

```csharp
public sealed record ModKitManifestValidationResult(
    bool IsValid,
    IReadOnlyList<string> Issues)
{
    public static ModKitManifestValidationResult Ok { get; }
    public static ModKitManifestValidationResult Failed(IReadOnlyList<string> issues);
}
```

## Registry

### `ModKitRegistry`

Static registry of `IModKitManagedMod` instances. Tracks UI ownership and validation state.

| Member | Description |
|--------|-------------|
| `IReadOnlyList<IModKitManagedMod> ManagedMods { get; }` | Snapshot of registered mods. |
| `bool HasEnabledMods { get; }` | True if any registered mod has `IsEnabled == true`. |
| `void Register(IModKitManagedMod mod)` | Adds the mod and runs validation across all registered mods. Read the result via `GetValidation(mod.Metadata.Id)` afterwards. |
| `void Unregister(string modId)` | Removes the mod and re-validates remaining mods. |
| `bool TryGet(string modId, out IModKitManagedMod? mod)` | Lookup by id, case-insensitive. |
| `bool IsUiOwner(string modId)` | True if this mod owns the in-game overlay (the first one to register). |
| `ModKitManifestValidationResult GetValidation(string modId)` | Current validation result. Returns `Ok` for unknown ids. |
| `ModKitManifestValidationResult RefreshSelfCheck(string modId)` | Re-runs the mod's self-check and merges issues into validation state. |
| `bool IsBlockedByManifest(string modId)` | Sugar for `!GetValidation(modId).IsValid`. |

### `ModKitRuntimePolicy`

```csharp
public static class ModKitRuntimePolicy
{
    public static bool ShouldRun(bool isEnabled, bool blockedByManifest);
}
```

Returns `true` only when the user-enabled state is on and manifest/self-check validation is not blocking the mod.

### `IModKitManagedMod`

Interface implemented by `ModKitMelonMod<TConfig>`. Implement directly only when you need a non-MelonMod object surfaced in the UI.

```csharp
public interface IModKitManagedMod
{
    ModKitModMetadata Metadata { get; }
    bool IsEnabled { get; }
    string ConfigPath { get; }
    IModKitConfigAdapter? ConfigAdapter { get; }
    void SetEnabled(bool enabled);
    IReadOnlyList<HotkeyBinding>? GetHotkeyBindings();
    bool TryRebindHotkey(string name, KeyCode newKey);
    ModKitManifestValidationResult? SelfCheck();
}
```

## Config

### `ModKitConfig`

```csharp
public static class ModKitConfig
{
    public static T LoadOrCreate<T>(string modId, T defaults, string? rootDirectory = null,
        Action<string>? info = null, Action<string>? warn = null) where T : class;
    public static ModKitConfigLoadResult<T> LoadOrCreateWithVersion<T>(string modId, T defaults,
        string? rootDirectory = null, Action<string>? info = null, Action<string>? warn = null) where T : class;
    public static bool Save<T>(string modId, T config, string? rootDirectory = null,
        Action<string>? warn = null) where T : class;
    public static string GetConfigPath(string modId, string? rootDirectory = null);
    public static bool ShouldReload(DateTimeOffset? loadedAt, DateTimeOffset? currentWriteTime);
}
```

`ModKitMelonMod<TConfig>` uses these internally. Call directly only when you need config without the lifecycle base class.

Decorate a config class with `[ModKitConfigVersion(2)]` to persist `__configVersion`. The base class calls `MigrateConfig(int loadedVersion, TConfig loaded)` when a loaded version differs from the attribute version.

### `ModKitConfigAdapter<TConfig>`

UI binding layer. `ModKitMelonMod<TConfig>` constructs one automatically; you can construct one for non-MelonMod managed mods.

```csharp
public sealed class ModKitConfigAdapter<TConfig> : IModKitConfigAdapter
{
    public ModKitConfigAdapter(string modId, Func<TConfig> getConfig, Action<TConfig> applyConfig,
        string? baseDirectory = null, Func<TConfig>? createDefault = null,
        Action<TConfig>? onChanged = null, Action<string>? warn = null);

    public IReadOnlyList<ModKitConfigProperty> GetProperties();
    public bool TrySetProperty(string name, string valueText, out string error);
    public void Save();
    public void Reload();
    public void ResetDefaults();
}
```

Editable types: `bool`, integer numerics, floating-point numerics, `string`, `enum`, `string[]`. Read-only/computed properties are skipped automatically.

### Config attributes

| Attribute | Effect |
|-----------|--------|
| `[ModKitConfigDisplay("Label text")]` | Replaces the property name with a friendlier label in the UI. |
| `[ModKitConfigRange(min, max, step)]` | Renders a slider. Applies to numeric types. |
| `[ModKitConfigOptions("a", "b", "c")]` | Renders a dropdown. Applies to `string`. |

## Patching

### `PatchGuard`

```csharp
public static class PatchGuard
{
    public static MethodInfo? ResolveMethod(Type? type, string methodName,
        Action<string>? warn = null, Type[]? parameters = null);
    public static bool PatchPostfix(Harmony harmony, Type? type, string methodName,
        MethodInfo postfix, Action<string>? warn = null, Type[]? parameters = null);
    public static bool PatchPrefix(Harmony harmony, Type? type, string methodName,
        MethodInfo prefix, Action<string>? warn = null, Type[]? parameters = null);
}
```

Returns `false` when the type or method cannot be resolved, after emitting a single warning. Modders never need to write try/catch around Harmony lookups.

### `PatchContext<TState>`

Typed static container for sharing per-mod state with Harmony postfix/prefix methods.

```csharp
public static class PatchContext<TState> where TState : class
{
    public static TState? State { get; }
    public static bool IsActive { get; }
    public static void Set(TState value);
    public static void SetActive(bool value);
    public static void Clear();
}
```

Each closed generic (one per `TState` type) gets its own static slot. Two mods using `PatchContext<MyState>` with different `MyState` classes do not collide.

Pattern:

```csharp
internal sealed class MyPatchState { public ModKitFileLog Log = null!; public MyConfig Config = null!; }

// In OnModKitEnabled:
PatchContext<MyPatchState>.Set(new MyPatchState { Log = log, Config = Config });
PatchContext<MyPatchState>.SetActive(true);

// In a Harmony postfix:
public static void Postfix(SomeType __instance)
{
    if (!PatchContext<MyPatchState>.IsActive) return;
    var state = PatchContext<MyPatchState>.State!;
    state.Log.Info($"event captured ({__instance.SomeProperty})");
}

// In OnModKitDisabled:
PatchContext<MyPatchState>.SetActive(false);
```

## Discovery

### `ModKitTypeResolver`

```csharp
public static class ModKitTypeResolver
{
    public static Type? ResolveFullName(string fullName, Action<string>? warn = null);
    public static Type? ResolveSuffix(string suffix, Action<string>? warn = null);
}
```

Prefer `ResolveFullName` in shipping mods; `ResolveSuffix` is for diagnostics during discovery sessions.

### `SceneQuery`

```csharp
public static class SceneQuery
{
    public sealed record SceneQueryResult(string Name, GameObject GameObject);

    public static IReadOnlyList<SceneQueryResult> FindObjectNamesContaining(
        string fragment, bool ignoreCase = true, int maxNames = 50, Action<string>? warn = null);
    public static IReadOnlyList<SceneQueryResult> FindObjectNamesByType(
        Type type, int maxNames = 50, Action<string>? warn = null);
    internal static IReadOnlyList<string> FilterObjectNamesContaining(
        IEnumerable<string> names, string fragment, bool ignoreCase = true, int maxNames = 50);
}
```

## Logging

### `ModKitFileLog`

Per-mod thread-safe log file with optional console mirror.

```csharp
public sealed class ModKitFileLog
{
    public ModKitFileLog(string modId, Action<string> info, Action<string> warn,
        string? rootDirectory = null);

    public string FilePath { get; }
    public bool LogToConsole { get; set; } // default true
    public bool LogToFile { get; set; }    // default true

    public void Header(string text);   // appends once, preserving prior content
    public void Info(string message);  // appends, mirrored to console
    public void Warn(string message);  // appends + always emits to warn callback
    public void Debug(string message); // appends to file only
}
```

File path is `UserData\FlashingLightsModKit\<modId>.log`. The file sink warns on write failures and pauses file writes for 30 seconds after three consecutive failures.

### `ModKitLogFormat`

Invariant-culture structured log helpers.

```csharp
public static class ModKitLogFormat
{
    public static string Quote(string? value);
    public static string KeyValue(string key, object? value);
    public static void AppendKeyValue(StringBuilder builder, string key, object? value);
    public static string Vector3(UnityEngine.Vector3 value);
    public static string Vector3(float x, float y, float z);
}
```

`Quote` escapes `\` and `"`. `KeyValue` produces `key="value"` for grep-friendly logs.

### `ModKitObjectSnapshot`

```csharp
public static class ModKitObjectSnapshot
{
    public static string Capture(object? target, params string[] memberNames);
}
```

Captures type/id plus requested fields/properties into `key="value"` log text. Unity object state is included when safe in-game; plain objects stay unit-test friendly.

## Hotkeys

### `Hotkeys` and `IHotkeyInputSource`

```csharp
public interface IHotkeyInputSource
{
    bool GetKeyDown(KeyCode key);
}

public sealed class Hotkeys
{
    public Hotkeys(IHotkeyInputSource? input = null, Action<string>? warn = null);

    public IReadOnlyCollection<string> RegisteredNames { get; }
    public IReadOnlyList<HotkeyBinding> Bindings { get; }
    public void Register(string name, KeyCode key, Action callback, string? displayName = null);
    public void Rebind(string name, KeyCode newKey);
    public void Unregister(string name);
    public void Update(); // called by ModKitMelonMod when returned from protected Hotkeys
}
```

Pass a custom `IHotkeyInputSource` for unit tests. The default `UnityHotkeyInputSource` reads `UnityEngine.Input.GetKeyDown` at runtime. Callback exceptions are caught and surfaced through the `warn` callback so a single broken hotkey can't take down the update loop. `ModKitMelonMod<TConfig>` exposes bindings to the Config tab when a derived mod returns a `Hotkeys` instance from the protected `Hotkeys` property.

## Strings

### `ModKitStrings`

Loads `UserData\FlashingLightsModKit\strings.json` as a flat dictionary. Missing files, missing keys, and invalid JSON fall back to built-in English strings.

```csharp
public static class ModKitStrings
{
    public static string Get(string key, params object?[] args);
    public static void Reload(string? baseDirectory = null);
}
```

## Paths

### `ModKitPaths`

```csharp
public static class ModKitPaths
{
    public const string UserDataFolderName = "FlashingLightsModKit";
    public static string UserDataRoot(string? baseDirectory = null);
    public static string ConfigPath(string modId, string? baseDirectory = null);
    public static string LogPath(string fileName, string? baseDirectory = null);
    public static string EnsureDirectory(string directory);
    public static string EnsureParentDirectory(string path);
    public static string ValidateFileName(string value, string parameterName);
}
```

`baseDirectory` overrides the default `MelonEnvironment.UserDataDirectory + "FlashingLightsModKit"`. Tests pass an explicit temp root.

## SDK info

### `SdkInfo`

```csharp
public static class SdkInfo
{
    public const string Version = "0.1.0";
    public static void Log(Action<string> info);
}
```

`Log` writes one line per environment fact (SDK, game, Unity, IL2CPP, base directory, user data directory) for the MelonLoader console.

## Multiplayer guard

The guard runs automatically. Modders should never need to call it directly. The policy class is documented for awareness:

### `ModKitMultiplayerPolicy`

```csharp
public static class ModKitMultiplayerPolicy
{
    public const string OnlineBlockedMessage;
    public const string ToggleBlockedMessage;

    public static bool ShouldBlockOnline(bool hasEnabledMods);
    public static bool CanEnableMods(bool isInOnlineMultiplayer);
    public static bool ShouldBlockPhotonOperation(bool hasEnabledMods, bool isOfflineMode,
        bool isOnlineRoomOperationReady, string operationName, string? roomName = null,
        int maxPlayers = 0);
    public static bool ShouldRedirectFromOnlineSession(bool hasEnabledMods, bool isInOnlineMultiplayer);
    public static bool ShouldShowOnlineBlockedUi(bool hasEnabledMods, bool isMainMenuScene,
        bool recentlyBlockedOnlineSession);
}
```

Used by the SDK's `ModKitMultiplayerGuard` Harmony patches to refuse Photon room operations and surface a banner on the main menu while managed mods are enabled.
