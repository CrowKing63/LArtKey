param(
    [string]$Version = "0.1.0",
    [string]$BuildDir = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$Version = $Version.TrimStart('v')
$out = Join-Path $root "dist/LArtKey-Portable-v$Version"

New-Item -ItemType Directory -Force $out | Out-Null

if ($BuildDir -and (Test-Path $BuildDir)) {
    Write-Host "Copying pre-built files from $BuildDir" -ForegroundColor Cyan
    Copy-Item -Recurse "$BuildDir/*" "$out/" -Force
} else {
    Write-Host "Building portable single-file package" -ForegroundColor Cyan
    Push-Location (Join-Path $root "LArtKey")
    dotnet publish `
        -c Release `
        -r win-x64 `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:PublishReadyToRun=true `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:EnableCompressionInSingleFile=true `
        -o $out
    Pop-Location
}

$layoutsSrc = Join-Path $root "LArtKey/layouts"
if (Test-Path $layoutsSrc) {
    Copy-Item -Recurse $layoutsSrc "$out/layouts" -Force
}

$defaultConfig = @{
    version = "1.0.0"
    default_layout = "Basic"
} | ConvertTo-Json -Depth 2
$defaultConfig | Out-File -Encoding UTF8 "$out/config.json"

$zipPath = Join-Path $root "dist/LArtKey-Portable-v$Version.zip"
Write-Host "Compressing to $zipPath" -ForegroundColor Cyan
Compress-Archive -Path "$out/*" -DestinationPath $zipPath -Force
Write-Host "Created dist/LArtKey-Portable-v$Version.zip" -ForegroundColor Green