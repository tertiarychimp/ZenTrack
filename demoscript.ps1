param(
    [int]$Port = 8787
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$releaseDir = Join-Path $root "artifacts\Release"
$exePath = Join-Path $releaseDir "ZenTrackAgent.exe"
$url = "http://localhost:$Port/"

if (-not (Test-Path $exePath)) {
    Write-Host "Build output not found. Running build.ps1 first..."
    & (Join-Path $root "build.ps1")
}

$serverRunning = $false
try {
    $probe = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 3
    if ($probe.StatusCode -eq 200) {
        $serverRunning = $true
    }
}
catch {
    $serverRunning = $false
}

if (-not $serverRunning) {
    Write-Host "Starting ZenTrack local web server on $url"
    Start-Process -FilePath $exePath -ArgumentList "--serve-ui", "--port=$Port" -WorkingDirectory $releaseDir | Out-Null
    Start-Sleep -Seconds 2
}
else {
    Write-Host "ZenTrack local web server is already running on port $Port"
}

$chromeCandidates = @(
    "$env:ProgramFiles\Google\Chrome\Application\chrome.exe",
    "${env:ProgramFiles(x86)}\Google\Chrome\Application\chrome.exe",
    "$env:LOCALAPPDATA\Google\Chrome\Application\chrome.exe"
)

$chromePath = $chromeCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $chromePath) {
    throw "Chrome was not found in the standard install locations."
}

Write-Host "Opening Chrome at $url"
Start-Process -FilePath $chromePath -ArgumentList $url | Out-Null
