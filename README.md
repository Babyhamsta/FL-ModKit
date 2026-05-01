# Flashing Lights ModKit

Slim helper SDK for building MelonLoader IL2CPP mods for Flashing Lights.

## Current Target

- Flashing Lights game version: 1.1
- Unity version: 2022.3.62f2
- Loader: MelonLoader 0.7.0 Open-Beta
- Runtime: IL2CPP x64, .NET 6

## Projects

- `FlashingLights.ModKit.Core`: reusable helper APIs.
- `FlashingLights.ModKit.Sample`: minimal MelonLoader sample mod.
- `FlashingLights.ModKit.Core.Tests`: no-NuGet console checks for core helpers.

## Quick Build

```powershell
dotnet build FlashingLightsModKit.sln -c Release
```

## Local Install

```powershell
Copy-Item 'src/FlashingLights.ModKit.Core/bin/Release/net6.0/FlashingLights.ModKit.Core.dll' '../Mods/' -Force
Copy-Item 'src/FlashingLights.ModKit.Sample/bin/Release/net6.0/FlashingLights.ModKit.Sample.dll' '../Mods/' -Force
```

Start the game and check `../MelonLoader/Latest.log`.
