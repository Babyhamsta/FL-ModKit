# Getting Started

Build the SDK, run the tests, package the zip, drop it into the game.

## Build

From the SDK folder:

```powershell
dotnet build .\FlashingLightsModKit.sln -c Release
dotnet run --project .\tests\FlashingLights.ModKit.Core.Tests\FlashingLights.ModKit.Core.Tests.csproj -c Release
```

If your `MelonLoader\` folder isn't a sibling of the SDK folder (typical for a Steam install), pass `GameRoot` explicitly with a trailing separator:

```powershell
dotnet build .\FlashingLightsModKit.sln -c Release -p:GameRoot="C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\"
```

## Package

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\package-sdk.ps1 -Configuration Release
```

Output:

```text
artifacts\FL-ModKit-v0.1.0-sdk.zip
```

The zip contains `lib\FlashingLights.ModKit.Core.dll`, the `templates\BasicMelonMod` starter, the `docs\` folder, README, LICENSE, and CHANGELOG.

## Build a mod from the template

1. Extract the zip somewhere outside the SDK source tree.
2. Copy `templates\BasicMelonMod` to a new folder. Rename the folder, the project file, the namespace, and the assembly metadata in `Properties\AssemblyInfo.cs`.
3. Open `BasicMod.cs`. Replace the body with your logic. The minimal mod looks like this:

```csharp
using FlashingLights.ModKit.Core;

namespace MyFirstMod;

[ModKitManifest(
    Id = "myname.firstmod",
    DisplayName = "My First Mod",
    Version = "0.1.0",
    Author = "Babyhamsta",
    License = "MIT",
    MinSdkVersion = "0.1.0")]
public sealed class MyFirstMod : ModKitMelonMod<MyConfig>
{
    protected override string ModId => "myname.firstmod";

    protected override void OnModKitInitialized()
    {
        LogInfo($"{Metadata.DisplayName} v{Metadata.Version} initialized.");
    }

    protected override void OnModKitEnabled() => LogInfo("Enabled.");
    protected override void OnModKitDisabled() => LogInfo("Disabled.");
}

public sealed class MyConfig
{
    public bool Enabled { get; set; } = true;

    [ModKitConfigDisplay("Speed multiplier")]
    [ModKitConfigRange(0.5, 4.0, 0.1)]
    public double SpeedMultiplier { get; set; } = 1.0;
}
```

4. Build the project against the unzipped SDK. From the mod project folder:

```powershell
dotnet build -c Release -p:GameRoot="C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\" -p:ModKitRoot="C:\Path\To\FL-ModKit-v0.1.0\"
```

## Deploy

Copy your built mod DLL plus `FlashingLights.ModKit.Core.dll` into:

```text
C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\Mods\
```

Launch the game. Press `Insert` once you're past the title screen — you'll see your mod listed with a live config editor bound to:

```text
C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\UserData\FlashingLightsModKit\myname.firstmod.json
```

Edit the JSON file with the game running and (if `EnableConfigHotReload` is on) the changes apply within the configured interval.

## Next steps

- [Lifecycle](lifecycle.md) — when each override fires.
- [Cookbook](cookbook.md) — patch a method, register a hotkey, write to a per-mod log file.
- [API reference](api-reference.md) — every public type and member.
