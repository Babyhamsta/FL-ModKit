# Lifecycle

`ModKitMelonMod<TConfig>` seals the MelonLoader entry points (`OnInitializeMelon`, `OnUpdate`, `OnGUI`, `OnSceneWasLoaded`) and routes them through framework hooks. Override the `OnModKit*` virtual methods instead — they fire in a predictable order regardless of MelonLoader plumbing changes.

## Order of operations

```mermaid
flowchart TD
    A[MelonLoader loads mod assembly] --> B[OnInitializeMelon]
    B --> C[ConfigPath resolved + ModKitConfig.LoadOrCreate]
    C --> D[ConfigAdapter constructed]
    D --> E[IsEnabled = config.Enabled ?? true]
    E --> F{RegisterWithModKitUi?}
    F -- yes --> G[ModKitRegistry.Register + manifest validation]
    F -- no --> H[Skip registry]
    G --> I[Log validation issues if any]
    H --> I
    I --> J[ModKitMultiplayerGuard.InstallPatches]
    J --> K[SdkInfo.Log]
    K --> L[OnModKitInitialized override]
    L --> M{IsEnabled and validation Ok?}
    M -- yes --> N[OnModKitEnabled override]
    M -- no --> O[Skip OnModKitEnabled]
    N --> P[OnUpdate loop]
    O --> P
    P --> Q[OnGUI loop]
```

Once initialization is complete, MelonLoader drives the per-frame loop:

```mermaid
flowchart LR
    U[OnUpdate per frame] --> V{IsUiOwner?}
    V -- yes --> W[ModKitUi tick + multiplayer guard tick]
    V -- no --> X[Skip UI tick]
    W --> Y[TryReloadConfig]
    X --> Y
    Y --> Z{Runtime active?}
    Z -- yes --> AA[OnModKitUpdate override]
    Z -- no --> AB[Skip update body]
    AA --> AC[Next frame]
    AB --> AC
```

## Config-edit and reload paths

There are three paths that re-apply config to your mod. They all converge on the same overrides so you only need to react in one place.

```mermaid
flowchart TD
    A[Hot reload timer ticks] --> B{File mtime newer than loaded?}
    B -- yes --> C[Reload config from disk]
    B -- no --> Z[no-op]
    C --> D[OnConfigApplied + OnConfigReloaded]
    E[UI Reset Defaults / Reload from File] --> C
    F[UI property edit] --> G[ConfigAdapter writes property]
    G --> H[OnConfigApplied only]
```

`OnConfigApplied` fires for every code path. `OnConfigReloaded` fires only when the entire config object is replaced (hot reload, manual reload, reset defaults).

## What each hook is for

| Hook | When it fires | Use it for |
|------|---------------|------------|
| `OnModKitInitialized()` | Once, after registry + multiplayer guard install. | One-time setup (resolving types, wiring `PatchContext<TState>`, setting up `ModKitFileLog`, registering hotkeys). |
| `OnModKitEnabled()` | After init when `IsEnabled` is true and manifest validation passed; and on every transition to runtime-active. | Apply Harmony patches, subscribe to events, start work. |
| `OnModKitDisabled()` | On every transition out of runtime-active. | Unpatch, unsubscribe, stop work. Be idempotent — config edits and self-check reruns can both change runtime state. |
| `OnModKitUpdate()` | Per frame, only while runtime-active. | Polling work. Keep cheap. |
| `OnModKitGui()` | Per OnGUI pass, only while runtime-active. | Custom IMGUI overlays. The ModKit overlay handles config UI for you. |
| `OnConfigApplied(TConfig)` | After any config change applied to runtime. | Push the new values into your runtime objects (cache references, recompute thresholds). |
| `OnConfigReloaded(TConfig prev, TConfig now)` | Hot reload, manual reload, reset defaults. | Diff old vs new for "did this expensive thing change" optimizations. |
| `OnModKitSceneWasLoaded(int, string)` | Scene-load hook, only while runtime-active. | Scene-specific setup. |

## Lifecycle invariants

- `Config` is non-null from the first invocation of any override.
- `Metadata` resolves once and is cached. Reading `Metadata.Id` from any hook is safe.
- `Metadata.Id` equals `ModId` only if you didn't override the `[ModKitManifest(Id = ...)]` value. Prefer `Metadata.Id` everywhere downstream.
- `IsEnabled` reflects the user's intent. Runtime hooks fire only when `ModKitRuntimePolicy.ShouldRun(IsEnabled, ModKitRegistry.IsBlockedByManifest(Metadata.Id))` is true, so manifest-blocked mods remain visible but do not run.
- Hot reload works for any property type the JSON serializer can round-trip. Live UI edits work for `bool`, integer numerics, floating-point numerics, `string`, enums, and `string[]`.
