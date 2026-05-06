# Troubleshooting

For deeper debugging recipes (in-game UI, log layout, common pitfalls), see [debugging.md](debugging.md). This page covers fixes for the errors most people hit first.

## Build errors

### `Texture2D` / `Vector2` / `GUIStyle` could not be found

The MelonLoader Unity assemblies aren't being resolved. Pass `GameRoot` to `dotnet build`:

```powershell
dotnet build .\FlashingLightsModKit.sln -c Release -p:GameRoot="C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\"
```

The trailing separator is required.

### `MelonLoader.dll` not found

You haven't run the game with MelonLoader installed yet. Launch the game once so MelonLoader generates `Il2CppAssemblies\` next to its own `net6\` folder.

### NETSDK1138: target framework `net6.0` is out of support

This is a build warning, not an error — `net6.0` is required by MelonLoader's runtime. Don't bump the target framework to silence the warning.

## Runtime errors

### `TypeInitializationException: The type initializer for 'UnityEngine.Vector3' threw an exception`

You're touching a `UnityEngine` type whose static constructor needs the IL2CPP runtime, but you're running outside the game (probably in a unit test). For helpers that format Unity types, expose a primitive-typed overload (`Vector3(float x, float y, float z)`) so tests can exercise the formatting without instantiating the Unity type. The SDK's `ModKitLogFormat.Vector3` follows this pattern.

### `Manifest validation: requires SDK >= X.Y.Z, current SDK is A.B.C`

The mod declares a higher `MinSdkVersion` than the SDK currently shipping in the game's `Mods\` folder. Either upgrade the SDK or relax the mod's `MinSdkVersion`.

### `Manifest validation: missing dependency 'mod.id'`

The mod declares a dependency that hasn't been registered. Check that the dependency mod is in the `Mods\` folder, that its DLL is loading (look for its init line in `Latest.log`), and that its `ModId` matches the dependency spec in your mod's manifest. Dependency ids match case-insensitively.

### `Cannot enable 'mod.id': blocked by manifest validation`

Triggered by `SetEnabled(true)` while the mod is in a `Blocked` state. Resolve the underlying validation issue first — the in-game About tab lists all current issues for the selected mod.

## Config

### Config file not where expected

Configs live at `UserData\FlashingLightsModKit\<mod-id>.json` unless your mod passes a custom base directory to `ModKitConfig.LoadOrCreate`.

### One of my config properties doesn't show up in the UI

`ModKitConfigAdapter` only renders public properties with both a getter and a setter, where the type is `bool`, integer numerics, floating-point numerics, `string`, an enum, or `string[]`. Computed read-only properties show as labels but aren't editable. Lists, dictionaries, and custom object types are skipped.

### Hot reload doesn't pick up my JSON edits

`EnableConfigHotReload` defaults to `false`. Override it in your mod class to `true` (or read it from a config field). Hot reload is mtime-based; an editor that does atomic-write-rename can occasionally skip a tick — saving twice usually clears it.

## In-game UI

### `Insert` doesn't open the overlay

At least one SDK-managed mod must be loaded. The UI host runs from the first registered managed mod (the **UI owner**). If no mod inherits `ModKitMelonMod<TConfig>` or registers via `ModKitRegistry.Register`, the overlay never starts.

Confirm in `Latest.log` that your mod's init line appeared. If it didn't, MelonLoader didn't load the assembly — check that both your mod DLL and `FlashingLights.ModKit.Core.dll` are in the game's `Mods\` folder.

### UI opens but my mod isn't in the list

The mod overrode `RegisterWithModKitUi => false`. Either drop the override or set it to `true`.

### UI looks broken at non-1080p resolutions

The footer fits within 1080p by design. At smaller resolutions, the body pane scrolls. At very large resolutions (4K, ultrawide) the panes expand horizontally — not pixel-perfect, but functional.

## Patching

### Patches install but don't fire

Three causes:

1. `PatchContext<TState>.IsActive` is false. The pattern is `Set` + `SetActive(true)` in `OnModKitEnabled`; `SetActive(false)` in `OnModKitDisabled`.
2. The mod is in a `Blocked` state — `OnModKitEnabled` was never called. Check the About tab.
3. The patched method runs in a code path that's gated upstream (e.g., the FSM never reached the action you patched). Add a `LogInfo` at the top of the postfix to confirm.

### `ModKitTypeResolver.ResolveFullName` returns null

The IL2CPP type name has changed (or you got the namespace wrong). Confirm the type still exists in `MelonLoader\Il2CppAssemblies\`. Game updates can rename obfuscated types; cache the resolved type in `OnModKitInitialized` and log once instead of silently no-oping.
