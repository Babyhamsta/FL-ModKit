# Flashing Lights ModKit

Slim helper SDK for building MelonLoader IL2CPP mods for Flashing Lights.

## Current Target

- Flashing Lights game version: 1.1
- Unity version: 2022.3.62f2
- Loader: MelonLoader 0.7.0 Open-Beta
- Runtime: IL2CPP x64, .NET 6

## Projects

- `FlashingLights.ModKit.Core`: reusable helper APIs for runtime info, type lookup, scene scans, config, and safe patch registration.
- `FlashingLights.ModKit.Sample`: minimal MelonLoader sample mod that logs known types and scene object counts.
- `FlashingLights.ModKit.Core.Tests`: no-NuGet console checks for helper logic that can run outside the game.

## Build

```powershell
dotnet build FlashingLightsModKit.sln -c Release
dotnet run --project 'tests/FlashingLights.ModKit.Core.Tests/FlashingLights.ModKit.Core.Tests.csproj' -c Release
```

## Install Sample

```powershell
Copy-Item 'src/FlashingLights.ModKit.Core/bin/Release/net6.0/FlashingLights.ModKit.Core.dll' '../Mods/' -Force
Copy-Item 'src/FlashingLights.ModKit.Sample/bin/Release/net6.0/FlashingLights.ModKit.Sample.dll' '../Mods/' -Force
```

Start the game and inspect:

```text
../MelonLoader/Latest.log
```

Expected log signals:

- `Flashing Lights ModKit Sample` loads.
- SDK info logs game and Unity version.
- Known type lookup logs found or missing types.
- Scene load logs object counts.

## Docs

- `docs/setup.md`
- `docs/il2cpp-discovery.md`
- `docs/known-types.md`
