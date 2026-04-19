using System.Runtime.Serialization;

namespace ZenTrackAgent
{
    [DataContract]
    internal sealed class AdminUsageSessionListItem
    {
        [DataMember(Name = "machineName")]
        public string MachineName { get; set; }

        [DataMember(Name = "windowsUsername")]
        public string WindowsUsername { get; set; }

        [DataMember(Name = "groupName")]
        public string GroupName { get; set; }

        [DataMember(Name = "processName")]
        public string ProcessName { get; set; }

        [DataMember(Name = "processId")]
        public int ProcessId { get; set; }

        [DataMember(Name = "startedAtUtc")]
        public string StartedAtUtc { get; set; }

        [DataMember(Name = "endedAtUtc")]
        public string EndedAtUtc { get; set; }

        [DataMember(Name = "durationSeconds")]
        public int? DurationSeconds { get; set; }

        [DataMember(Name = "isSynced")]
        public bool IsSynced { get; set; }
    }
}
