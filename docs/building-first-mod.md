# Build a First Mod

The fastest path:

1. Extract `FL-ModKit-v0.1.0-sdk.zip` somewhere outside the SDK source tree.
2. Copy `templates\BasicMelonMod` to your own folder.
3. Rename the folder, the project file (`BasicMelonMod.csproj`), the namespace inside the `.cs` files, and the assembly metadata in `Properties\AssemblyInfo.cs`.
4. Edit `BasicMod.cs`. The starter is already a working mod — replace the body with your logic. Centralize metadata with the `[ModKitManifest]` attribute when you're ready.
5. Build with `GameRoot` pointing at the folder that contains `MelonLoader\` (the Steam install for most people):

   ```powershell
   dotnet build -c Release `
     -p:GameRoot="C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\" `
     -p:ModKitRoot="C:\Path\To\FL-ModKit-v0.1.0\"
   ```

6. Copy your mod DLL plus `FlashingLights.ModKit.Core.dll` (from the SDK's `lib\` folder) into:

   ```text
   C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\Mods\
   ```

7. Launch the game. Press `Insert` to open the ModKit overlay. Your mod is in the Mods tab with a live config editor.

## What the base class gives you

`BasicMod` inherits `ModKitMelonMod<BasicConfig>`. You get:

- A managed config under `UserData\FlashingLightsModKit\<mod-id>.json`.
- An entry in the in-game UI with toggle, config editor, and metadata display.
- Sealed lifecycle hooks: `OnModKitInitialized`, `OnModKitEnabled`, `OnModKitDisabled`, `OnModKitUpdate`, `OnModKitGui`, `OnModKitSceneWasLoaded`, `OnConfigApplied`, `OnConfigReloaded`. See [lifecycle.md](lifecycle.md) for ordering.
- Multiplayer guard installed automatically.
- Manifest validation from `[ModKitManifest]` (or virtual-property fallbacks) with dependency and SDK-version checking.
- `LogInfo` / `LogWarning` for the MelonLoader console.
- `PatchPrefix` / `PatchPostfix` shortcuts for safe Harmony hooks.

## Customize the metadata

Add `[ModKitManifest]` on the class for a single-place declaration:

```csharp
[ModKitManifest(
    Id = "myname.mymod",
    DisplayName = "My Mod",
    Version = "0.1.0",
    Author = "Babyhamsta",
    License = "MIT",
    GitHubUrl = "https://github.com/Babyhamsta/FL-ModKit",
    Changelog = "Initial public release.",
    MinSdkVersion = "0.1.0")]
public sealed class MyMod : ModKitMelonMod<MyConfig>
{
    protected override string ModId => "myname.mymod";
}
```

`ModId` still has to come from a property override (the framework needs it before metadata is resolved, to construct the config path). Everything else can live in the attribute.

## Add a config option

Public property with a setter, plus an optional decorator attribute for nicer rendering:

```csharp
public sealed class MyConfig
{
    public bool Enabled { get; set; } = true;

    [ModKitConfigDisplay("Spawn distance (m)")]
    [ModKitConfigRange(50, 500, 10)]
    public int SpawnDistance { get; set; } = 200;
}
```

Editable types: `bool`, integer numerics, floating-point numerics, `string`, enums, `string[]`.

## React to changes

```csharp
protected override void OnConfigApplied(MyConfig currentConfig)
{
    spawnDistance = currentConfig.SpawnDistance;
}
```

`OnConfigApplied` runs for every config change — UI edit, hot reload, or reset. Use it as the single funnel for "push the new config into runtime state."

## Next steps

- [Cookbook](cookbook.md) — patch a method, register a hotkey, write to a per-mod log file.
- [API reference](api-reference.md) — every public type and member.
- [Debugging](debugging.md) — when things don't work.
