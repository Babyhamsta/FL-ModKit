# Packaging

Run:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\package-sdk.ps1 -Configuration Release
```

If the script cannot find the game install, pass `GameRoot`:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\package-sdk.ps1 -Configuration Release -GameRoot "C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\"
```

Output:

```text
artifacts\FL-ModKit-v0.1.0-sdk.zip
artifacts\sdk\FL-ModKit-v0.1.0\
```

Package contents:

```text
lib\FlashingLights.ModKit.Core.dll
templates\BasicMelonMod\
templates\ModTests\
docs\
README.md
LICENSE
CHANGELOG.md
```

No gameplay mods are copied into SDK packages.
