# Getting Started

This page is for mod authors who want to create a mod.

You do not need to build the SDK source first. Start from a packaged FL-ModKit release.

## What you need

- Flashing Lights on PC.
- MelonLoader v0.7.0 or newer installed for the game.
- .NET SDK that can build `net6.0` projects.
- `FL-ModKit-v0.1.0-sdk.zip` from the GitHub or Nexus release.

## The short version

1. Extract `FL-ModKit-v0.1.0-sdk.zip`.
2. Copy `templates\BasicMelonMod` into a new folder for your mod.
3. Rename the project, namespace, mod id, and display name.
4. Build with `GameRoot` pointing at the game folder.
5. Copy your mod DLL and `lib\FlashingLights.ModKit.Core.dll` into the game's `Mods\` folder.
6. Launch the game and press `Insert`.

## Folder example

After extracting the SDK zip, you should see:

```text
FL-ModKit-v0.1.0\
  lib\
    FlashingLights.ModKit.Core.dll
  templates\
    BasicMelonMod\
    ModTests\
  docs\
```

Copy the template somewhere outside the extracted SDK folder:

```text
C:\Users\Admin\Documents\FL_Mods\MyFirstMod\
```

## First build command

From your copied mod folder:

```powershell
dotnet build -c Release -p:GameRoot="C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\" -p:ModKitRoot="C:\Path\To\FL-ModKit-v0.1.0\"
```

`GameRoot` is the folder that contains `MelonLoader\`.

`ModKitRoot` is the extracted SDK folder that contains `lib\`.

## Deploy test

Copy your built mod DLL plus `FlashingLights.ModKit.Core.dll` into:

```text
C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\Mods\
```

Launch the game. Press `Insert` once you are past the title screen. Your mod should appear in the overlay.

The config file appears here after the mod loads:

```text
C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\UserData\FlashingLightsModKit\myname.firstmod.json
```

## Learn next

- [Build a first mod](building-first-mod.md): exact rename and edit checklist.
- [SDK concepts](sdk-concepts.md): what each SDK part does.
- [Troubleshooting](troubleshooting.md): common build and load errors.
- [Cookbook](cookbook.md): copyable recipes once the first mod loads.
