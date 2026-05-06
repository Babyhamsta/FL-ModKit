# Install FL-ModKit

Use this page if you only want to run FL-ModKit-based mods.

## Requirements

- Flashing Lights on PC.
- MelonLoader v0.7.0 or newer installed for the game.
- An FL-ModKit release package.
- One or more mods that were built for FL-ModKit.

## Install

1. Download the FL-ModKit package.
2. Extract `FlashingLights.ModKit.Core.dll` from the package's `lib\` folder.
3. Copy it into the game `Mods\` folder:

   ```text
   C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\Mods\
   ```

4. Copy any FL-ModKit-based mod DLLs into the same `Mods\` folder.
5. Launch the game.
6. Press `Insert` after the game reaches the menu or a scene.

## Check that it loaded

Open MelonLoader's latest log:

```text
C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\MelonLoader\Latest.log
```

Look for the FL-ModKit-based mod name and a line saying the mod initialized.

## Configs and logs

FL-ModKit stores per-mod files here:

```text
C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\UserData\FlashingLightsModKit\
```

Each mod gets its own JSON config file. Mods that use file logging also create a `.log` file there.

## Common mistakes

- `FlashingLights.ModKit.Core.dll` must be in `Mods\`, not only inside the SDK zip.
- Mod DLLs must be in the same `Mods\` folder.
- MelonLoader must be v0.7.0 or newer.
- The `Insert` overlay only appears when at least one FL-ModKit-managed mod loaded.
