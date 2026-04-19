param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$sourceRoot = Join-Path $projectRoot "src\\ZenTrackAgent"
$outputRoot = Join-Path $projectRoot "artifacts\\$Configuration"
$compiler = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if (-not (Test-Path $compiler)) {
    throw "C# compiler not found at $compiler"
}

New-Item -ItemType Directory -Force -Path $outputRoot | Out-Null

$sources = Get-ChildItem -Path $sourceRoot -Filter *.cs | ForEach-Object { $_.FullName }

& $compiler `
    /nologo `
    /target:exe `
    /out:"$outputRoot\\ZenTrackAgent.exe" `
    /platform:x64 `
    /optimize+ `
    /debug- `
    /r:System.Management.dll `
    /r:System.Runtime.Serialization.dll `
    $sources

if ($LASTEXITCODE -ne 0) {
    throw "Compilation failed with exit code $LASTEXITCODE"
}

Copy-Item (Join-Path $sourceRoot "ZenTrackAgent.config.json") (Join-Path $outputRoot "ZenTrackAgent.config.json") -Force

Write-Host "Build completed:"
Write-Host "  Executable: $outputRoot\\ZenTrackAgent.exe"
Write-Host "  Config:     $outputRoot\\ZenTrackAgent.config.json"
