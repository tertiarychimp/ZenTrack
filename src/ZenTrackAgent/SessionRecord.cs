using System;

namespace ZenTrackAgent
{
    internal sealed class SessionRecord
    {
        public long SessionId { get; set; }

        public int ProcessId { get; set; }

        public string ProcessName { get; set; }

        public string MachineName { get; set; }

        public string WindowsUserName { get; set; }

        public DateTime StartedAtUtc { get; set; }

        public DateTime? EndedAtUtc { get; set; }
    }
}
