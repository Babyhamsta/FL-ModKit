# Config

Each managed mod gets a JSON config under:

```text
UserData\FlashingLightsModKit\<mod-id>.json
```

## With the base class

Inherit `ModKitMelonMod<TConfig>` and the SDK loads, saves, hot-reloads, and binds the config to the in-game UI for you:

```csharp
public sealed class MyMod : ModKitMelonMod<MyConfig>
{
    protected override string ModId => "myname.mymod";
    protected override bool EnableConfigHotReload => true;
}

public sealed class MyConfig
{
    public bool Enabled { get; set; } = true;

    [ModKitConfigDisplay("Spawn distance (m)")]
    [ModKitConfigRange(50, 500, 10)]
    public int SpawnDistance { get; set; } = 200;
}
```

The framework will:

1. Resolve the path to `UserData\FlashingLightsModKit\myname.mymod.json`.
2. Load the file or write defaults if missing.
3. Track the file's mtime and re-read it within `ConfigReloadInterval` when `EnableConfigHotReload` is `true`.
4. Render every supported public property in the in-game UI.
5. Call `OnConfigApplied(currentConfig)` after every change.
6. Call `OnConfigReloaded(prev, current)` on full reloads (file changed, manual reload, reset defaults).

## Without the base class

For non-MelonMod helpers that just need persistence:

```csharp
var config = ModKitConfig.LoadOrCreate("my-mod", new MyConfig(),
    info: LoggerInstance.Msg,
    warn: LoggerInstance.Warning);
```

`ModKitPaths.ConfigPath`, `ModKitPaths.LogPath`, and `ModKitPaths.EnsureParentDirectory` are useful when you need explicit paths.

## Supported UI types

The in-game editor (`ModKitConfigAdapter<TConfig>`) renders public properties with setters of these types:

- `bool`
- integer numerics (`int`, `long`, etc.)
- floating-point numerics (`float`, `double`)
- `string`
- `enum`
- `string[]`

Read-only / computed properties are shown as labels but not editable. Anything else is skipped — keep config classes flat.

## Reserved property: `Enabled`

If your config exposes `public bool Enabled { get; set; }`, the SDK treats it as the managed enabled state. The in-game toggle reads/writes this property; `IsEnabled` on the mod stays synchronized with it across saves and hot reloads.

## Decorator attributes

| Attribute | Effect |
|-----------|--------|
| `[ModKitConfigDisplay("Friendly label")]` | Replaces the property name with a friendlier label. |
| `[ModKitConfigRange(min, max, step)]` | Renders a slider on numeric properties. Slider step controls keyboard increment. |
| `[ModKitConfigOptions("a", "b", "c")]` | Renders a dropdown on `string` properties. |

Combine `Display` + `Range` for sliders with custom labels.

## Reacting to changes

Override `OnConfigApplied` to push values into runtime state. It runs for every reload path (UI edit, hot reload, manual reload, reset):

```csharp
protected override void OnConfigApplied(MyConfig currentConfig)
{
    spawnSink?.UpdateThreshold(currentConfig.SpawnDistance);
}
```

Use `OnConfigReloaded(prev, current)` only when you need to compare old vs new — for example, "skip a costly recompute unless the relevant field changed".

## Schema versioning

Add `[ModKitConfigVersion(n)]` to a config class when changing its JSON shape:

```csharp
[ModKitConfigVersion(2)]
public sealed class MyConfig
{
    public bool Enabled { get; set; } = true;
    public int SpawnDistance { get; set; } = 200;
}
```

The serializer writes `__configVersion`. During load, `ModKitMelonMod<TConfig>` calls:

```csharp
protected override MyConfig MigrateConfig(int loadedVersion, MyConfig loaded)
{
    if (loadedVersion < 2)
    {
        loaded.SpawnDistance = Math.Max(loaded.SpawnDistance, 200);
    }

    return loaded;
}
```

Unversioned config classes do not write `__configVersion`.
