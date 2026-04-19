using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;

namespace ZenTrackAgent
{
    internal sealed class ZenUsageTracker : IDisposable
    {
        private readonly AgentConfig _config;
        private readonly TrackerDatabase _database;
        private readonly Dictionary<int, SessionRecord> _activeSessions;
        private readonly object _syncRoot;
        private ManagementEventWatcher _startWatcher;
        private ManagementEventWatcher _stopWatcher;
        private bool _stopRequested;

        public ZenUsageTracker(AgentConfig config, TrackerDatabase database)
        {
            _config = config;
            _database = database;
            _activeSessions = new Dictionary<int, SessionRecord>();
            _syncRoot = new object();
        }

        public void Run(TimeSpan? maxRuntime)
        {
            Console.CancelKeyPress += OnCancelKeyPress;
            _stopRequested = false;
            DateTime? stopAtUtc = maxRuntime.HasValue ? DateTime.UtcNow.Add(maxRuntime.Value) : (DateTime?)null;

            _database.Initialize();
            _database.CloseOpenSessions(DateTime.UtcNow);
            StartWatchers();
            PollOnce();

            Log("Tracking started for process names: " + string.Join(", ", _config.ProcessNames));
            Log("Database: " + _database.DatabasePath);
            Log("Mode: event-driven tracking with " + (int)_config.PollInterval.TotalSeconds + " second reconciliation polling.");

            while (!_stopRequested)
            {
                if (stopAtUtc.HasValue && DateTime.UtcNow >= stopAtUtc.Value)
                {
                    _stopRequested = true;
                    break;
                }

                try
                {
                    PollOnce();
                }
                catch (Exception ex)
                {
                    Log("Poll failure: " + ex.Message);
                }

                SleepWithStopCheck(_config.PollInterval);
            }

            FlushActiveSessions();
            StopWatchers();
            Log("Tracking stopped.");
        }

        private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _stopRequested = true;
        }

        private void PollOnce()
        {
            DateTime observedAtUtc = DateTime.UtcNow;
            Dictionary<int, ProcessSnapshot> runningTargets = ProcessSnapshot.Capture(_config.ProcessNames);

            foreach (KeyValuePair<int, ProcessSnapshot> entry in runningTargets)
            {
                TryOpenSession(entry.Value, observedAtUtc);
            }

            List<int> endedProcessIds;
            lock (_syncRoot)
            {
                endedProcessIds = _activeSessions.Keys
                    .Where(processId => !runningTargets.ContainsKey(processId))
                    .ToList();
            }

            foreach (int processId in endedProcessIds)
            {
                TryCloseSession(processId, observedAtUtc);
            }
        }

        private void FlushActiveSessions()
        {
            DateTime stopTimeUtc = DateTime.UtcNow;
            List<SessionRecord> sessionsToClose;
            lock (_syncRoot)
            {
                sessionsToClose = _activeSessions.Values.ToList();
                _activeSessions.Clear();
            }

            foreach (SessionRecord session in sessionsToClose)
            {
                session.EndedAtUtc = stopTimeUtc;
                _database.CloseSession(session);
                Log("Session flushed on shutdown. PID=" + session.ProcessId + ", Process=" + session.ProcessName + ", EndUtc=" + session.EndedAtUtc.Value.ToString("o"));
            }
        }

        private void StartWatchers()
        {
            string filter = string.Join(" OR ", _config.ProcessNames.Select(
                delegate(string processName)
                {
                    string normalized = AgentConfig.NormalizeProcessName(processName) + ".exe";
                    return "ProcessName = '" + normalized.Replace("'", "''") + "'";
                }));

            _startWatcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace WHERE " + filter));
            _stopWatcher = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace WHERE " + filter));

            _startWatcher.EventArrived += OnProcessStarted;
            _stopWatcher.EventArrived += OnProcessStopped;

            _startWatcher.Start();
            _stopWatcher.Start();
        }

        private void StopWatchers()
        {
            if (_startWatcher != null)
            {
                _startWatcher.EventArrived -= OnProcessStarted;
                _startWatcher.Stop();
                _startWatcher.Dispose();
                _startWatcher = null;
            }

            if (_stopWatcher != null)
            {
                _stopWatcher.EventArrived -= OnProcessStopped;
                _stopWatcher.Stop();
                _stopWatcher.Dispose();
                _stopWatcher = null;
            }
        }

        private void OnProcessStarted(object sender, EventArrivedEventArgs e)
        {
            try
            {
                int processId = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);
                string processName = Convert.ToString(e.NewEvent.Properties["ProcessName"].Value);
                ProcessSnapshot snapshot = ProcessSnapshot.FromProcessId(processId, processName);
                if (snapshot != null)
                {
                    TryOpenSession(snapshot, DateTime.UtcNow);
                }
            }
            catch (Exception ex)
            {
                Log("Process start event failure: " + ex.Message);
            }
        }

        private void OnProcessStopped(object sender, EventArrivedEventArgs e)
        {
            try
            {
                int processId = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);
                TryCloseSession(processId, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                Log("Process stop event failure: " + ex.Message);
            }
        }

        private void TryOpenSession(ProcessSnapshot snapshot, DateTime observedAtUtc)
        {
            lock (_syncRoot)
            {
                if (_activeSessions.ContainsKey(snapshot.ProcessId))
                {
                    return;
                }

                DateTime startTimeUtc = snapshot.StartTimeUtc ?? observedAtUtc;
                SessionRecord session = new SessionRecord
                {
                    ProcessId = snapshot.ProcessId,
                    ProcessName = snapshot.ProcessName,
                    MachineName = Environment.MachineName,
                    WindowsUserName = Environment.UserName,
                    StartedAtUtc = startTimeUtc
                };

                session.SessionId = _database.InsertSession(session);
                _activeSessions[session.ProcessId] = session;
                Log("Session started. PID=" + session.ProcessId + ", Process=" + session.ProcessName + ", StartUtc=" + session.StartedAtUtc.ToString("o"));
            }
        }

        private void TryCloseSession(int processId, DateTime observedAtUtc)
        {
            SessionRecord session;
            lock (_syncRoot)
            {
                if (!_activeSessions.TryGetValue(processId, out session))
                {
                    return;
                }

                _activeSessions.Remove(processId);
            }

            session.EndedAtUtc = observedAtUtc;
            _database.CloseSession(session);
            Log("Session ended. PID=" + session.ProcessId + ", Process=" + session.ProcessName + ", EndUtc=" + session.EndedAtUtc.Value.ToString("o"));
        }

        private void SleepWithStopCheck(TimeSpan interval)
        {
            int remainingMilliseconds = (int)interval.TotalMilliseconds;
            while (remainingMilliseconds > 0 && !_stopRequested)
            {
                int currentDelay = Math.Min(remainingMilliseconds, 500);
                Thread.Sleep(currentDelay);
                remainingMilliseconds -= currentDelay;
            }
        }

        private void Log(string message)
        {
            if (_config.LogToConsole)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message);
            }
        }

        public void Dispose()
        {
            StopWatchers();
            Console.CancelKeyPress -= OnCancelKeyPress;
        }
    }
}
