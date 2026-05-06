# FL-ModKit

[![SDK Version](https://img.shields.io/badge/SDK-0.1.0-blue)](CHANGELOG.md)
[![Target](https://img.shields.io/badge/target-net6.0-512bd4)](FlashingLightsModKit.sln)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

The reference modding framework for [Flashing Lights](https://flashinglights-game.com/) — built on MelonLoader, Harmony, and Unity IL2CPP. Drop the SDK in, inherit one base class, and get a polished mod surface for free.

## What you get

- **One base class, full lifecycle.** Inherit `ModKitMelonMod<TConfig>` and you have hooks for init, enable/disable, per-frame update, scene load, config apply, and config reload — all sealed correctly to MelonLoader's underlying calls.
- **JSON config with hot reload.** Drop properties on a POCO, get them serialized to `UserData\FlashingLightsModKit\<mod-id>.json` automatically. Edit the file in-game and the mod re-applies on the next tick.
- **In-game config editor.** Press `Insert` in-game and the SDK overlay lets the player toggle, edit, save, reload, reset, rebind hotkeys, view logs, and rerun self-checks for every managed mod from a single panel.
- **Mod manifest with dependency resolution.** Declare `[ModKitManifest(Id = "...", Category = "Gameplay", Dependencies = new[] { "other.mod>=1.0.0", "soft.mod?" })]` and the SDK validates SDK version, inter-mod dependencies, optional dependencies, and cycles on registration. Blocked mods stay visible in the UI but are gated from running.
- **Multiplayer guard out of the box.** Online play is automatically refused while any managed mod is enabled, and Photon room operations are blocked at the source. You don't have to think about it.
- **Harmony plumbing.** `PatchGuard` resolves and patches methods with full diagnostic logging. `PatchContext<TState>` replaces the bespoke "static sink" pattern with a typed, thread-safe container.
- **Discovery helpers.** `ModKitTypeResolver`, `SceneQuery`, structured `ModKitLogFormat`, per-mod `ModKitFileLog`, `ModKitObjectSnapshot`, configurable `Hotkeys` — the helpers every mod re-implements, written once and tested.

## Why pick this over rolling your own

You can write a MelonMod from scratch and get something working in a hundred lines. ModKit is for the next ten thousand:

- You stop writing the same JSON-load + UI + Harmony-resolve scaffolding for every mod.
- Your players get a unified place to manage every ModKit-built mod, instead of a folder full of bespoke configs.
- Cross-mod dependencies, version constraints, and "this mod requires SDK 0.3 or newer" are first-class — you declare them, the SDK enforces them.
- Multiplayer-safety is enforced at the framework level. You can't ship something that breaks public lobbies.
- The SDK repo stays focused on framework code. Individual mods are separate consumers, so release zips contain only SDK binaries, docs, and templates.

## Quickstart

Build the SDK and run tests:

```powershell
dotnet build .\FlashingLightsModKit.sln -c Release
dotnet run --project .\tests\FlashingLights.ModKit.Core.Tests\FlashingLights.ModKit.Core.Tests.csproj -c Release
```

If your `MelonLoader\` folder lives outside the workspace (the typical case for the Steam install), pass `GameRoot`:

```powershell
dotnet build .\FlashingLightsModKit.sln -c Release -p:GameRoot="C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\"
```

Package an SDK zip ready to share:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\package-sdk.ps1 -Configuration Release
```

Output: `artifacts\FL-ModKit-v0.1.0-sdk.zip`.

## Build your first mod

```csharp
using FlashingLights.ModKit.Core;

namespace MyFirstMod;

[ModKitManifest(
    Id = "myname.firstmod",
    DisplayName = "My First Mod",
    Version = "0.1.0",
    Author = "Babyhamsta",
    License = "MIT",
    MinSdkVersion = "0.1.0")]
public sealed class MyFirstMod : ModKitMelonMod<MyConfig>
{
    protected override string ModId => "myname.firstmod";

    protected override void OnModKitEnabled()
    {
        LogInfo($"{Metadata.DisplayName} v{Metadata.Version} enabled.");
    }
}

public sealed class MyConfig
{
    public bool Enabled { get; set; } = true;
    [ModKitConfigDisplay("Speed multiplier")]
    [ModKitConfigRange(0.5, 4.0, 0.1)]
    public double SpeedMultiplier { get; set; } = 1.0;
}
```

Build the project against the packaged SDK, drop `MyFirstMod.dll` and `FlashingLights.ModKit.Core.dll` into:

```text
C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\Mods\
```

Launch the game, press `Insert`, and you'll see your mod in the list with the speed slider live-bound to the JSON config under `UserData\FlashingLightsModKit\myname.firstmod.json`.

## Documentation

Quickstart and concepts:

- [Getting started](docs/getting-started.md) — build, package, deploy.
- [Build a first mod](docs/building-first-mod.md) — copy the template and rename.
- [Lifecycle](docs/lifecycle.md) — when each override fires.
- [Project layout](docs/project-layout.md) — what lives where.

API surfaces:

- [API reference](docs/api-reference.md) — every public type and member.
- [Config](docs/config.md) — JSON storage, hot reload, UI binding.
- [In-game UI](docs/ui.md) — overlay, manifest surfaces, multiplayer guard.
- [Patching](docs/patching.md) — `PatchGuard`, `PatchContext<TState>`.
- [Discovery](docs/discovery.md) — `ModKitTypeResolver`, `SceneQuery`.

Operating the SDK:

- [Cookbook](docs/cookbook.md) — short recipes for common tasks.
- [Debugging](docs/debugging.md) — logs, common pitfalls, in-game diagnostics.
- [Troubleshooting](docs/troubleshooting.md) — fixes for build and load errors.
- [Packaging](docs/packaging.md) — what `package-sdk.ps1` produces.
- [SDK versioning](docs/sdk-versioning.md) — semver promise and migration notes.
- [Contributing](docs/contributing.md) — how to send a PR.

## License

[MIT](LICENSE).
