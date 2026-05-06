# Basic Melon Mod

Copy this folder, rename the project and namespace, then build it against a packaged ModKit SDK.

Build from this folder:

```powershell
dotnet build -c Release -p:GameRoot="C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\" -p:ModKitRoot="C:\Path\To\FL-ModKit-v0.1.0\"
```

Deploy `bin\Release\net6.0\BasicMelonMod.dll` to the game's `Mods` folder with `FlashingLights.ModKit.Core.dll`.
