# Setup

## Requirements

- Flashing Lights installed through Steam.
- MelonLoader installed in the game folder.
- .NET 6 SDK available as `dotnet`.
- One successful MelonLoader IL2CPP game launch so `../MelonLoader/Il2CppAssemblies` exists.

The build references MelonLoader outputs under `../MelonLoader/net6` and `../MelonLoader/Il2CppAssemblies`.

This local install was observed at:

```text
C:/Program Files (x86)/Steam/steamapps/common/Flashing Lights
```

## Build

Run from `FlashingLightsModKit`:

```powershell
dotnet build FlashingLightsModKit.sln -c Release
dotnet run --project 'tests/FlashingLights.ModKit.Core.Tests/FlashingLights.ModKit.Core.Tests.csproj' -c Release
```

## Install Sample Mod

Run from `FlashingLightsModKit`:

```powershell
Copy-Item 'src/FlashingLights.ModKit.Core/bin/Release/net6.0/FlashingLights.ModKit.Core.dll' '../Mods/' -Force
Copy-Item 'src/FlashingLights.ModKit.Sample/bin/Release/net6.0/FlashingLights.ModKit.Sample.dll' '../Mods/' -Force
```

## Verify

Start Flashing Lights. Open:

```text
../MelonLoader/Latest.log
```

Look for:

```text
Flashing Lights ModKit Sample
FlashingLights.ModKit.Core 0.1.0
Sample observation patch registered.
```

If the patch is skipped, the SDK is still usable. Skipped means the target type or method name changed.
