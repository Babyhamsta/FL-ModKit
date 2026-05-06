# Mod Tests Template

Minimal console-app test harness for mods that consume the packaged SDK.

From this folder:

```powershell
dotnet run -c Release `
  -p:GameRoot="C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\" `
  -p:ModKitRoot="C:\Path\To\FL-ModKit-v0.1.0\"
```

The harness exits with code `0` when every test passes and `1` when any test fails.
