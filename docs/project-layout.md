# Project Layout

```text
src\FlashingLights.ModKit.Core\         Generic SDK helpers, lifecycle base class, in-game UI host.
tests\FlashingLights.ModKit.Core.Tests\ Console-app test harness (no xunit).
templates\BasicMelonMod\                Copyable starter mod that ships in the SDK zip.
docs\                                   Public SDK docs (this folder).
tools\package-sdk.ps1                   Release packaging script.
artifacts\                              Local package output (gitignored).
.github\workflows\                      CI (build + test on push, release on tag).
```

The SDK package zip contains only `lib\`, `templates\`, `docs\`, README, LICENSE, and CHANGELOG. Gameplay mods live in separate workspaces or repositories and are intentionally not included.

The future v1.0 release will introduce two more assemblies:

- `src\FlashingLights.ModKit.Game\` — game-system wrappers (vehicles, world events, save events, scene lifecycle, pools) exposed as plain C# events.
- `src\FlashingLights.ModKit.Diagnostics\` — drop-in test harness mod for verifying every wrapper binding.

Until then, `Core` is the only published assembly and gameplay mods write their own Harmony patches against IL2CPP types directly.

## Conventions

- Target framework `net6.0` everywhere (MelonLoader runtime constraint).
- `LangVersion=latest`, `Nullable=enable`.
- Game / MelonLoader references use `<Private>false</Private>` — the assemblies live in the game folder, never copied into mod output.
- Per-solution `Directory.Build.props` defines `$(GameRoot)`, `$(MelonLoaderNet6)`, `$(Il2CppAssemblies)`, `$(ModKitRoot)`. Override via `-p:` flags, never hardcode paths in csproj.
- One type per file. Filename matches the type. CRLF line endings, 4-space C# indent, 2-space project files.
