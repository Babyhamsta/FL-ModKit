# Local Verification

## Build

`dotnet build FlashingLightsModKit.sln -c Release` passed.

## Core Tests

`dotnet run --project tests/FlashingLights.ModKit.Core.Tests/FlashingLights.ModKit.Core.Tests.csproj -c Release` passed.

## Install

Installed `FlashingLights.ModKit.Core.dll` and `FlashingLights.ModKit.Sample.dll` into `Mods`. No existing ModKit DLLs were present, so no backup files were created.

## Runtime

The sample mod loaded in Flashing Lights through MelonLoader. `MelonLoader/Latest.log` showed SDK startup output, `FlashingLights.ModKit.Core 0.1.0`, `Flashing Lights ModKit Sample v0.1.0`, `Sample observation patch registered.`, and `Scene loaded: MainMenu2 (1)`.
