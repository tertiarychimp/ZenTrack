# ZenTrack

This application monitors configured ZEISS ZEN process names on a Windows machine, stores usage sessions in a local SQLite database, and can now serve both a local HTML viewer and an administrator-oriented reporting interface for the tracking records.

## What it records

- Windows login name
- Machine name
- Process name
- Process ID
- Session start time in UTC
- Session end time in UTC
- Session duration in seconds
- Sync status placeholder for later MySQL synchronization

## Current design

- `src/ZenTrackAgent` contains the C# source code.
- `build.ps1` compiles the app with the local .NET Framework C# compiler already present on Windows.
- `artifacts/Release` is the default build output folder.
- `ZenTrackAgent.config.json` controls the monitored process names and local database path.
- `demoscript.ps1` starts the local web server and opens the tracking page in Chrome.

## Configuration

Edit `src/ZenTrackAgent/ZenTrackAgent.config.json` before building or copy the generated file beside the executable and update it there.

```json
{
  "databasePath": "%LOCALAPPDATA%\\ZenTrack\\data\\zentrack.db",
  "pollIntervalSeconds": 60,
  "processNames": [
    "ZEN.exe"
  ],
  "logToConsole": true
}
```

Notes:

- Process names can be given with or without `.exe`.
- The tracker is currently configured to monitor only `ZEN.exe`.
- The default database path uses the current Windows user's local application data folder.
- The tracker stores timestamps in UTC so later reporting and synchronization stay consistent.
- The tracker uses Windows process start/stop events and keeps the poll interval as a fallback reconciliation check.
- The default fallback reconciliation interval is 60 seconds to keep background resource use low.

## Build

Run:

```powershell
.\build.ps1
```

## Run

Run:

```powershell
.\artifacts\Release\ZenTrackAgent.exe
```

Press `Ctrl+C` to stop it. Active sessions are closed cleanly on shutdown.

For a short smoke test, you can run:

```powershell
.\artifacts\Release\ZenTrackAgent.exe --run-for-seconds=10
```

## Local Web Viewer

Run the server directly:

```powershell
.\artifacts\Release\ZenTrackAgent.exe --serve-ui
```

Then open:

```text
http://localhost:8787/
```

The viewer:

- shows paged `usage_sessions` results
- orders the newest sessions first
- displays local date and time in separate columns
- provides Previous and Next paging buttons
- supports page sizes of 10, 20, 50, and 100 rows

## Administrator Reporting Interface

Run the same server:

```powershell
.\artifacts\Release\ZenTrackAgent.exe --serve-ui
```

Then open:

```text
http://localhost:8787/admin
```

The administrator interface:

- uses a blue reporting theme intended for the future master SQL rollout
- filters by user, group, start date, and end date
- stores group administration in a separate SQLite `GroupDB.db` database created in the ZenTrack data folder
- shows group-level summary totals above the detailed session list, including billing references
- allows groups to be added directly from the Group Reporting section
- links each group name to its own properties page for billing reference and unassigned-user assignment
- links to `/admin/settings` for the future HTML-based settings workflow
- reserves space for future PDF billing exports

## MySQL Master Schema

The first-pass MySQL schema for the future central reporting database lives here:

```text
database/mysql/master_schema.sql
```

It defines tables for:

- users
- groups and dated group memberships
- machines
- master usage sessions collected from multiple systems
- agent setting profiles and assignments
- future billing report metadata

## GroupDB Schema

The separate local SQLite schema for group administration lives here:

```text
database/sqlite/groupdb_schema.sql
```

It stores:

- group names
- group billing references
- user-to-group membership assignments for reporting

## Demo Script

To start the local server and launch Chrome to the viewer page:

```powershell
.\demoscript.ps1
```

Optional custom port:

```powershell
.\demoscript.ps1 -Port 8788
```

## SQLite table

The app creates a table named `usage_sessions` with these columns:

- `session_id`
- `machine_name`
- `windows_username`
- `process_name`
- `process_id`
- `started_at_utc`
- `ended_at_utc`
- `duration_seconds`
- `is_synced`
- `created_at_utc`
- `updated_at_utc`

## Next recommended steps

1. Add the MySQL sync worker and a unique machine registration strategy.
2. Wire `/admin/settings` to live configuration persistence.
3. Add group-member removal and broader group lifecycle tools.
4. Add PDF billing export generation from the admin reporting view.
