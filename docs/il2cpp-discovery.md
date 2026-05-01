# IL2CPP Discovery

Flashing Lights uses Unity IL2CPP. MelonLoader generates managed interop assemblies under:

```text
../MelonLoader/Il2CppAssemblies
```

Main game assembly:

```text
../MelonLoader/Il2CppAssemblies/Assembly-CSharp.dll
```

## Inspect Type Names

Use Mono.Cecil from MelonLoader to inspect generated assemblies without loading them as normal .NET assemblies.

```powershell
[Reflection.Assembly]::LoadFrom((Resolve-Path '../MelonLoader/net35/Mono.Cecil.dll')) | Out-Null
$asm = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Resolve-Path '../MelonLoader/Il2CppAssemblies/Assembly-CSharp.dll'))
$asm.MainModule.Types |
  Where-Object { $_.FullName -match 'Traffic|Vehicle|Car|Spawn|Speed|Waypoint' } |
  Select-Object -ExpandProperty FullName |
  Sort-Object -Unique
```

## Inspect Fields And Methods

```powershell
[Reflection.Assembly]::LoadFrom((Resolve-Path '../MelonLoader/net35/Mono.Cecil.dll')) | Out-Null
$asm = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Resolve-Path '../MelonLoader/Il2CppAssemblies/Assembly-CSharp.dll'))
$type = $asm.MainModule.Types | Where-Object { $_.FullName -eq 'Il2CppOmniVehicleAi.AIVehicleController' }
$type.Fields | ForEach-Object { "$($_.FieldType.Name) $($_.Name)" }
$type.Methods | ForEach-Object { "$($_.ReturnType.Name) $($_.Name)" }
```

## Runtime Discovery

Use `TypeResolver.ResolveFullName` to find loaded game types. Use `SceneQuery.FindObjectNamesByType` after a scene loads to list active Unity objects of a discovered type.
