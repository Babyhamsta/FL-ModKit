[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$Version = "0.1.0",
    [string]$OutputRoot,
    [string]$GameRoot
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot "artifacts"
}

$packageRoot = Join-Path $OutputRoot "sdk\FL-ModKit-v$Version"
$zipPath = Join-Path $OutputRoot "FL-ModKit-v$Version-sdk.zip"
$coreProject = Join-Path $repoRoot "src\FlashingLights.ModKit.Core\FlashingLights.ModKit.Core.csproj"
$coreDll = Join-Path $repoRoot "src\FlashingLights.ModKit.Core\bin\$Configuration\net6.0\FlashingLights.ModKit.Core.dll"

if ([string]::IsNullOrWhiteSpace($GameRoot)) {
    $candidates = @(
        (Resolve-Path (Join-Path $repoRoot "..") -ErrorAction SilentlyContinue),
        "C:\Program Files (x86)\Steam\steamapps\common\Flashing Lights\"
    )

    foreach ($candidate in $candidates) {
        if ($null -eq $candidate) {
            continue
        }

        $candidateText = [string]$candidate
        if (Test-Path (Join-Path $candidateText "MelonLoader\Il2CppAssemblies\UnityEngine.CoreModule.dll")) {
            $GameRoot = $candidateText
            break
        }
    }
}

if ([string]::IsNullOrWhiteSpace($GameRoot)) {
    throw "GameRoot was not supplied and MelonLoader assemblies could not be found."
}

if (!$GameRoot.EndsWith("\") -and !$GameRoot.EndsWith("/")) {
    $GameRoot += "\"
}

$GameRoot = $GameRoot.Replace("\", "/")
dotnet build $coreProject -c $Configuration "-p:GameRoot=$GameRoot"
if ($LASTEXITCODE -ne 0) {
    throw "Core build failed with exit code $LASTEXITCODE."
}

Remove-Item -LiteralPath $packageRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path (Join-Path $packageRoot "lib") | Out-Null

Copy-Item -LiteralPath $coreDll -Destination (Join-Path $packageRoot "lib\FlashingLights.ModKit.Core.dll") -Force
Copy-Item -LiteralPath (Join-Path $repoRoot "templates") -Destination $packageRoot -Recurse -Force
Copy-Item -LiteralPath (Join-Path $repoRoot "docs") -Destination $packageRoot -Recurse -Force
Copy-Item -LiteralPath (Join-Path $repoRoot "README.md") -Destination $packageRoot -Force
Copy-Item -LiteralPath (Join-Path $repoRoot "LICENSE") -Destination $packageRoot -Force
Copy-Item -LiteralPath (Join-Path $repoRoot "CHANGELOG.md") -Destination $packageRoot -Force

Get-ChildItem -LiteralPath $packageRoot -Directory -Recurse |
    Where-Object { $_.Name -in @("bin", "obj", ".vs", ".git") } |
    Remove-Item -Recurse -Force

Remove-Item -LiteralPath $zipPath -Force -ErrorAction SilentlyContinue
Compress-Archive -Path (Join-Path $packageRoot "*") -DestinationPath $zipPath -Force

Write-Host "SDK package root: $packageRoot"
Write-Host "SDK zip: $zipPath"
