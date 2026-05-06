# FL-ModKit

FL-ModKit is an unofficial MelonLoader/Harmony SDK and runtime framework for building Flashing Lights mods.

It gives mod authors the common parts most mods need: config files, an in-game overlay, logging, hotkeys, dependency checks, and multiplayer safeguards.

## Start Here

Pick the path that matches what you are doing:

- [I want to install FL-ModKit](installing-fl-modkit.md): copy the runtime DLL and run FL-ModKit-based mods.
- [I want to make my first mod](getting-started.md): use the packaged SDK and template. No SDK source build required.
- [I want to understand the SDK](sdk-concepts.md): short explanation of config, lifecycle, UI, patches, and manifests.
- [I want API details](api-reference.md): public `FlashingLights.ModKit.Core` types and members.
- [I want to work on the SDK itself](building-sdk.md): build, test, and package the source repo.

## Runtime Install Summary

1. Install MelonLoader v0.7.0 or newer for Flashing Lights.
2. Copy `FlashingLights.ModKit.Core.dll` into the game `Mods` folder.
3. Install FL-ModKit-based mods into the same folder.
4. Launch the game and press `Insert` to open the overlay.

## Source

Source code, templates, release notes, and docs are available on GitHub:

[Babyhamsta/FL-ModKit](https://github.com/Babyhamsta/FL-ModKit)
