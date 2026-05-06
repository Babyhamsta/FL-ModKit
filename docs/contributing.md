# Contributing

Thanks for considering a contribution. The SDK is small and the bar is high — this is the framework every future Flashing Lights mod will inherit from.

## Local setup

Requirements:

- Windows (the game is Windows-only and the IL2CPP assemblies have Windows path conventions).
- .NET SDK 9.x (we target `net6.0` to match MelonLoader, but build with the latest SDK for `LangVersion=latest`).
- Flashing Lights with [MelonLoader](https://melonwiki.xyz/) installed at least once so `MelonLoader\Il2CppAssemblies\` exists.

Build + test:

```powershell
dotnet build .\FlashingLightsModKit.sln -c Release
dotnet run --project .\tests\FlashingLights.ModKit.Core.Tests\FlashingLights.ModKit.Core.Tests.csproj -c Release
```

If MelonLoader assemblies aren't a sibling of the SDK folder, pass `GameRoot` (trailing separator required):

```powershell
dotnet build .\FlashingLightsModKit.sln -c Release -p:GameRoot="C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\"
```

## What to send

PRs welcome for:

- Bug fixes with a regression test.
- New helpers that have already been duplicated across two or more mods (the "promote when proven" rule).
- Documentation improvements.
- Template or documentation updates that help mod authors use the SDK correctly.

Please discuss in an issue first for:

- Any change that breaks the public API of `FlashingLights.ModKit.Core`.
- New game-specific wrapper APIs (the `FlashingLights.ModKit.Game` assembly arrives in v1.0 — please coordinate).
- Anything that increases the surface of the in-game UI.

## Code style

- Target framework stays `net6.0`. Don't bump it past MelonLoader's runtime constraint.
- `LangVersion=latest`, `Nullable=enable`. Annotate carefully; the SDK is consumed without nullable annotations by IL2CPP-side mods.
- Public types get XML docs only when their behavior isn't obvious from the name. Default to no comment; lean on identifiers, attributes, and tests.
- One type per file. File name matches the type.
- 4-space indent for `.cs`, 2-space for project/props files. CRLF line endings (enforced by `.editorconfig`).
- Use `Path.Combine` and forward-slash literals where cross-platform tooling looks at the path. Stick to backslashes only in user-facing Windows examples.

## Tests

The Core test app at `tests\FlashingLights.ModKit.Core.Tests\Program.cs` is a console-app harness (not xunit, not NUnit). To add a test:

1. Append a `(Name, Body)` tuple to the `tests` array.
2. Implement the body as a static method that throws on failure.
3. Use the existing `Assert*` helpers near the bottom of the file.

The harness exits with code 1 on any failure. CI fails the build accordingly.

For tests that need to write files (e.g., `ModKitFileLog`, `ModKitConfig`), use `NewTempRoot()` / `DeleteTempRoot()` — they isolate per-test temp directories so tests don't bleed into each other.

For tests that need IL2CPP types but can't initialize them (the `Vector3` static-init problem), split the helper into a primitive-typed overload that the test calls directly.

The packaged SDK includes `templates\ModTests\`, a minimal console-app harness for mod authors. Keep it aligned with the Core harness style: append `(Name, Body)` tuples, throw on failure, exit 0 only when every test passes.

## Commits

[Conventional Commits](https://www.conventionalcommits.org/), imperative mood. Established prefixes: `feat:`, `fix:`, `docs:`, `chore:`, `test:`, `refactor:`. Subject ≤72 chars.

Reference an issue number in the body when fixing one. Don't squash multiple unrelated changes into one commit; reviewers find them easier to read separately.

Examples from the existing log:

```text
feat: add ModKitFileLog with thread-safe file sink
fix: refuse Photon room ops when managed mods are enabled
docs: add lifecycle diagram and override ordering
```

## PRs

Open against `main`. Include:

- One sentence summary of what changed and why.
- A short test plan (what you ran, what you saw).
- A note in `CHANGELOG.md` under `## Unreleased`. The maintainer cuts a versioned section at release time.
- Updated docs if the public API surface moved.

CI runs the build + test harness on every push. Don't merge red.

## Project boundary

This repo is the public SDK. Gameplay mods should live in separate repositories or workspaces and consume the SDK through the packaged `FlashingLights.ModKit.Core.dll` or a local project reference during development. Do not include mod-specific source, binaries, configs, or logs in SDK PRs.

## License

By contributing you agree your work will be licensed under the [MIT license](../LICENSE) used by this repo.
