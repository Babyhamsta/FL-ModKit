# In-Game UI

The SDK ships a full-screen Unity IMGUI overlay for every SDK-managed mod.

Open / close: `Insert`. While open, `Esc` also closes.

A managed mod is one of:

- A class inheriting `ModKitMelonMod<TConfig>` (default — registers automatically unless `RegisterWithModKitUi` is overridden to `false`).
- A non-MelonMod that implements `IModKitManagedMod` and calls `ModKitRegistry.Register(this)` itself.

## Tabs

| Tab | Contents |
|-----|----------|
| **Mods** | Filterable list of registered managed mods with toggle buttons. When two or more categories exist, mods are grouped under category headers. |
| **Config** | Editable property grid for the selected mod's config, Config-tab hotkey rebinding, save, reload-from-file, and two-click reset-to-defaults buttons. |
| **Logs** | Lists files under `UserData\FlashingLightsModKit\` with copy-path and View buttons. View opens the last 200 lines with manual Refresh. |
| **About** | SDK version, mod count, copy-GitHub-URL button. Repeats the selected-mod summary including any manifest validation issues. |

The first managed mod that registers becomes the **UI owner** — its `OnUpdate` and `OnGUI` ticks drive the overlay. If that mod unregisters, ownership passes to the next registered mod.

## Editable types

The config grid renders public properties with setters of:

- `bool`
- integer numerics
- floating-point numerics
- `string`
- enums
- `string[]`

Anything else is skipped. Keep config classes flat.

## Manifest fields shown

For each selected mod the UI surfaces:

- ID
- Version
- Author
- License (or "Unspecified")
- Min SDK (or "any")
- Category (or "Uncategorized")
- Tags (comma-joined or "None")
- Dependencies (comma-joined or "None")
- Status: `OK` or `Blocked (N issues)` with each issue listed below
- Enabled / Disabled state
- Optional GitHub URL
- Optional changelog text

The Status line is the fastest way to spot when a mod is loaded but gated by the manifest validator.

## Multiplayer guard

While any managed mod is enabled, the SDK:

- Scans the active Unity UI for online-play buttons and disables them.
- Patches Photon room create/join/rejoin to refuse the call.
- Shows a banner explaining the block on the main menu.

Offline Photon mode (single-player voice room, etc.) stays enabled. The guard distinguishes single-player rooms by name pattern; see `ModKitMultiplayerPolicy` source for the heuristics.

If a managed mod is enabled and the player tries to enter online multiplayer anyway, the SDK redirects them out and refuses to enable any further mods until the online room state is gone.

## Toggling a mod

Click the on/off switch in the Mods tab, or set `Enabled` in the config and save.

A mod's enable request is refused (with a status message) when:

- Multiplayer guard reports an online session in progress.
- Manifest validation failed for that mod (missing dependency, SDK version mismatch).

Disable is always allowed.

## Customizing metadata

Use the `[ModKitManifest]` attribute on the mod class to centralize what the UI shows:

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
    Changelog = "Initial release.")]
```

Anything you leave out falls back to the virtual property overrides on `ModKitMelonMod<TConfig>`. Anything left null on both falls back to "Unspecified" in the UI.
