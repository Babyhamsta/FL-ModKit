# Build the SDK from Source

Most mod authors do not need this page. Use it when you are changing FL-ModKit itself.

## Build and test

From the SDK repo folder:

```powershell
dotnet build .\FlashingLightsModKit.sln -c Release
dotnet run --project .\tests\FlashingLights.ModKit.Core.Tests\FlashingLights.ModKit.Core.Tests.csproj -c Release
```

If the game install is not next to the repo, pass `GameRoot` with a trailing slash:

```powershell
dotnet build .\FlashingLightsModKit.sln -c Release -p:GameRoot="C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\"
dotnet run --project .\tests\FlashingLights.ModKit.Core.Tests\FlashingLights.ModKit.Core.Tests.csproj -c Release -p:GameRoot="C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\" --no-build
```

## Package

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\package-sdk.ps1 -Configuration Release
```

Output:

```text
artifacts\FL-ModKit-v0.1.0-sdk.zip
artifacts\sdk\FL-ModKit-v0.1.0\
```

The zip contains:

```text
lib\FlashingLights.ModKit.Core.dll
templates\BasicMelonMod\
templates\ModTests\
docs\
README.md
LICENSE
CHANGELOG.md
```

Gameplay mods are separate projects and are not included in SDK packages.
