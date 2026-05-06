# Discovery

Finding the IL2CPP types and live scene objects you need to patch.

## Generated assemblies

After running the game once with MelonLoader installed, the IL2CPP-managed wrapper assemblies live at:

```text
<game>\MelonLoader\Il2CppAssemblies\
```

Open these in dnSpy or ILSpy to find type names, method names, and parameter types. Public fields and properties are visible; obfuscated fields show as `XYZABCDEFG`-style identifiers. Stick to clean names where you can — obfuscated names churn between game updates.

## SDK helpers

| Helper | Use |
|--------|-----|
| `ModKitTypeResolver.ResolveFullName(string, Action<string>?)` | Resolve a type by full name across all loaded assemblies. Returns `null` and emits one warning on miss. The base mod exposes a one-liner `ResolveType("Full.Type.Name")` shortcut. |
| `ModKitTypeResolver.ResolveSuffix(string, Action<string>?)` | Resolve by class-name suffix. Diagnostics only — don't ship code that depends on suffix matches. |
| `SceneQuery.FindObjectNamesContaining(...)` | List live `GameObject` names containing a substring. Useful for finding the right hierarchy path for a UI button or vehicle root. |
| `SceneQuery.FindObjectNamesByType(...)` | List live `GameObject` names whose attached components include a given `Type`. Pair with `ModKitTypeResolver` for fully name-based discovery. |

## Recommended workflow

1. Use dnSpy / ILSpy to confirm a type exists and find its members.
2. Try the type name in a one-off `LogInfo($"resolved={ResolveType(\"Full.Type.Name\") != null}")` to confirm the loader sees it at runtime.
3. Cache the resolved type in `OnModKitInitialized` and reuse it. Don't re-resolve every frame.
4. For runtime objects, use `SceneQuery.FindObjectNamesByType` once and store references — scene scans are cheap relative to per-frame work but not free.

Prefer full names in shipping mods. Save suffix matching for exploratory work.
