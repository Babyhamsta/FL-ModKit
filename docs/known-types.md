# Known Types

Game-specific vehicle and traffic types were observed from `MelonLoader/Il2CppAssemblies/Assembly-CSharp.dll` in the local Flashing Lights install.
Unity and Addressables types were observed from generated Unity/ResourceManager assemblies or catalog/runtime context.

## Vehicle And Traffic Candidates

| Type | Likely Use |
| --- | --- |
| `Il2CppOmniVehicleAi.AIVehicleController` | AI vehicle input, path following, obstacle logic |
| `Il2CppOmniVehicleAi.PathProgressTracker` | AI path progress and target movement |
| `Il2CppOmniVehicleAi.ContextSteering` | Obstacle/context steering |
| `Il2CppEVP.VehicleController` | Vehicle physics controller from EVP |
| `Il2CppEVP.VehicleManager` | EVP vehicle manager |
| `Il2Cpp.CarController` | Simple custom car controller |
| `Il2CppHutongGames.PlayMaker.Actions.SetCarWaypoint` | PlayMaker action for assigning car waypoint |
| `Il2CppHutongGames.PlayMaker.Actions.LeanPoolSpawnV2` | PlayMaker pool spawn action |
| `Il2CppJinxterGames.Scripts.FlashingLights.CarTypes` | Vehicle type enum |

## Visual And Asset Candidates

| Type | Likely Use |
| --- | --- |
| `UnityEngine.Texture2D` | Texture assets |
| `UnityEngine.Material` | Material assets |
| `UnityEngine.Shader` | Shader assets |
| `UnityEngine.ResourceManagement.ResourceProviders.AssetBundleProvider` | Addressables bundle loading |
| `UnityEngine.ResourceManagement.ResourceProviders.BundledAssetProvider` | Addressables asset loading |

## Addressables Location

```text
../flashinglights_Data/StreamingAssets/aa/StandaloneWindows64
```

The local install contains thousands of `.bundle` files. Texture replacement work should begin with catalog mapping before editing bundles.
