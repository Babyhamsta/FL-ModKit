# Build a First Mod

Goal: copy the starter template, rename it, build it, and see it in-game.

## 1. Copy the template

Extract `FL-ModKit-v0.1.0-sdk.zip`, then copy:

```text
templates\BasicMelonMod\
```

to a new folder, for example:

```text
C:\Users\Admin\Documents\FL_Mods\MyFirstMod\
```

## 2. Rename the project

Rename these things first:

| Old | New example |
|-----|-------------|
| Folder `BasicMelonMod` | `MyFirstMod` |
| File `BasicMelonMod.csproj` | `MyFirstMod.csproj` |
| Namespace `BasicMelonMod` | `MyFirstMod` |
| File `BasicMod.cs` | `MyFirstModMod.cs` |
| Class `BasicMod` | `MyFirstModMod` |
| File `BasicConfig.cs` | `MyFirstModConfig.cs` |
| Class `BasicConfig` | `MyFirstModConfig` |

Open `Properties\AssemblyInfo.cs` and update the namespace import, mod class, display name, version, and author:

```csharp
using MyFirstMod;
using MelonLoader;

[assembly: MelonInfo(typeof(MyFirstModMod), "My First Mod", "0.1.0", "Babyhamsta")]
[assembly: MelonGame("NilsJakrins", "flashinglights")]
```

This is what MelonLoader shows in its log.

## 3. Set the ModKit metadata

Open the main mod class. The important values are:

```csharp
[ModKitManifest(
    Id = "myname.firstmod",
    DisplayName = "My First Mod",
    Version = "0.1.0",
    Author = "Babyhamsta",
    License = "MIT",
    MinSdkVersion = "0.1.0")]
public sealed class MyFirstModMod : ModKitMelonMod<MyFirstModConfig>
{
    protected override string ModId => "myname.firstmod";
}
```

`Id` and `ModId` must match. Use lowercase letters, numbers, dots, and dashes.

For a published mod, set `Author` to the mod author's display name. If the mod has its own source repository, add `GitHubUrl`; otherwise leave it out.

## 4. Add one config option

Config is a plain C# class. Public properties with getters and setters become JSON fields. Supported fields also show in the in-game Config tab.

```csharp
public sealed class MyFirstModConfig
{
    public bool Enabled { get; set; } = true;

    [ModKitConfigDisplay("Speed multiplier")]
    [ModKitConfigRange(0.5, 4.0, 0.1)]
    public double SpeedMultiplier { get; set; } = 1.0;
}
```

Keep `Enabled`. The SDK uses it for the overlay toggle.

## 5. Log something

Start simple:

```csharp
protected override void OnModKitInitialized()
{
    LogInfo($"{Metadata.DisplayName} initialized.");
}

protected override void OnModKitEnabled()
{
    LogInfo("Enabled.");
}
```

`OnModKitInitialized` runs once when MelonLoader loads the mod.

`OnModKitEnabled` runs only when the mod is allowed to run and `Enabled` is true.

## 6. Build

```powershell
dotnet build -c Release -p:GameRoot="C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\" -p:ModKitRoot="C:\Path\To\FL-ModKit-v0.1.0\"
```

If this fails with a missing MelonLoader or Unity DLL, check that `GameRoot` points to the folder that contains `MelonLoader\`.

## 7. Deploy

Copy both DLLs into the game's `Mods\` folder:

```text
bin\Release\net6.0\MyFirstMod.dll
FL-ModKit-v0.1.0\lib\FlashingLights.ModKit.Core.dll
```

Target folder:

```text
C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\Mods\
```

Launch the game and press `Insert`. You should see:

- Your mod listed in the Mods tab.
- Your config fields in the Config tab.
- A JSON config under `UserData\FlashingLightsModKit\`.

## What the base class gives you

`ModKitMelonMod<TConfig>` handles the shared work:

- Config load, save, reload, and reset.
- In-game UI entry.
- Enable/disable lifecycle.
- Manifest validation.
- Multiplayer guard.
- Log helpers.
- Harmony patch helpers.

You write the mod behavior. The SDK handles the scaffolding.

## Next steps

- [SDK concepts](sdk-concepts.md): short explanation of the moving parts.
- [Lifecycle](lifecycle.md): exact hook order.
- [Cookbook](cookbook.md): patching, hotkeys, logging, snapshots.
- [Troubleshooting](troubleshooting.md): common fixes.
