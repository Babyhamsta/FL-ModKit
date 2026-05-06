# Debugging

How to find out why something isn't working.

## Where the logs live

Three log streams:

| Stream | Path | What it captures |
|--------|------|------------------|
| MelonLoader console | in-game console window (with the loader's debug build) | Everything written via `LogInfo` / `LogWarning`, plus the SDK's own startup banner. |
| MelonLoader file log | `<game>\MelonLoader\Latest.log` | Same content as the console, archived per launch. Older runs in `MelonLoader\Logs\`. |
| Per-mod `ModKitFileLog` | `<game>\UserData\FlashingLightsModKit\<mod-id>.log` | Whatever your mod writes through `log.Info` / `log.Warn` / `log.Header`. |

Read in this order:

1. `Latest.log` for SDK-level events (manifest validation, multiplayer guard, type resolution warnings).
2. The per-mod log for your own diagnostics.
3. The in-game UI (Insert key) for live state — selected mod's manifest, validation status, current config values.

## In-game diagnostics

Press `Insert`. Pick your mod from the list.

- The **Mods** tab shows enable/disable toggle and selected-mod summary including license, min SDK, dependencies, and validation status.
- The **Config** tab shows live editable property values. Edits persist to JSON on Save and re-apply via `OnConfigApplied`.
- The **Logs** tab lists files under `UserData\FlashingLightsModKit\`, copies paths, and views the last 200 lines with manual Refresh.
- The **About** tab shows SDK version, mod count, and the selected mod's full metadata + validation issues.

If a mod is showing **Blocked (N issues)** in Status, expand the issue list — the lines describe exactly which dependency is missing or which version constraint failed.

## Common pitfalls

### "My mod loads but never enters OnModKitEnabled"

Three causes in order of likelihood:

1. **Manifest validation failed.** Check `Latest.log` for `Manifest validation:` lines. If `MinSdkVersion` or a dependency is unmet, `OnModKitEnabled` is intentionally skipped. Either fix the constraint or update the dependency.
2. **Mod is disabled in config.** Check `UserData\FlashingLightsModKit\<mod-id>.json` for `"Enabled": false`. The in-game UI toggle writes this back.
3. **Multiplayer guard blocked the enable.** Look for `ToggleBlockedMessage` in the log. Quit the online lobby and try again.

### "ModKitTypeResolver.ResolveFullName returns null"

The IL2CPP type name has changed (or you got the namespace wrong). Confirm the type still exists in `MelonLoader\Il2CppAssemblies\` by opening the relevant `Il2Cpp*.dll` in dnSpy or ILSpy. Game updates can rename obfuscated types; cache the resolved type in `OnModKitInitialized` and log a single warning if it's null instead of silently no-oping.

### "Patches install but don't fire"

Check `PatchContext<TState>.IsActive` is true. The pattern is `Set` + `SetActive(true)` in `OnModKitEnabled`, `SetActive(false)` in `OnModKitDisabled`. If `Set` is called once at init but `SetActive` never flips, the patch will run but every body will short-circuit on `if (!IsActive) return;`.

### "Hot reload doesn't pick up my JSON edits"

`EnableConfigHotReload` defaults to `false`. Override it (returning either a const `true` or `Config.EnableHotReload` for player-controlled toggling) in your mod class. Hot reload polls based on file mtime; check that your editor isn't writing a temp file and renaming (which can update mtime in unexpected ways).

### "Config UI doesn't show one of my properties"

`ModKitConfigAdapter` only renders properties with public getters and setters of supported types: `bool`, integer numerics, floating-point numerics, `string`, enums, `string[]`. Computed read-only properties are skipped. Nested objects, lists, dictionaries, custom types are also skipped. Flatten the config or split it across multiple flat configs.

### "Build fails with 'Texture2D could not be found'"

Game assemblies aren't being resolved. Pass `GameRoot` to `dotnet build`:

```powershell
dotnet build .\FlashingLightsModKit.sln -c Release -p:GameRoot="C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\"
```

The trailing separator is required.

### "PlayMaker action patches don't fire"

PlayMaker actions are IL2CPP types with parameters that get serialized into FSM state. Patching `OnEnter` works in postfix; `__instance` gives you access to the action's fields. Some actions are pooled and reused — don't store references between calls. Keep worked gameplay examples in their own mod repositories, not in the SDK.

### "Mod works but breaks in IL2CPP runtime with TypeInitializationException"

Some `UnityEngine` types (e.g., `Vector3.zero`, `Color.white`) trigger static constructors that depend on the IL2CPP runtime. They work in-game but throw if you reference them from a unit test. For helpers that need to format Unity types, expose a primitive-typed overload (`Vector3(float x, float y, float z)`) so the testable surface doesn't touch Unity statics.

## When all else fails

Open the in-game UI, copy your mod's config path, attach `Latest.log` and the relevant per-mod `.log`, and file an issue at the SDK repo. The two files plus the in-game UI screenshot of the About tab are usually enough to triage.
