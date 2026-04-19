using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ZenTrackAgent
{
    internal sealed class ProcessSnapshot
    {
        public int ProcessId { get; private set; }

        public string ProcessName { get; private set; }

        public DateTime? StartTimeUtc { get; private set; }

        public static Dictionary<int, ProcessSnapshot> Capture(ICollection<string> targetProcessNames)
        {
            HashSet<string> normalizedTargets = new HashSet<string>(targetProcessNames, StringComparer.OrdinalIgnoreCase);
            Dictionary<int, ProcessSnapshot> processes = new Dictionary<int, ProcessSnapshot>();

            foreach (Process process in Process.GetProcesses())
            {
                using (process)
                {
                    string processName;
                    try
                    {
                        processName = AgentConfig.NormalizeProcessName(process.ProcessName);
                    }
                    catch
                    {
                        continue;
                    }

                    if (!normalizedTargets.Contains(processName))
                    {
                        continue;
                    }

                    DateTime? startTimeUtc = null;
                    try
                    {
                        startTimeUtc = process.StartTime.ToUniversalTime();
                    }
                    catch
                    {
                        startTimeUtc = null;
                    }

                    processes[process.Id] = new ProcessSnapshot
                    {
                        ProcessId = process.Id,
                        ProcessName = processName,
                        StartTimeUtc = startTimeUtc
                    };
                }
            }

            return processes;
        }

        public static ProcessSnapshot FromProcessId(int processId, string processName)
        {
            try
            {
                using (Process process = Process.GetProcessById(processId))
                {
                    return new ProcessSnapshot
                    {
                        ProcessId = processId,
                        ProcessName = AgentConfig.NormalizeProcessName(string.IsNullOrWhiteSpace(processName) ? process.ProcessName : processName),
                        StartTimeUtc = process.StartTime.ToUniversalTime()
                    };
                }
            }
            catch
            {
                return new ProcessSnapshot
                {
                    ProcessId = processId,
                    ProcessName = AgentConfig.NormalizeProcessName(processName ?? string.Empty),
                    StartTimeUtc = DateTime.UtcNow
                };
            }
        }
    }
}
