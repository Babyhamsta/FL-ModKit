# SDK Concepts

This page explains the SDK without listing every API.

## Runtime DLL

`FlashingLights.ModKit.Core.dll` is the shared runtime.

Players put it in the game `Mods\` folder. Mod authors reference it when building mods.

## Managed mod

A managed mod inherits:

```csharp
ModKitMelonMod<TConfig>
```

That base class connects your mod to config loading, the overlay, lifecycle hooks, logging, manifests, hotkeys, and safe patch helpers.

## Config

`TConfig` is your config class.

```csharp
public sealed class MyConfig
{
    public bool Enabled { get; set; } = true;
    public int SpawnLimit { get; set; } = 10;
}
```

The SDK saves it as JSON under:

```text
UserData\FlashingLightsModKit\<mod-id>.json
```

Supported simple properties can be edited from the Config tab.

## Lifecycle

Use the `OnModKit*` methods instead of raw MelonLoader methods.

Most mods start with these:

- `OnModKitInitialized`: one-time setup.
- `OnModKitEnabled`: start runtime behavior.
- `OnModKitDisabled`: stop runtime behavior.
- `OnConfigApplied`: react to config changes.

See [Lifecycle](lifecycle.md) when you need exact ordering.

## Manifest

`[ModKitManifest]` tells the SDK what your mod is:

- id
- display name
- version
- author
- category and tags
- dependencies
- minimum SDK version

If a dependency or SDK version check fails, the mod stays visible in the overlay but does not run.

## Patches

Most game behavior changes need Harmony patches.

For a first mod, use the base helpers:

- `PatchPrefix`
- `PatchPostfix`

For larger mods, use `PatchGuard` and `PatchContext<TState>`.

See [Patching](patching.md) and [Cookbook](cookbook.md) after your first mod loads.

## In-game overlay

Press `Insert` in-game.

The overlay gives players:

- mod enable/disable toggle
- config editor
- hotkey rebinding
- metadata and dependency status
- log viewer
- self-check rerun button

Your mod appears there when it inherits `ModKitMelonMod<TConfig>` and stays registered with the UI.
