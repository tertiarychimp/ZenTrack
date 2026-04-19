@echo off
setlocal

set "PORT=8787"
if not "%~1"=="" set "PORT=%~1"

set "ROOT=%~dp0"
if "%ROOT:~-1%"=="\" set "ROOT=%ROOT:~0,-1%"
set "RELEASE_DIR=%ROOT%\artifacts\Release"
set "EXE_PATH=%RELEASE_DIR%\ZenTrackAgent.exe"
set "ADMIN_URL=http://localhost:%PORT%/admin"
set "LOCAL_URL=http://localhost:%PORT%/"

if not exist "%EXE_PATH%" (
  echo Build output not found at "%EXE_PATH%".
  echo Run build.ps1 first.
  exit /b 1
)

echo Checking for an existing ZenTrack server on port %PORT%...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$connection = Get-NetTCPConnection -LocalPort %PORT% -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1; if ($connection) { try { Stop-Process -Id $connection.OwningProcess -Force -ErrorAction Stop; Write-Host ('Stopped process ' + $connection.OwningProcess + ' on port %PORT%.') } catch { Write-Host ('Could not stop process on port %PORT%: ' + $_.Exception.Message) } }"

echo Starting ZenTrack server on port %PORT%...
start "" "%EXE_PATH%" --serve-ui --port=%PORT%

echo Waiting for server startup...
timeout /t 2 /nobreak >nul

set "CHROME_PATH="
if exist "%ProgramFiles%\Google\Chrome\Application\chrome.exe" set "CHROME_PATH=%ProgramFiles%\Google\Chrome\Application\chrome.exe"
if not defined CHROME_PATH if exist "%ProgramFiles(x86)%\Google\Chrome\Application\chrome.exe" set "CHROME_PATH=%ProgramFiles(x86)%\Google\Chrome\Application\chrome.exe"
if not defined CHROME_PATH if exist "%LOCALAPPDATA%\Google\Chrome\Application\chrome.exe" set "CHROME_PATH=%LOCALAPPDATA%\Google\Chrome\Application\chrome.exe"

if not defined CHROME_PATH (
  echo Chrome was not found in the standard install locations.
  echo Admin URL: %ADMIN_URL%
  echo Local URL: %LOCAL_URL%
  exit /b 1
)

echo Opening Chrome tabs...
start "" "%CHROME_PATH%" "%ADMIN_URL%" "%LOCAL_URL%"

echo ZenTrack admin and local pages launched.
exit /b 0
